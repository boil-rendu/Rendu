using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HybridCLR;
using RD.Core;
using UnityEngine;
using RD.Client;
using RD.Loader;
using RD.YooAsset;
using YooAsset;

namespace RD
{
    /// <summary>
    /// 客户端程序集加载器。
    /// 负责从 AssetBundle（具备下载能力的模式：EditorSimulateMode / HostPlayMode / WebPlayMode）或本地 CodeDir（OfflinePlayMode）
    /// 加载 Model / ModelView / Hotfix / HotfixView 四个程序集，
    /// 并将它们注册到 World.CodeTypes 中，最终启动 RD.Entry。
    /// </summary>
    public class CodeLoader: Singleton<CodeLoader>, ISingletonAwake
    {
        // ----- 程序集名称 -----
        private const string HotfixAssemblyName    = "RD.Hotfix";
        private const string HotfixViewAssemblyName = "RD.HotfixView";
        private const string ModelAssemblyName     = "RD.Model";
        private const string ModelViewAssemblyName  = "RD.ModelView";

        // ----- 入口 -----
        private const string EntryClassName  = "RD.Entry";
        private const string EntryMethodName = "Start";

        // ----- AssetBundle 路径 -----
        private const string ModelAssetPath = "Packages/cn.rendu.loader/Bundles/Code/RD.Model.dll.bytes";
        private const string AotAssetPath   = "Packages/cn.rendu.loader/Bundles/AotDlls/mscorlib.dll.bytes";

        // ----- 命名辅助 -----
        private static string AsmDllName(string asm)      => $"{asm}.dll";
        private static string AsmPdbName(string asm)      => $"{asm}.pdb";
        private static string AsmDllBytesName(string asm) => $"{asm}.dll.bytes";
        private static string AsmPdbBytesName(string asm) => $"{asm}.pdb.bytes";

        /// <summary>Model + ModelView 程序集，热重载时不重新加载</summary>
        private Assembly modelAssembly;
        private Assembly modelViewAssembly;

        /// <summary>从 AssetBundle 下载的 dll 资源（key: "RD.Hotfix.dll" 等）</summary>
        private Dictionary<string, TextAsset> dlls;
        /// <summary>IL2CPP 下补充元数据所需的 AOT dll</summary>
        private Dictionary<string, TextAsset> aotDlls;

        public void Awake() { }

        // ==================== download ====================

        /// <summary>
        /// 是否启用"检查/更新核心数据"流程（下载 + 进度展示），同时决定代码 dll 的来源：
        /// EditorSimulateMode / HostPlayMode / WebPlayMode 具备下载能力 → 启用，代码从资源包（虚拟）下载/加载；
        /// OfflinePlayMode 资源全内置无下载 → 禁用，代码从本地 CodeDir 读取。
        /// </summary>
        private static bool IsCheckUpdateEnabled => ResourcesComponent.Instance.IsDownloadEnabled;

        /// <summary>具备下载能力的模式下，从 AssetBundle 下载代码资源包（Editor 模拟模式走虚拟下载）</summary>
        private async Promise DownloadAsync()
        {
            // 不具备下载能力的模式（OfflinePlayMode）直接从 CodeDir 读取文件，无需下载
            if (!IsCheckUpdateEnabled) return;

            this.dlls = await ResourcesComponent.Instance.LoadAllAssetsAsync<TextAsset>(ModelAssetPath);
            if (Define.EnableIL2CPP)
            {
                this.aotDlls = await ResourcesComponent.Instance.LoadAllAssetsAsync<TextAsset>(AotAssetPath);
            }
        }

        // ==================== code update check ====================

