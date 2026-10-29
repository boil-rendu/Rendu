using System;

namespace RD.Draw
{
    /// <summary>修改成员在绘制面板中显示的标签名。宿主无关（检视器 / 工具窗口 / 任意 DrawObject 宿主）。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class LabelTextAttribute : Attribute
    {
        public string Text { get; set; }

        public LabelTextAttribute(string text)
        {
            Text = text;
        }
    }
}
