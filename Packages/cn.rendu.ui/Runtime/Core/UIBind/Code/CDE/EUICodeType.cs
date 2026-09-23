using RD.Inspector;
using RD.Core;

namespace RD.Client
{
    [LabelText("组件类型")]
    public enum EUICodeType
    {
        [LabelText("Panel 面板")]
        Panel,

        [LabelText("View 界面")]
        View,

        [LabelText("Common 组件")]
        Common,
    }
}
