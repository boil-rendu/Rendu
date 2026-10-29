using System;
using UnityEngine;

namespace RD.Draw
{
    /// <summary>隐藏 MonoBehaviour 脚本体（Script 栏）。（永久占位: 引擎从不绘制 Script 引用栏、没有可隐藏的东西, 故有意不实现; 特性名保留以兼容 Odin 迁移代码）</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class HideMonoScriptAttribute : Attribute
    {
    }
}
