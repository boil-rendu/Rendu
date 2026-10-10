using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using RD.Core;

namespace RD
{
    /// <summary>
    /// 动态编译器 - 负责编译生成的配置代码
    /// </summary>
    public class AssemblyCompiler
    {
        private readonly string solutionPath;

        public AssemblyCompiler(string solutionPath)
        {
            this.solutionPath = solutionPath;
        }

        public Assembly Compile(string classPath, ConfigType configType)
        {
            Console.WriteLine($"[AssemblyCompiler] 编译 {configType}: {classPath}");

            var syntaxTrees = CollectSyntaxTrees(classPath);
            var references = CollectReferences();

            var compilation = CreateCompilation(syntaxTrees, references);
            var assembly = EmitAndLoadAssembly(compilation);

            Console.WriteLine($"[AssemblyCompiler] {configType} 编译成功");
            return assembly;
        }

        private List<SyntaxTree> CollectSyntaxTrees(string classPath)
        {
            var syntaxTrees = new List<SyntaxTree>();
            foreach (string classFile in FileHelper.GetAllFiles(classPath, "*.cs"))
            {
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(classFile)));
            }

            Console.WriteLine($"[AssemblyCompiler] 共 {syntaxTrees.Count} 个文件");
            return syntaxTrees;
        }

        private List<PortableExecutableReference> CollectReferences()
        {
            var references = new List<PortableExecutableReference>();

            // 添加已加载的程序集
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
                        continue;

                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
                catch
                {
                    // 忽略无法访问的程序集
                }
            }

            // 添加必要的程序集
            AddRequiredReferences(references);

            Console.WriteLine($"[AssemblyCompiler] 共 {references.Count} 个程序集引用");
            return references;
        }

        private void AddRequiredReferences(List<PortableExecutableReference> references)
        {
            // ---- runtime 核心引用（mscorlib 等价物） ----
            // .NET 5+ host 里 netstandard.dll 是 facade，不一定出现在
            // AppDomain.GetAssemblies() 列表里（或 Location 为空被原过滤跳过），
            // Roslyn 解析不到 System.Attribute 等基元时即报：
            //   "The type 'Attribute' is defined in an assembly that is not referenced.
            //    You must add a reference to assembly 'netstandard, Version=2.0.0.0'."
            // 显式补 typeof(object).Assembly（System.Private.CoreLib）即可。
            string corePath = typeof(object).Assembly.Location;
            if (!string.IsNullOrEmpty(corePath) && !ContainsFile(references, corePath))
            {
                references.Add(MetadataReference.CreateFromFile(corePath));
                Console.WriteLine($"[AssemblyCompiler] 添加引用: {corePath} (mscorlib)");
            }

            // ---- netstandard facade（.NET 5+ 才有，与 mscorlib 同目录） ----
            if (!string.IsNullOrEmpty(corePath))
            {
                string netStdPath = Path.Combine(
                    Path.GetDirectoryName(corePath) ?? string.Empty, "netstandard.dll");
                if (File.Exists(netStdPath) && !ContainsFile(references, netStdPath))
                {
                    references.Add(MetadataReference.CreateFromFile(netStdPath));
                    Console.WriteLine($"[AssemblyCompiler] 添加引用: {netStdPath} (netstandard)");
                }
            }

            // ---- RD.SourceGeneratorAttribute（原有） ----
            string sourceGenAttrPath = FindAssembly("RD.SourceGeneratorAttribute.dll", solutionPath);
            if (sourceGenAttrPath != null)
            {
                if (!ContainsFile(references, sourceGenAttrPath))
                    references.Add(MetadataReference.CreateFromFile(sourceGenAttrPath));
                Console.WriteLine($"[AssemblyCompiler] 添加引用: {sourceGenAttrPath}");
            }
            else
            {
                Console.WriteLine($"[AssemblyCompiler] 警告: 未找到 RD.SourceGeneratorAttribute.dll");
            }
        }

        private static bool ContainsFile(List<PortableExecutableReference> references, string path)
        {
            foreach (PortableExecutableReference r in references)
            {
                if (string.Equals(r.FilePath, path, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private CSharpCompilation CreateCompilation(List<SyntaxTree> syntaxTrees, List<PortableExecutableReference> references)
        {
            return CSharpCompilation.Create(
                null,
                syntaxTrees.ToArray(),
                references.ToArray(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
        }

        private Assembly EmitAndLoadAssembly(CSharpCompilation compilation)
        {
            using var memStream = new MemoryStream();
            EmitResult emitResult = compilation.Emit(memStream);

            if (!emitResult.Success)
            {
                var errors = new System.Text.StringBuilder();
                foreach (Diagnostic diagnostic in emitResult.Diagnostics)
                {
                    errors.AppendLine($"{diagnostic.GetMessage()}");
                }

                throw new Exception($"动态编译失败:\n{errors}");
            }

            memStream.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(memStream.ToArray());
        }

        private string FindAssembly(string assemblyName, string solutionPath)
        {
            // 优先从已加载的程序集中查找
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (assembly.GetName().Name == Path.GetFileNameWithoutExtension(assemblyName) &&
                        !string.IsNullOrEmpty(assembly.Location) &&
                        File.Exists(assembly.Location))
                    {
                        return assembly.Location;
                    }
                }
                catch
                {
                    // 忽略
                }
            }

            // 从文件系统中查找
            string packagesDir = Path.Combine(solutionPath, "Packages/cn.rendu.sourcegenerator");
            if (!Directory.Exists(packagesDir))
                return null;

            string[] dllFiles = Directory.GetFiles(packagesDir, assemblyName, SearchOption.AllDirectories);
            if (dllFiles.Length == 0)
                return null;

            // 优先选择 obj 目录下的文件
            foreach (string dll in dllFiles)
            {
                if (dll.Contains("/obj/") || dll.Contains("\\obj\\"))
                    return dll;
            }

            return dllFiles[0];
        }
    }
}
