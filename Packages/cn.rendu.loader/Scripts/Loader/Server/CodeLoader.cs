#if DOTNET
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using RD.Core;
using RD.Server;

namespace RD
{
    /// <summary>
    /// 服务端程序集加载器。
    /// Model 程序集已由 dotnet 运行时预加载，只需从中查找；
    /// Hotfix 程序集通过 AssemblyLoadContext 独立加载，支持运行时热重载（卸载旧 Context → 重新加载 dll）。
    /// </summary>
    public class CodeLoader: Singleton<CodeLoader>, ISingletonAwake
    {
        // ----- 程序集名称 -----
        private const string HotfixAssemblyName = "RD.Hotfix";
        private const string ModelAssemblyName  = "RD.Model";

        // ----- 入口 -----
        private const string EntryClassName  = "RD.Entry";
        private const string EntryMethodName = "Start";

        // ----- 文件路径 -----
        private const string HotfixDllPath = "./RD.Hotfix.dll";
        private const string HotfixPdbPath = "./RD.Hotfix.pdb";

        /// <summary>Model 程序集引用（热重载不重新加载）</summary>
        private Assembly modelAssembly;

        /// <summary>Hotfix 专用的 AssemblyLoadContext，支持卸载后重新加载</summary>
        private AssemblyLoadContext hotfixLoadContext;

        public void Awake() { }

        // ==================== assembly load ====================

        /// <summary>从当前 AppDomain 中查找已加载的 Model 程序集</summary>
        private Assembly ResolveModelAssembly()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .First(ass => ass.GetName().Name == ModelAssemblyName);
        }

        /// <summary>
        /// 通过独立的 AssemblyLoadContext 加载 Hotfix 程序集。
        /// 每次调用会先卸载旧 Context，确保热重载时不会冲突。
        /// </summary>
        private Assembly LoadHotfix()
        {
            // 卸载旧的 LoadContext，释放对旧 dll 的引用
            hotfixLoadContext?.Unload();
            GC.Collect();
            hotfixLoadContext = new AssemblyLoadContext(HotfixAssemblyName, isCollectible: true);

            byte[] dllBytes = File.ReadAllBytes(HotfixDllPath);
            byte[] pdbBytes = File.ReadAllBytes(HotfixPdbPath);
            using var dllStream = new MemoryStream(dllBytes);
            using var pdbStream = new MemoryStream(pdbBytes);
            return hotfixLoadContext.LoadFromStream(dllStream, pdbStream);
        }

        // ==================== entry ====================

        /// <summary>
        /// 启动流程：
        /// 1. 查找已加载的 Model 程序集
        /// 2. 通过 AssemblyLoadContext 加载 Hotfix
        /// 3. 注册到 World.CodeTypes → 调用 RD.Entry.Start()
        /// </summary>
        public void Start()
        {
            // 1. Model 由运行时预加载
            this.modelAssembly = ResolveModelAssembly();

            // 2. Hotfix 独立加载
            Assembly hotfixAssembly = LoadHotfix();

            // 3. 注册 CodeTypes → 启动 Entry
            World.Instance.AddSingleton<CodeTypes, Assembly[]>(new[]
            {
                typeof(World).Assembly, typeof(Launcher).Assembly,
                this.modelAssembly, hotfixAssembly,
            });

            new StaticMethod(this.modelAssembly, EntryClassName, EntryMethodName).Run();
        }

        /// <summary>
        /// 热重载：卸载旧 Hotfix LoadContext → 重新加载 Hotfix dll → 刷新 CodeTypes。
        /// Model 程序集保持不动。
        /// </summary>
        public void Reload()
        {
            using var _ = Profiler.Once("CodeLoader.Reload");
            Assembly hotfixAssembly = LoadHotfix();

            CodeTypes codeTypes = World.Instance.AddSingleton<CodeTypes, Assembly[]>(new[]
            {
                typeof(World).Assembly, typeof(Launcher).Assembly,
                this.modelAssembly, hotfixAssembly,
            });
            codeTypes.CodeProcess();

            Log.Info("reload dll finish!");
        }
    }
}
#endif