        /// <summary>创建代码资源下载器（4 个 dll 所在包 + AOT 元数据包），用于判断与下载核心数据。清单中无代码资源时返回 null。</summary>
        private ResourceDownloaderOperation CreateCodeDownloader()
        {
            var package = YooAssets.GetPackage(ResourcesComponent.Instance.PackageName);

            // 收集代码资源定位（4 个 dll 打在同一代码包；IL2CPP 下还需 AOT 元数据包）
            var assets = new List<AssetInfo> { package.GetAssetInfo(ModelAssetPath) };
            if (Define.EnableIL2CPP)
            {
                assets.Add(package.GetAssetInfo(AotAssetPath));
            }
            assets.RemoveAll(a => !a.IsValid);
            if (assets.Count == 0) return null;

            return package.CreateResourceDownloader(
                new BundleDownloaderOptions(assets.ToArray(), true, 5, 3));
        }

        /// <summary>
        /// 判断核心数据（代码 dll 所在资源包）是否为最新。
        /// 原理：用当前 ActiveManifest（远端最新清单）统计需要下载的代码 bundle 数量，
        /// 0 表示本地沙盒缓存已具备当前清单所需的代码，无需更新。
        /// OfflinePlayMode 下无代码资源可下载，恒为 true。
        /// </summary>
        public bool IsCoreDataReady()
        {
            ResourceDownloaderOperation downloader = CreateCodeDownloader();
            return downloader == null || downloader.TotalDownloadCount <= 0;
        }

        /// <summary>
        /// 每次启动、在加载核心数据（dll）之前，检查代码资源是否为最新并静默下载。
        /// 原理：包初始化时 ActiveManifest 已加载为远端最新清单，
        /// 创建按代码资源定位的下载器，TotalDownloadCount &gt; 0 说明本地沙盒缓存缺少新版代码 bundle（有新代码），
        /// 先下载再加载，保证每次启动加载的都是最新代码，代码更新无需重启生效。
        /// 仅具备下载能力的模式执行（EditorSimulateMode / HostPlayMode / WebPlayMode）。
        /// </summary>
        private async Promise CheckAndUpdateCodeAsync()
        {
            // 仅具备下载能力的模式（EditorSimulateMode / HostPlayMode / WebPlayMode）执行
            if (!IsCheckUpdateEnabled) return;

            ResourceDownloaderOperation downloader = CreateCodeDownloader();
            if (downloader == null || downloader.TotalDownloadCount <= 0) return;

            // 有更新：启动界面展示下载进度（含字节进度）
            LaunchScreen.Instance?.SetState("检测到新代码，正在更新核心数据...");
            downloader.DownloadProgressChanged += args =>
            {
                // Log.Debug($"[LaunchScreen] 下载进度 {args.Progress:P0} | {args.CurrentDownloadBytes}/{args.TotalDownloadBytes} bytes | {args.CurrentDownloadCount}/{args.TotalDownloadCount}");
                LaunchScreen.Instance?.SetProgress(
                    args.Progress,
                    args.CurrentDownloadBytes,
                    args.TotalDownloadBytes,
                    $"检测到新代码，正在更新核心数据（{args.CurrentDownloadCount}/{args.TotalDownloadCount}）");
            };
            Log.Info($"检测到新代码，正在下载 {downloader.TotalDownloadCount} 个代码资源包（{downloader.TotalDownloadBytes} bytes）...");
            downloader.StartDownload();
            await downloader;
            LaunchScreen.Instance?.SetProgress(1f, downloader.TotalDownloadBytes, downloader.TotalDownloadBytes, "核心数据更新完成，正在加载...");
            if (downloader.Status != EOperationStatus.Succeeded)
            {
                LaunchScreen.Instance?.SetState($"核心数据更新失败：{downloader.Error}");
                throw new Exception($"下载代码资源失败: {downloader.Error}");
            }

            LaunchScreen.Instance?.SetState("核心数据更新完成，正在加载...");
        }

        // ==================== assembly load ====================

