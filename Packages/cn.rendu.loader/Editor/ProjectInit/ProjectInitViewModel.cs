using RD.Init.Editor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 项目初始化视图模型
    /// </summary>
    public class ProjectInitViewModel : ViewModelBase
    {
        private readonly ProjectInitModel _model;
        private bool _isProcessing;

        public ProjectInitViewModel(ProjectInitModel model)
        {
            _model = model;
            _isProcessing = false;
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public ProjectInitModel Model => _model;

        /// <summary>
        /// 加载包名和场景名信息（不执行完整初始化）
        /// </summary>
        public void LoadPackageInfo()
        {
            try
            {
                UnityEngine.Debug.Log("[ProjectInit] ========== 开始加载包信息 ==========");

                _model.PackageName = ProjectInitCommand.FindPackageName();
                UnityEngine.Debug.Log($"[ProjectInit] 包名称: {_model.PackageName}");

                // 确保资源数据库已刷新
                UnityEditor.AssetDatabase.Refresh();

                // 从 GlobalConfigAsset 读取配置信息
                string assetPath = "Packages/cn.rendu.loader/Resources/GlobalConfig.asset";
                UnityEngine.Debug.Log($"[ProjectInit] 加载资源路径: {assetPath}");

                GlobalConfig globalConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<GlobalConfig>(assetPath);
                if (globalConfig is not null)
                {
                    _model.SceneName = globalConfig.SceneName;
                    _model.CodeMode = globalConfig.CodeMode;
                    _model.Address = globalConfig.Address;
                    UnityEngine.Debug.Log($"[ProjectInit] SceneName: {_model.SceneName}, CodeMode: {_model.CodeMode}, Address: {_model.Address}");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("[ProjectInit] GlobalConfigAsset 为 null，尝试创建默认值");
                    _model.SceneName = "Main";
                    _model.CodeMode = CodeMode.Client;
                    _model.Address = "";
                }

                _model.IsInited = ProjectInitCommand.CheckIsInited();
                UnityEngine.Debug.Log($"[ProjectInit] 是否已初始化: {_model.IsInited}");

                UnityEngine.Debug.Log("[ProjectInit] ========== 加载包信息完成 ==========");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[ProjectInit] 加载包信息失败: {ex.Message}\n{ex.StackTrace}");
                // 设置默认值，避免 null
                _model.SceneName = "Main";
                _model.CodeMode = CodeMode.Client;
                _model.Address = "";
            }
        }

        /// <summary>
        /// 执行项目初始化
        /// </summary>
        public void InitProject()
        {
            InitProjectInternal(_model);
        }

        /// <summary>
        /// 静态方法：执行项目初始化
        /// </summary>
        public static void InitProjectStatic(ProjectInitModel model)
        {
            InitProjectInternal(model);
        }

        /// <summary>
        /// 内部实现：执行项目初始化
        /// </summary>
        private static void InitProjectInternal(ProjectInitModel model)
        {
            try
            {
                Debug.Log("[ProjectInit] ========== 开始项目初始化 ==========");

                // 1. 设置 GlobalConfig 中的 SceneName
                SetSceneNameInternal(model);
                Debug.Log("[ProjectInit] 设置 SceneName 完成");

                // 2. 链接 SLN 文件
                LinkSlnInternal();
                Debug.Log("[ProjectInit] 链接 SLN 完成");

                // 3. 刷新 4 个程序集的 asmdef 引用
                UpdateScriptsReferencesInternal();
                Debug.Log("[ProjectInit] 更新 Scripts 引用完成");

                // 4. 切换到 ClientServer 代码模式
                ChangeCodeModeInternal("ClientServer");
                Debug.Log("[ProjectInit] 切换 CodeMode 到 ClientServer 完成");

                // 5. 运行初始化脚本
                RunInitScriptsInternal();
                Debug.Log("[ProjectInit] 运行初始化脚本完成");

                // 6. 启用 INITED 宏定义
                EnableDefineInternal();
                Debug.Log("[ProjectInit] 启用 INITED 宏定义完成");

                Debug.Log("[ProjectInit] ========== 项目初始化完成 ==========");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[ProjectInit] 项目初始化失败: {ex.Message}");
            }
        }

        private static void SetSceneNameInternal(ProjectInitModel model)
        {
            model.PackageName = ProjectInitCommand.FindPackageName();
            new SetSceneNameCommand(model.SceneName).Execute();
            model.SceneName = model.SceneName;
        }

        private static void LinkSlnInternal()
        {
            new LinkSlnCommand().Execute();
        }

        private static void UpdateScriptsReferencesInternal()
        {
            new UpdateScriptsReferencesCommand().Execute();
        }

        private static void ChangeCodeModeInternal(string codeMode)
        {
            new ChangeCodeModeCommand(codeMode).Execute();
        }

        private static void RunInitScriptsInternal()
        {
            new RunInitScriptsCommand().Execute();
        }

        private static void EnableDefineInternal()
        {
            new EnableDefineCommand("INITED").Execute();
        }

        /// <summary>
        /// 检查项目是否已初始化
        /// </summary>
        public bool CheckIsInited()
        {
            return ProjectInitCommand.CheckIsInited();
        }
    }
}
