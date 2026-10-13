using RD.Init.Editor;
using RD.EditTool.Editor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 启用宏定义命令
    /// </summary>
    public class EnableDefineCommand : CommandBase
    {
        private readonly string _defineSymbol;

        public EnableDefineCommand(string defineSymbol)
        {
            _defineSymbol = defineSymbol;
        }

        public override void Execute()
        {
            DefineCommand.EnableDefineSymbols(_defineSymbol, true);
            UnityEngine.Debug.Log($"[EnableDefine] 启用宏定义: {_defineSymbol}");
        }
    }
}
