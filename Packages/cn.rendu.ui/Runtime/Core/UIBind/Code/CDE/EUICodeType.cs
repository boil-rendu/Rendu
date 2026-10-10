using RD.EditorKit;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI 组件（CDE）类型
    /// 标识一个 UIBindCDETable 承载的是 Panel（面板） / View（界面） / Common（通用组件）
    /// </summary>
    [LabelText("组件类型")]
    public enum EUICodeType
    {
        /// <summary>Panel 面板</summary>
        [LabelText("Panel 面板")]
        Panel,

        /// <summary>View 界面</summary>
        [LabelText("View 界面")]
        View,

        /// <summary>Common 通用组件</summary>
        [LabelText("Common 组件")]
        Common,
    }
}
