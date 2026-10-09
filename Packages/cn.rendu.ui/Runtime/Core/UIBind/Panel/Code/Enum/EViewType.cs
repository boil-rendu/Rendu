using RD.Inspector;
using RD.Core;

namespace RD.Client
{
    //不要修改值 否则已存在的界面会错误
    //只能新增 不允许修改
    /// <summary>
    /// 视图层级类型（EViewWindowType）
    /// View=窗口 / Popup=弹窗；注意值不可修改、只能新增
    /// </summary>
    [LabelText("层级类型")]
    public enum EViewWindowType
    {
        /// <summary>窗口</summary>
        [LabelText("窗口")]
        View = 0,

        /// <summary>弹窗</summary>
        [LabelText("弹窗")]
        Popup = 1,
    }
}
