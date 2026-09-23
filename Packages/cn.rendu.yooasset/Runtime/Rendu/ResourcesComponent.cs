using System;
using System.Collections.Generic;
using System.IO;
using RD.Core;
using UnityEngine;
using YooAsset;

namespace RD.YooAsset
{
    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    public class RemoteService : IRemoteService
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteService(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }
        public IReadOnlyList<string> GetRemoteUrls(string fileName)
        {
            List<string> result = new List<string>();
            result.Add($"{_defaultHostServer}/{fileName}");
            result.Add($"{_fallbackHostServer}/{fileName}");
            return result;
        }
        
        string GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }

        string GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }

    public class ResourcesComponent : Singleton<ResourcesComponent>, ISingletonAwake
    {
        public string PackageName { get; private set; }

        /// <summary>当前资源包运行模式（EditorSimulateMode / OfflinePlayMode / HostPlayMode / WebPlayMode）</summary>
        public EPlayMode PlayMode { get; private set; }

        /// <summary>
        /// 当前模式是否具备资源下载能力（决定是否走"检查更新 → 下载 → 进度展示"流程）。
        /// EditorSimulateMode（虚拟下载）/ HostPlayMode（联机）/ WebPlayMode（WebGL）支持下载；
        /// OfflinePlayMode 资源全内置，无下载流程。
        /// 与是否 Editor 无关，完全由 YooAsset 运行模式决定。
        /// </summary>
        public bool IsDownloadEnabled =>
            PlayMode == EPlayMode.EditorSimulateMode ||
            PlayMode == EPlayMode.HostPlayMode ||
            PlayMode == EPlayMode.WebPlayMode;

        /// <summary>
        /// 本地内置/缓存清单版本号（初始化时记录，用于与远端版本对比判断是否需要更新）
        /// </summary>
        public string LocalPackageVersion { get; private set; }

        public void Awake()
        {
            YooAssets.Initialize();
        }

        protected override void Destroy()
        {
            YooAssets.Destroy();
        }

        public async Promise CreatePackageAsync(string packageName, bool isDefault = false)
        {
            PackageName = isDefault? "DefaultPackage" : packageName; 

            YooConfig yooConfig = Resources.Load<YooConfig>("YooConfig");
            if (yooConfig == null)
            {
                throw new Exception("YooConfig is null\n\n请确保 YooConfig.asset 文件存在于 Packages/cn.rendu.yooasset/Resources/ 文件夹中（包内 Resources 会被 Resources.Load 正常检索）。");
            }
            // Create package.
            if (!YooAssets.TryGetPackage(packageName, out var package))
                package = YooAssets.CreatePackage(packageName);

            // 记录运行模式（供上层判断是否走检查/下载/进度流程）
            PlayMode = yooConfig.EPlayMode;

            // 编辑器下的模拟模式
            InitializePackageOperation initializationOperation = null;
            switch (yooConfig.EPlayMode)
            {
                // Editor simulation mode.
                case EPlayMode.EditorSimulateMode:
                {
                    // 测试便利：勾选 EditorForceRedownload 时每次启动清空虚拟下载缓存，
                    // 强制重新走一遍「检查 → 下载核心」流程（与真机每次启动检查更新的行为一致）
                    if (yooConfig.EditorForceRedownload)
                    {
                        ClearEditorSimulateCache(packageName);
                    }

                    var buildResult = EditorSimulateBuildInvoker.Build(packageName, (int)EBundleType.VirtualAssetBundle);
                    var packageRoot = buildResult.PackageRootDirectory;
                    var createParameters = new EditorSimulateModeOptions();
                    createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                    createParameters.EditorFileSystemParameters.AddParameter(EFileSystemParameter.VirtualWebglMode, true);
                    createParameters.EditorFileSystemParameters.AddParameter(EFileSystemParameter.VirtualDownloadMode, true);
                    createParameters.EditorFileSystemParameters.AddParameter(EFileSystemParameter.VirtualDownloadSpeed, yooConfig.EditorSimulateDownloadSpeed);
                    createParameters.EditorFileSystemParameters.AddParameter(EFileSystemParameter.AsyncSimulateMinFrame, 5);
                    createParameters.EditorFileSystemParameters.AddParameter(EFileSystemParameter.AsyncSimulateMaxFrame, 10);
                    initializationOperation = package.InitializePackageAsync(createParameters);
                    break;
                }
                case EPlayMode.OfflinePlayMode:
                {
                    var createParameters = new OfflinePlayModeOptions();
                    createParameters.BuiltinFileSystemParameters = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters();
                    initializationOperation = package.InitializePackageAsync(createParameters);
                    break;
                }
                case EPlayMode.HostPlayMode:
                {
                    // 测试便利：勾选 HostForceClearSandbox 时每次启动清空本地沙盒缓存，
                    // 模拟首次安装，强制重新走「检查 → 下载」流程
                    //（与 EditorSimulateMode 的 EditorForceRedownload 对应，均仅测试用）
                    if (yooConfig.HostForceClearSandbox)
                    {
                        ClearSandboxCache(packageName);
                    }

                    string defaultHostServer = GetHostServerURL(yooConfig.Url, package.PackageName);
                    string fallbackHostServer = GetHostServerURL(yooConfig.Url, package.PackageName);
                    IRemoteService remoteService = new RemoteService(defaultHostServer, fallbackHostServer);
                    var createParameters = new HostPlayModeOptions();
                    createParameters.BuiltinFileSystemParameters = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters();
                    createParameters.BuiltinFileSystemParameters.AddParameter(EFileSystemParameter.CopyBuiltinPackageManifest, true);
                    createParameters.CacheFileSystemParameters = FileSystemParameters.CreateDefaultSandboxFileSystemParameters(remoteService);
                    createParameters.CacheFileSystemParameters.AddParameter(EFileSystemParameter.DownloadMaxConcurrency, 5);
                    createParameters.CacheFileSystemParameters.AddParameter(EFileSystemParameter.DownloadMaxRequestPerFrame, 1);
                    createParameters.CacheFileSystemParameters.AddParameter(EFileSystemParameter.DownloadWatchdogTimeout, 10);
                    // 启用断点续传：1MB 以上的文件下载中断后保留临时文件，下次启动从断点继续，而非整文件重下
                    createParameters.CacheFileSystemParameters.AddParameter(EFileSystemParameter.DownloadResumeMinimumSize, 1024 * 1024L);
                    initializationOperation = package.InitializePackageAsync(createParameters);
                    break;
                }
                case EPlayMode.WebPlayMode:
                {
#if UNITY_WEBGL && (WEIXINMINIGAME || UNITY_WECHATMINIGAME) && !UNITY_EDITOR
            var createParameters = new WebPlayModeOptions();
            string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE"; // Change this path if subdirectories are required.
            IRemoteService remoteService = new RemoteService(defaultHostServer, fallbackHostServer);
            createParameters.WebNetworkFileSystemParameters = WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteService);
            initializationOperation = package.InitializePackageAsync(createParameters);
#else
                    var createParameters = new WebPlayModeOptions();
                    createParameters.WebServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
                    initializationOperation = package.InitializePackageAsync(createParameters);
#endif
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            await initializationOperation;
            if (initializationOperation.Status != EOperationStatus.Succeeded)
            {
                Debug.LogError($"Initialize package [{packageName}] failed: {initializationOperation.Error}");
                return;
            }

            // 记录本地版本号（从内置/缓存清单读取，不依赖 ActiveManifest）
            LocalPackageVersion = package.GetLocalPackageVersion();

            RequestPackageVersionOperation versionOp = package.RequestPackageVersionAsync();
            await versionOp;
            if (versionOp.Status != EOperationStatus.Succeeded)
            {
                Debug.LogError($"Request package version [{packageName}] failed: {versionOp.Error}");
                return;
            }

            LoadPackageManifestOperation manifestOp = package.LoadPackageManifestAsync(
                new LoadPackageManifestOptions(versionOp.PackageVersion, 60));
            await manifestOp;
            if (manifestOp.Status != EOperationStatus.Succeeded)
            {
                // 远端清单加载失败时，回退加载本地已有清单（内置或缓存），
                // 保证热更代码可立即加载资源（dll/config 等）。
                Debug.LogWarning($"Load remote manifest [{packageName}] failed: {manifestOp.Error}");
                if (string.IsNullOrEmpty(LocalPackageVersion) == false
                    && LocalPackageVersion != versionOp.PackageVersion)
                {
                    manifestOp = package.LoadPackageManifestAsync(
                        new LoadPackageManifestOptions(LocalPackageVersion, 60));
                    await manifestOp;
                }

                // 本地清单也加载失败时中断启动流程，
                // 避免后续 LoadAssetAsync 在 ActiveManifest 为 null 时
                // 抛出 "Active package manifest not found." 这类误导性错误。
                if (manifestOp.Status != EOperationStatus.Succeeded)
                {
                    throw new Exception($"Load package manifest [{packageName}] failed: {manifestOp.Error}");
                }
            }
        }

        string GetHostServerURL(string url, string pacakgeName)
        {
            //string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
            string hostServerIP = url;
            string appVersion = $"{Application.version}";
                
                
#if UNITY_EDITOR
            switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
            {
                case UnityEditor.BuildTarget.WebGL:
                {
                    return $"{hostServerIP}/StreamingAssets/Bundles/{pacakgeName}";
                }
                case UnityEditor.BuildTarget.Android:
                    return $"{hostServerIP}/Package/Bundles/Android/{pacakgeName}/{appVersion}";
                case UnityEditor.BuildTarget.iOS:
                    return $"{hostServerIP}/Package/Bundles/IPhone/{pacakgeName}/{appVersion}";
                default:
                    return $"{hostServerIP}/Package/Bundles/{GetStandalonePlatformDir(UnityEditor.EditorUserBuildSettings.activeBuildTarget)}/{pacakgeName}/{appVersion}";
            }
#else
            switch (Application.platform)
            {
                case RuntimePlatform.WebGLPlayer:
                {
                    return $"{hostServerIP}/StreamingAssets/Bundles/{pacakgeName}";
                }
                case RuntimePlatform.Android:
                    return $"{hostServerIP}/Package/Bundles/Android/{pacakgeName}/{appVersion}";
                case RuntimePlatform.IPhonePlayer:
                    return $"{hostServerIP}/Package/Bundles/IPhone/{pacakgeName}/{appVersion}";
                default:
                    return $"{hostServerIP}/Package/Bundles/{GetStandalonePlatformDir(Application.platform)}/{pacakgeName}/{appVersion}";
            }
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// 将构建目标平台映射为 CDN 目录名（与资源构建输出目录 BuildParameters.GetPackageOutputDirectory 保持一致）
        /// </summary>
        static string GetStandalonePlatformDir(UnityEditor.BuildTarget target)
        {
            switch (target)
            {
                case UnityEditor.BuildTarget.StandaloneWindows:
                case UnityEditor.BuildTarget.StandaloneWindows64:
                    return "StandaloneWindows64";
                case UnityEditor.BuildTarget.StandaloneLinux64:
                    return "StandaloneLinux64";
                case UnityEditor.BuildTarget.StandaloneOSX:
                    return "StandaloneOSX";
                default:
                    return target.ToString();
            }
        }
#else
        /// <summary>
        /// 将运行时平台映射为 CDN 目录名（与资源构建输出目录 BuildParameters.GetPackageOutputDirectory 保持一致）
        /// </summary>
        static string GetStandalonePlatformDir(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.WindowsPlayer:
                    return "StandaloneWindows64";
                case RuntimePlatform.LinuxPlayer:
                    return "StandaloneLinux64";
                case RuntimePlatform.OSXPlayer:
                    return "StandaloneOSX";
                default:
                    return platform.ToString();
            }
        }
#endif

        public void DestroyPackage(string packageName)
        {
            ResourcePackage package = YooAssets.GetPackage(packageName);
            package.DestroyPackageAsync();
        }

        /// <summary>
        /// 清空编辑器模拟下载缓存（Library/YooFolderName/PackageName/SimulateCache）。
        /// 虚拟下载的 bundle 会以 marker 形式持久化在此目录，清空后下次初始化会重新走一遍下载流程，
        /// 用于测试「每次启动检查 → 下载核心」。
        /// </summary>
        private static void ClearEditorSimulateCache(string packageName)
        {
            string root = Path.Combine(
                Path.GetDirectoryName(Application.dataPath),
                "Library",
                YooAssetConfiguration.GetYooFolderName(),
                packageName,
                "SimulateCache");
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
        
        /// <summary>
        /// 清空本地沙盒缓存（已下载资源 + 缓存清单 + 断点续传临时文件），
        /// 用于模拟首次安装、强制全量重新下载。仅测试用。
        /// </summary>
        private static void ClearSandboxCache(string packageName)
        {
            string root = YooAssetConfiguration.GetDefaultCacheRoot(packageName);
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }

        /// <summary>
        /// 主要用来加载dll config aotdll，因为这时候纤程还没创建，无法使用ResourcesLoaderComponent。
        /// 游戏中的资源应该使用ResourcesLoaderComponent来加载
        /// </summary>
        public async Promise<T> LoadAssetAsync<T>(string location) where T : UnityEngine.Object
        {
            var package = YooAssets.GetPackage(this.PackageName);
            AssetHandle handle =   package.LoadAssetAsync<T>(location);
            await handle;
            T t = (T)handle.AssetObject;
            handle.Release();
            return t;
        }

        /// <summary>
        /// 主要用来加载dll config aotdll，因为这时候纤程还没创建，无法使用ResourcesLoaderComponent。
        /// 游戏中的资源应该使用ResourcesLoaderComponent来加载
        /// </summary>
        public async Promise<Dictionary<string, T>> LoadAllAssetsAsync<T>(string location) where T : UnityEngine.Object
        {
            var package = YooAssets.GetPackage(this.PackageName);
            AllAssetsHandle allAssetsOperationHandle = package.LoadAllAssetsAsync<T>(location);
            await allAssetsOperationHandle;
            Dictionary<string, T> dictionary = new Dictionary<string, T>();
            foreach (UnityEngine.Object assetObj in allAssetsOperationHandle.AllAssetObjects)
            {
                T t = assetObj as T;
                dictionary.Add(t.name, t);
            }

            allAssetsOperationHandle.Release();
            return dictionary;
        }
    }
}