        /// <summary>
        /// 读取程序集的 dll + pdb 字节数据。
        /// - 具备下载能力的模式（含 EditorSimulateMode 虚拟下载）：从 AssetBundle 的 TextAsset 中读取
        /// - OfflinePlayMode / USE_LOCAL_DLL_BYTES：直接从 CodeDir 读取文件
        /// </summary>
        /// <param name="asmName">程序集名（如 "RD.Hotfix"）</param>
        /// <param name="dllDict">非空时从 AssetBundle 取；null 时从本地文件取</param>
        private static (byte[] ass, byte[] pdb) ReadAssemblyBytes(string asmName, Dictionary<string, TextAsset> dllDict = null)
        {
#if USE_LOCAL_DLL_BYTES
            var baseDir = Define.CodeDir;
            return (
                File.ReadAllBytes(Path.Combine(baseDir, AsmDllBytesName(asmName))),
                File.ReadAllBytes(Path.Combine(baseDir, AsmPdbBytesName(asmName)))
            );
#else
            if (dllDict != null)
            {
                return (
                    dllDict[AsmDllName(asmName)].bytes,
                    dllDict[AsmPdbName(asmName)].bytes
                );
            }
            var dir = Define.CodeDir;
            return (
                File.ReadAllBytes(Path.Combine(dir, AsmDllBytesName(asmName))),
                File.ReadAllBytes(Path.Combine(dir, AsmPdbBytesName(asmName)))
            );
#endif
        }

        /// <summary>读取字节并调用 Assembly.Load 加载到 AppDomain</summary>
        private static Assembly LoadAssembly(string asmName, Dictionary<string, TextAsset> dllDict = null)
        {
            var (ass, pdb) = ReadAssemblyBytes(asmName, dllDict);
            return Assembly.Load(ass, pdb);
        }

        /// <summary>IL2CPP 下为 AOT 程序集补充元数据，使解释器能正常调用 AOT 方法</summary>
        private void LoadAotMetadata()
        {
            if (!Define.EnableIL2CPP) return;
            if (this.aotDlls == null) return;

            foreach (var kv in this.aotDlls)
            {
                RuntimeApi.LoadMetadataForAOTAssembly(kv.Value.bytes, HomologousImageMode.SuperSet);
            }
        }

        /// <summary>加载 Model + ModelView（热重载不重新加载）</summary>
        private void LoadModelAssemblies()
        {
            this.modelAssembly     = LoadAssembly(ModelAssemblyName,     IsCheckUpdateEnabled ? this.dlls : null);
            this.modelViewAssembly = LoadAssembly(ModelViewAssemblyName, IsCheckUpdateEnabled ? this.dlls : null);
        }

        /// <summary>加载 Hotfix + HotfixView（热重载时重新加载）</summary>
        private (Assembly hotfix, Assembly hotfixView) LoadHotfix()
        {
            var hotfix     = LoadAssembly(HotfixAssemblyName,     IsCheckUpdateEnabled ? this.dlls : null);
            var hotfixView = LoadAssembly(HotfixViewAssemblyName, IsCheckUpdateEnabled ? this.dlls : null);
            return (hotfix, hotfixView);
        }

        // ==================== entry ====================

