using RD.Init.Editor;
using RD.EditTool.Editor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 切换宏定义命令
    /// </summary>
    public class ToggleDefineCommand : CommandBase
    {
        private readonly string _defineSymbol;
        private readonly bool _enable;

        public ToggleDefineCommand(string defineSymbol, bool enable)
        {
            _defineSymbol = defineSymbol;
            _enable = enable;
        }

        public override void Execute()
        {
            DefineCommand.EnableDefineSymbols(_defineSymbol, _enable);
            UnityEngine.Debug.Log($"[ToggleDefine] {_defineSymbol} = {_enable}");
        }
    }
}
