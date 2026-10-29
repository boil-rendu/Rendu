using System;

namespace RD.Draw
{
    /// <summary>隐藏表格内的绘制: 标了它的成员在 [TableList] 表格里既不单独成列、也不出现在所属分组的单元格内(普通列表与面板下照常显示)。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class HideInTablesAttribute : Attribute
    {
    }
}
