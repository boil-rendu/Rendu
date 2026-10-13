#if UNITY_EDITOR

using System.IO;
using RD.Panel;
using RD.EditorWindows.Editor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 项目文件管理模块: 顶部 Sln 状态指示 + 6 个动作按钮(两组横排)。
    /// 替代原 ProjectFilesWindow + ProjectFilesViewModel + ProjectFilesModel + ProjectFilesWindow.uxml。
    /// </summary>
    [UIAutoMenu("项目文件", 400, Scope = "loader")]
    public class ProjectFilesModule : BaseToolModule
    {
        private const string SlnLinkedText   = "<color=#4CAF50>✅ 已链接</color>";
        private const string SlnMissingText  = "<color=#E2696C>❌ 未链接</color>";

        [LabelText("Sln 状态")]
        [ShowInInspector]
        [DisplayAsString(false)]
        public string SlnStatus { get; private set; } = SlnMissingText;

        [ButtonGroup("项目文件")]
        [Button("链接 Sln", Tooltip = "链接解决方案到 Rider/VS")]
        public void LinkSln()
        {
            try
            {
                new LinkSlnCommand().Execute();
                RefreshSlnStatus();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ProjectFiles] Sln 链接失败: {ex.Message}");
            }
        }

        [ButtonGroup("项目文件")]
        [Button("重新生成项目文件")]
        public void Regenerate()
        {
            try
            {
                new RegenerateProjectFilesCommand().Execute();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ProjectFiles] 重新生成项目文件失败: {ex.Message}");
            }
        }

        [ButtonGroup("项目文件")]
        [Button("刷新资源数据库")]
        public void RefreshDatabase()
        {
            try
            {
                new RefreshAssetDatabaseCommand().Execute();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ProjectFiles] 刷新资源失败: {ex.Message}");
            }
        }

        [ButtonGroup("程序集引用")]
        [Button("生成程序集引用", Tooltip = "生成 CodeMode.Client 模式的 AssemblyReference")]
        public void GenerateReferences()
        {
            try
            {
                new GenerateAssemblyReferenceCommand(CodeMode.Client).Execute();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ProjectFiles] 生成引用失败: {ex.Message}");
            }
        }

        [ButtonGroup("程序集引用")]
        [Button("更新脚本引用")]
        public void UpdateScriptsReferences()
        {
            try
            {
                new UpdateScriptsReferencesCommand().Execute();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ProjectFiles] 更新脚本引用失败: {ex.Message}");
            }
        }

        [ButtonGroup("程序集引用")]
        [Button("运行初始化脚本")]
        public void RunInitScripts()
        {
            try
            {
                new RunInitScriptsCommand().Execute();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ProjectFiles] 初始化脚本失败: {ex.Message}");
            }
        }

        [Button("列出被忽略的 csproj", Tooltip = "在 Console 输出当前 Ignore.RD*.csproj 列表")]
        public void ListIgnored()
        {
            var files = ProjectFilesCommand.GetIgnoredCsprojFiles();
            if (files.Length == 0)
            {
                Debug.Log("[ProjectFiles] 当前没有 Ignore.RD*.csproj 文件");
                return;
            }
            Debug.Log($"[ProjectFiles] 找到 {files.Length} 个被忽略的 csproj:\n  " + string.Join("\n  ", files));
        }

        public override void Initialize()
        {
            RefreshSlnStatus();
        }

        public override void OnDestroy()
        {
        }

        private void RefreshSlnStatus()
        {
            string slnPath = Path.Combine(Directory.GetCurrentDirectory(), "Rendu.sln");
            SlnStatus = File.Exists(slnPath) ? SlnLinkedText : SlnMissingText;
        }
    }
}

#endif
