using System;

namespace RD.Draw
{
    /// <summary>强制在绘制面板中显示该字段/属性（含非序列化成员）。宿主无关（检视器 / 工具窗口 / 任意 DrawObject 宿主）。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class ShowInInspectorAttribute : Attribute
    {
    }
}
