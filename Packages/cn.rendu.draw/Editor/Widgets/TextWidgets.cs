namespace RD.Draw.Editor
{
    /// <summary>一行文本: 对应 <c>Label</c>。</summary>
    internal sealed class LabelWidget : Widget
    {
        public string       Text;
        public DrawTextSpec Spec;

        public override void Reset()
        {
            Text = null;
            Spec = default;
        }
    }

    /// <summary>"标签 + 值"两栏一行: 对应 <c>KeyValueLabel</c>。</summary>
    internal sealed class KeyValueLabelWidget : Widget
    {
        public string Label;
        public string Value;

        public override void Reset()
        {
            Label = null;
            Value = null;
        }
    }

    /// <summary>提示框([Required] 校验与 [InfoBox]): 对应 <c>HelpBox</c>。</summary>
    internal sealed class HelpBoxWidget : Widget
    {
        public string          Message;
        public DrawMessageKind Kind;

        public override void Reset()
        {
            Message = null;
            Kind    = default;
        }
    }

    /// <summary>进度条([ProgressBar]): 对应 <c>ProgressBar</c>。</summary>
    internal sealed class ProgressBarWidget : Widget
    {
        public float  Value;   // 0~1 归一
        public string Title;
        public float  Height;  // 0 = 默认行高

        public override void Reset()
        {
            Value  = 0f;
            Title  = null;
            Height = 0f;
        }
    }
}
