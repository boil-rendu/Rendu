#if UNITY_EDITOR

using System.IO;
using RD.Panel;
using RD.EditorWindows.Editor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 项目文件管理模块: 6 个动作按钮 — 链接 Sln、重新生成项目文件、刷新资源库、
    /// 生成程序集引用、更新脚本引用、运行初始化脚本。顶部一个 Sln 状态指示。
    /// 替代原 ProjectFilesWindow + ProjectFilesViewModel + ProjectFilesModel + ProjectFilesWindow.uxml。
    /// </summary>
    [UIAutoMenu("项目文件", 400, Scope = "loader")]
    public class ProjectFilesModule : BaseToolModule
    {
        [LabelText("Sln 状态")]
        [ReadOnly]
        [ShowInInspector]
        [GUIColor(0.9f, 0.4f, 0.4f)]
        public string SlnStatus { get; private set; } = "Sln 状态：❌ 未链接";

        [Button("链接 Sln (Rider/VS)", 10)]
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

        [Button("重新生成项目文件", 20)]
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

        [Button("刷新资源数据库", 30)]
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

        [Button("生成 AssemblyReference (Client 模式)", 40)]
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

        [Button("更新脚本引用", 50)]
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

        [Button("运行初始化脚本", 60)]
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

        [Button("列出被忽略的 csproj", 70)]
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
            bool isLinked = File.Exists(slnPath);
            SlnStatus = isLinked ? "Sln 状态：✅ 已链接" : "Sln 状态：❌ 未链接";
        }
    }
}

#endif
