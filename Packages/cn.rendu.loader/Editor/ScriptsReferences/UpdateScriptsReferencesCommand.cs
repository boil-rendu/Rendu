using RD.Init.Editor;
using RD.EditTool.Editor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 更新 Scripts 引用命令
    /// </summary>
    public class UpdateScriptsReferencesCommand : CommandBase
    {
        public override void Execute()
        {
            UnityEngine.Debug.Log("[UpdateScriptsReferences] 更新 Scripts 引用");
            ScriptsReferencesCommand.SaveAndRefresh();
            UnityEngine.Debug.Log("[UpdateScriptsReferences] Scripts 引用更新完成");
        }
    }
}
