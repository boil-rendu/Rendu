using RD.Init.Editor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 重新生成项目文件命令
    /// </summary>
    public class RegenerateProjectFilesCommand : CommandBase
    {
        public override void Execute()
        {
            UnityEngine.Debug.Log("[ProjectFiles] ========== 开始重新生成项目文件 ==========");
            int deletedCount = ProjectFilesCommand.ReGenerateProjectFiles();
            UnityEngine.Debug.Log($"[ProjectFiles] 删除了 {deletedCount} 个忽略的 csproj 文件");
            UnityEngine.Debug.Log("[ProjectFiles] ========== 项目文件生成完成 ==========");
        }
    }
}
