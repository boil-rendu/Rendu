using RD.Init.Editor;
using RD.EditTool.Editor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 切换代码模式命令
    /// </summary>
    public class ChangeCodeModeCommand : CommandBase
    {
        private readonly string _codeMode;

        public ChangeCodeModeCommand(string codeMode)
        {
            _codeMode = codeMode;
        }

        public override void Execute()
        {
            ProjectInitCommand.ChangeCodeMode(_codeMode);
            UnityEngine.Debug.Log($"[ChangeCodeMode] 切换到: {_codeMode}");
        }
    }
}
