using System;
using System.IO;

namespace RD
{
    /// <summary>
    /// 代码生成器 - 负责生成最终的 CS 文件
    /// 遵循单一职责原则，只负责代码生成
    /// </summary>
    public class CodeGenerator
    {
        private readonly PathProvider pathProvider;
        private readonly OpcodeRegistry opcodeRegistry;

        public CodeGenerator(PathProvider pathProvider, OpcodeRegistry opcodeRegistry)
        {
            this.pathProvider = pathProvider;
            this.opcodeRegistry = opcodeRegistry;
        }

        /// <summary>
        /// 生成到指定路径（策略模式使用）
        /// </summary>
        public void GenerateToPath(string content, string outputPath, string fileName)
        {
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            string filePath = Path.Combine(outputPath, $"{fileName}.cs");
            Console.WriteLine($"[CodeGenerator] 生成: {filePath}");

            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// 添加 opcode 常量类到代码中
        /// </summary>
        public string AppendOpcodeConstants(string code, string protoName)
        {
            string constantsClass = $@"
	public static class {protoName}
	{{
";
            foreach (var info in opcodeRegistry.GetAllOpcodes())
            {
                constantsClass += $"\t\tpublic const ushort {info.Name} = {info.Opcode};\n";
            }

            constantsClass += "\t}";

            // 在命名空间结束前插入 opcode 常量类
            int namespaceEndIndex = code.LastIndexOf('}');
            return code.Insert(namespaceEndIndex, constantsClass);
        }
    }
}
