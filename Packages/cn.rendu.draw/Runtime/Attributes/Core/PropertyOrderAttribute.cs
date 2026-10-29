using System;

namespace RD.Draw
{
    /// <summary>控制成员在绘制面板中的排序权重（越小越靠前，未标按声明序）。宿主无关（检视器 / 工具窗口 / 任意 DrawObject 宿主）。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class PropertyOrderAttribute : Attribute
    {
        public double Order { get; set; }

        public PropertyOrderAttribute(double order)
        {
            Order = order;
        }
    }
}
