using System;

namespace RD.Draw
{
    /// <summary>
    /// 把数值成员按进度条显示(取值按 0~1 归一), 而不是输入框 —— 用于导出/构建这类有进度的操作。
    /// 成员类型可以是 float / double / int, 超出 0~1 的部分会被截断显示。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ProgressBarAttribute : Attribute
    {
        /// <summary>条上的文字; 为空时用成员名, 再为空时显示百分比。</summary>
        public string Title;

        /// <summary>条的高度; 0 = 用后端默认的一行高。</summary>
        public float Height;

        public ProgressBarAttribute(string title = null, float height = 0f)
        {
            Title  = title;
            Height = height;
        }
    }
}
