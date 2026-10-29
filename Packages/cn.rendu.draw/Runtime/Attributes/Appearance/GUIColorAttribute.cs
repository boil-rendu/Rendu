using System;
using UnityEngine;

namespace RD.Draw
{
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
}
