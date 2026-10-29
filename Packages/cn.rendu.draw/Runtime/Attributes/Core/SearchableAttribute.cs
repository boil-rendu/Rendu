using System;

namespace RD.Draw
{
    /// <summary>
    /// 成员搜索（永久占位特性, 引擎有意不实现 —— 标注后无任何效果）。
    /// 特性名保留以兼容 Odin 迁移代码; 需要搜索请自行在宿主层实现。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class SearchableAttribute : Attribute
    {
        public bool RecursiveFilter { get; set; }

        public SearchableAttribute()
        {
            RecursiveFilter = true;
        }
    }
}