        /// <summary>
        /// 启动流程：
        /// 0. 展示加载器层启动界面（具备下载能力的模式），负责核心数据下载/加载的进度与状态
        /// 1. 每次启动先检查代码更新（具备下载能力的模式）：有新代码先静默下载，确保加载的是最新代码
        /// 2. 下载代码资源（具备下载能力的模式）
        /// 3. 补充 AOT 元数据（IL2CPP）
        /// 4. 加载四个程序集
        /// 5. 注册到 World.CodeTypes → 调用 RD.Entry.Start()
        /// 6. 热更层已接管，隐藏加载器层启动界面
        /// </summary>
        public async Promise Start()
        {
            // 0. 展示加载器层启动界面（具备下载能力的模式）：核心数据（dll）下载/加载期间显示进度
            if (IsCheckUpdateEnabled)
            {
                LaunchScreen screen = LaunchScreen.Show();
                screen?.SetState("正在加载核心数据...");

                // 左下角服务器/渠道信息：地址来自 GlobalConfig，版本为本地清单版本
                var globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
                screen?.SetServerInfo(
                    $"服务器：{globalConfig?.Address ?? "未知"}   版本：{ResourcesComponent.Instance.LocalPackageVersion}");
            }

            // 1. 每次启动先检查代码更新，再加载核心数据（dll）
            await CheckAndUpdateCodeAsync();

            // 2. 下载
            await DownloadAsync();

            // 3. AOT 元数据
            LoadAotMetadata();

            // 4. 加载 Model（不热更） + Hotfix（热更）
            LoadModelAssemblies();
            (Assembly hotfixAssembly, Assembly hotfixViewAssembly) = LoadHotfix();

            // 4.1 记录已加载代码摘要，供更新完成后判断热更代码（dll）是否变化
            this.LoadedCodeHash = ComputeCodeHash(this.dlls);

            // 5. 注册 CodeTypes → 启动 Entry
            World.Instance.AddSingleton<CodeTypes, Assembly[]>(new[]
            {
                typeof(World).Assembly, typeof(Launcher).Assembly,
                this.modelAssembly, this.modelViewAssembly,
                hotfixAssembly, hotfixViewAssembly,
            });

            new StaticMethod(this.modelAssembly, EntryClassName, EntryMethodName).Run();

            // 6. 热更层已接管（UIResCheck 即将显示），隐藏加载器层启动界面
            if (IsCheckUpdateEnabled)
            {
                LaunchScreen.Hide();
            }
        }

        /// <summary>
        /// 热重载：重新加载 Hotfix + HotfixView 程序集，刷新 CodeTypes。
        /// Model / ModelView 保持不动。
        /// </summary>
        public void Reload()
        {
            (Assembly hotfixAssembly, Assembly hotfixViewAssembly) = LoadHotfix();

            CodeTypes codeTypes = World.Instance.AddSingleton<CodeTypes, Assembly[]>(new[]
            {
                typeof(World).Assembly, typeof(Launcher).Assembly,
                this.modelAssembly, this.modelViewAssembly,
                hotfixAssembly, hotfixViewAssembly,
            });
            codeTypes.CodeProcess();

            Log.Info("reload dll finish!");
        }

        // ==================== code changed ====================

        /// <summary>启动时已加载的 4 个热更程序集 dll 字节摘要（MD5 拼接），用于更新后判断代码是否变化</summary>
        public string LoadedCodeHash { get; private set; }

        /// <summary>
        /// 计算 4 个热更程序集 dll 的字节摘要。
        /// 真机取 AssetBundle 内 TextAsset，Editor 下 dllDict 为 null 时读本地 CodeDir 文件，与加载来源保持一致。
        /// </summary>
        private static string ComputeCodeHash(Dictionary<string, TextAsset> dllDict)
        {
            var sb = new System.Text.StringBuilder();
            foreach (string asmName in new[] { ModelAssemblyName, ModelViewAssemblyName, HotfixAssemblyName, HotfixViewAssemblyName })
            {
                var (ass, _) = ReadAssemblyBytes(asmName, dllDict);
                sb.Append(HashUtility.ComputeMD5(ass));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 更新完成后判断热更代码（dll）是否发生变化。
        /// 原理：从当前（新）清单重新加载 dll 字节，与启动时已加载的 dll 字节摘要对比。
        /// 摘要一致说明本次只更新了资源，无需重启即可生效；不一致则必须重启加载新 dll。
        /// </summary>
        public async Promise<bool> IsCodeChanged()
        {
            // 未记录旧代码摘要（异常场景），保守处理为需要重启
            if (this.dlls == null || string.IsNullOrEmpty(this.LoadedCodeHash)) return true;

            var newDlls = await ResourcesComponent.Instance.LoadAllAssetsAsync<TextAsset>(ModelAssetPath);
            return ComputeCodeHash(newDlls) != this.LoadedCodeHash;
        }
    }
}