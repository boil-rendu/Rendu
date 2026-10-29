using System;

namespace RD.Draw.Editor
{
    /// <summary>
    /// Inspector 属性状态（对应表达式 $property.State 语义）。
    /// 由引擎在绘制时构造，Expanded 等读写会联动实际折叠状态。
    /// </summary>
    public sealed class PropertyState
    {
        public bool Expanded = true;
        public bool Selected;
        public bool Visible = true;
        public bool Enabled = true;
    }

    /// <summary>
    /// 表达式求值上下文。
    /// Target=当前绘制对象(this)；Value=当前成员当前值($value)；State=当前成员状态($property.State)。
    /// </summary>
    public sealed class ExpressionContext
    {
        public object Target;
        public object Value;
        public PropertyState State = new PropertyState();
    }

    /// <summary>
    /// Inspector 表达式求值器。
    /// 支持: @ 前缀、$property/$value/$root/this、成员访问链、方法调用(实例/静态/全局静态类)、
    /// 括号、比较/逻辑/算术、一元、三元、赋值(如 $property.State.Expanded = true)。
    /// </summary>
}
