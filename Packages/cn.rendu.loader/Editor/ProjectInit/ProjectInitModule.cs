#if UNITY_EDITOR

using RD.Draw;
using RD.EditTool.Editor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 项目初始化模块: 展示当前工程的初始化状态(包名/场景/代码模式/地址/是否已初始化),
    /// 底部一个"初始化项目"按钮, 触发原 ProjectInitViewModel.InitProjectInternal 的完整链路
    /// (SetSceneName → LinkSln → UpdateScriptsReferences → ChangeCodeMode(ClientServer) → RunInitScripts → EnableDefine(INITED))。
    /// 替代原 ProjectInitWindow + ProjectInitViewModel + ProjectInitWindow.uxml。
    /// </summary>
    [UIAutoMenu("项目初始化", 300, Scope = "loader")]
    public class ProjectInitModule : BaseToolModule
    {
        [LabelText("包名称")]
        [ReadOnly]
        [ShowInInspector]
        public string PackageName { get; private set; } = "(未加载)";

        [LabelText("场景名称")]
        [ReadOnly]
        [ShowInInspector]
        public string SceneName { get; private set; } = "(未加载)";

        [LabelText("代码模式")]
        [ReadOnly]
        [ShowInInspector]
        public CodeMode CodeMode { get; private set; }

        [LabelText("服务器地址")]
        [ReadOnly]
        [ShowInInspector]
        public string Address { get; private set; } = "(未加载)";

        [LabelText("初始化状态")]
        [ShowInInspector]
        [DisplayAsString(false)]
        public string InitStatus { get; private set; } = "(未加载)";

        [LabelText("说明")]
        [DisplayAsString(false, 0, TextAlign.Left, false)]
        [ShowInInspector]
        public string InfoText { get; private set; } =
            "初始化项目将执行以下步骤:\n" +
            "1. 在 GlobalConfig 中设置 SceneName\n" +
            "2. 链接 SLN 文件用于 Rider/VS\n" +
            "3. 更新脚本程序集引用\n" +
            "4. 切换到 ClientServer 代码模式\n" +
            "5. 运行初始化脚本\n" +
            "6. 启用 INITED 宏定义";

        [Button("初始化项目", 10)]
        [GUIColor(0.53f, 0.95f, 0.72f)]
        public void InitProject()
        {
            try
            {
                Debug.Log("[ProjectInit] ========== 开始项目初始化 ==========");

                // 1. 设置 SceneName
                new SetSceneNameCommand(SceneName == "(未加载)" ? "Main" : SceneName).Execute();
                Debug.Log("[ProjectInit] 设置 SceneName 完成");

                // 2. 链接 SLN
                new LinkSlnCommand().Execute();
                Debug.Log("[ProjectInit] 链接 SLN 完成");

                // 3. 刷新 4 个程序集引用
                new UpdateScriptsReferencesCommand().Execute();
                Debug.Log("[ProjectInit] 更新 Scripts 引用完成");

                // 4. 切换到 ClientServer
                new ChangeCodeModeCommand("ClientServer").Execute();
                Debug.Log("[ProjectInit] 切换 CodeMode 到 ClientServer 完成");

                // 5. 运行初始化脚本
                new RunInitScriptsCommand().Execute();
                Debug.Log("[ProjectInit] 运行初始化脚本完成");

                // 6. 启用 INITED 宏
                new EnableDefineCommand("INITED").Execute();
                Debug.Log("[ProjectInit] 启用 INITED 宏定义完成");

                Debug.Log("[ProjectInit] ========== 项目初始化完成 ==========");

                // 重新加载一次状态
                LoadPackageInfo();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ProjectInit] 项目初始化失败: {ex.Message}");
            }
        }

        public override void Initialize()
        {
            LoadPackageInfo();
        }

        public override void OnDestroy()
        {
        }

        private void LoadPackageInfo()
        {
            try
            {
                PackageName = ProjectInitCommand.FindPackageName();
                UnityEditor.AssetDatabase.Refresh();

                var assetPath = "Packages/cn.rendu.loader/Resources/GlobalConfig.asset";
                var globalConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<GlobalConfig>(assetPath);
                if (globalConfig != null)
                {
                    SceneName = globalConfig.SceneName;
                    CodeMode  = globalConfig.CodeMode;
                    Address   = globalConfig.Address;
                }
                else
                {
                    SceneName = "Main";
                    CodeMode  = CodeMode.Client;
                    Address   = "";
                }

                bool isInited = ProjectInitCommand.CheckIsInited();
                InitStatus = isInited ? "✅ 已初始化" : "❌ 未初始化";
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ProjectInit] 加载包信息失败: {ex.Message}");
                SceneName  = "Main";
                CodeMode   = CodeMode.Client;
                Address    = "";
                InitStatus = "(加载失败)";
            }
        }
    }
}

#endif
