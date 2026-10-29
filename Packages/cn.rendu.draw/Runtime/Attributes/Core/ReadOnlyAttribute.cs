using System;

namespace RD.Draw
{
    /// <summary>
    /// 只读: 控件画成禁用态不可编辑; 挂在集合成员上时列表/表格不可增删、不可编辑。
    /// 宿主无关 —— 检视器面板(RD.Inspector)、工具窗口(RD.EditTool)、任何直接调用 Engine.DrawObject 的宿主都生效。
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class ReadOnlyAttribute : Attribute
    {
    }
}
