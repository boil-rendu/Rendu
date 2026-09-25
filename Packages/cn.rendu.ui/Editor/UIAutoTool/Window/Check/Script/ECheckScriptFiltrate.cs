using RD.Inspector;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    [HideLabel]
    public enum ECheckScriptFiltrate
    {
        [LabelText("所有")]
        All = 0,

        [LabelText("可删除")]
        Delete = 1,

        [LabelText("忽略")]
        Igonre = 2
    }
}