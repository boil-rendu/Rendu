using System;
using UnityEngine;

namespace RD.Draw.Editor
{
    /// <summary>按钮: 对应 <c>Button</c>。<see cref="Key"/> 是引擎给的稳定标识(点击回填用)。</summary>
    internal sealed class ButtonWidget : ControlWidget
    {
        public string         Text;
        public DrawButtonSpec Spec;
        public int            Key;

        public override void Reset()
        {
            Text = null;
            Spec = default;
            Key  = 0;
        }
    }

    /// <summary>折叠头: 对应 <c>Foldout</c>。<see cref="Expanded"/> 是输入+输出(会话持久化)。</summary>
    internal sealed class FoldoutWidget : ControlWidget
    {
        public bool   Expanded;
        public string Label;

        public override void Reset()
        {
            Expanded = false;
            Label    = null;
        }
    }

    /// <summary>字符串输入: 对应 <c>TextField</c>。</summary>
    internal sealed class TextFieldWidget : ControlWidget
    {
        public string Label;
        public string Value;   // 输入+输出
        public float  Width;

        public override void Reset()
        {
            Label = null;
            Value = null;
            Width = 0f;
        }
    }

    /// <summary>整数输入: 对应 <c>IntegerField</c>。</summary>
    internal sealed class IntegerFieldWidget : ControlWidget
    {
        public string Label;
        public long   Value;   // 输入+输出
        public float  Width;

        public override void Reset()
        {
            Label = null;
            Value = 0L;
            Width = 0f;
        }
    }

    /// <summary>浮点输入: 对应 <c>FloatField</c>。</summary>
    internal sealed class FloatFieldWidget : ControlWidget
    {
        public string Label;
        public double Value;   // 输入+输出
        public float  Width;

        public override void Reset()
        {
            Label = null;
            Value = 0d;
            Width = 0f;
        }
    }

    /// <summary>滑杆: 对应 <c>Slider</c>。</summary>
    internal sealed class SliderWidget : ControlWidget
    {
        public string Label;
        public float  Value;   // 输入+输出
        public float  Min;
        public float  Max;
        public float  Width;

        public override void Reset()
        {
            Label = null;
            Value = 0f;
            Min   = 0f;
            Max   = 0f;
            Width = 0f;
        }
    }

    /// <summary>复选框: 对应 <c>ToggleField</c>。</summary>
    internal sealed class ToggleWidget : ControlWidget
    {
        public string Label;
        public bool   Value;   // 输入+输出
        public float  Width;

        public override void Reset()
        {
            Label = null;
            Value = false;
            Width = 0f;
        }
    }

    /// <summary>下拉框: 对应 <c>Popup</c>。</summary>
    internal sealed class PopupWidget : ControlWidget
    {
        public string   Label;
        public int      Selected;   // 输入+输出
        public string[] Options;
        public float    Width;

        public override void Reset()
        {
            Label    = null;
            Selected = 0;
            Options  = null;
            Width    = 0f;
        }
    }

    /// <summary>按钮组(单选): 对应 <c>Toolbar</c>。</summary>
    internal sealed class ToolbarWidget : ControlWidget
    {
        public int      Selected;   // 输入+输出
        public string[] Options;
        public float    Width;

        public override void Reset()
        {
            Selected = 0;
            Options  = null;
            Width    = 0f;
        }
    }

    /// <summary>多选按钮组: 对应 <c>ToolbarMulti</c>。</summary>
    internal sealed class ToolbarMultiWidget : ControlWidget
    {
        public bool[]   States;            // 输入(当前亮灭)
        public int      ClickedIndex = -1; // 输出: 渲染器回填的被点击下标(-1 = 未点击)
        public string[] Options;
        public float    Width;

        // 注意 ClickedIndex 是字段初始化器给的: 池里借出来的实例不跑构造, 必须在 Reset 里补上,
        // 否则复用时会带着上一帧"被点击的下标"直接改写成员值。
        public override void Reset()
        {
            States       = null;
            ClickedIndex = -1;
            Options      = null;
            Width        = 0f;
        }
    }

    /// <summary>[Flags] 枚举多选: 对应 <c>EnumFlagsField</c>。</summary>
    internal sealed class EnumFlagsWidget : ControlWidget
    {
        public string Label;
        public Enum   Value;    // 输入+输出
        public float  Width;

        public override void Reset()
        {
            Label = null;
            Value = null;
            Width = 0f;
        }
    }

    /// <summary>Unity 对象引用: 对应 <c>ObjectField</c>。</summary>
    internal sealed class ObjectFieldWidget : ControlWidget
    {
        public string  Label;
        public UnityEngine.Object Value;   // 输入+输出
        public Type    Type;
        public float   Width;

        public override void Reset()
        {
            Label = null;
            Value = null;
            Type  = null;
            Width = 0f;
        }
    }

    /// <summary>复合值类型控件(Vector3/Quaternion/Rect/...): 对应 <c>StructField</c>。</summary>
    internal sealed class StructFieldWidget : ControlWidget
    {
        public string Label;
        public object Value;    // 输入+输出
        public Type   Type;
        public float  Width;

        public override void Reset()
        {
            Label = null;
            Value = null;
            Type  = null;
            Width = 0f;
        }
    }
}
