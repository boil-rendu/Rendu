using System;

namespace RD.Panel
{
    /// <summary>限制数值最小值。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class MinValueAttribute : Attribute
    {
        public double MinValue { get; set; }
        public string MemberName { get; set; }

        public MinValueAttribute(double minValue)
        {
            MinValue = minValue;
        }

        public MinValueAttribute(string memberName)
        {
            MemberName = memberName;
        }
    }

    /// <summary>限制数值最大值。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class MaxValueAttribute : Attribute
    {
        public double MaxValue { get; set; }
        public string MemberName { get; set; }

        public MaxValueAttribute(double maxValue)
        {
            MaxValue = maxValue;
        }

        public MaxValueAttribute(string memberName)
        {
            MemberName = memberName;
        }
    }

    /// <summary>设置属性标签宽度。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class LabelWidthAttribute : Attribute
    {
        public float Width { get; set; }

        public LabelWidthAttribute(float width)
        {
            Width = width;
        }
    }

    /// <summary>设置 GUI 颜色。支持颜色字符串（如 "1,0,0,1"）、方法名（返回 Color）或 RGB(A) 分量。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class GUIColorAttribute : Attribute
    {
        public string ColorString { get; set; }
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }
        public string MethodName { get; set; }

        public GUIColorAttribute(string color)
        {
            ColorString = color;
            A = 1f;
        }

        public GUIColorAttribute(float r, float g, float b, float a = 1f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }

    /// <summary>字段值校验：为空/默认值时在 Inspector 中报错。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class RequiredAttribute : Attribute
    {
        public string ErrorMessage { get; set; }
        public InfoMessageType MessageType { get; set; }

        public RequiredAttribute(string errorMessage = null, InfoMessageType infoMessageType = InfoMessageType.Error)
        {
            ErrorMessage = errorMessage;
            MessageType = infoMessageType;
        }
    }

    /// <summary>仅在编辑模式下显示。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class HideInPlayModeAttribute : Attribute
    {
    }

    /// <summary>仅在播放模式下显示。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class HideInEditorModeAttribute : Attribute
    {
    }
}
