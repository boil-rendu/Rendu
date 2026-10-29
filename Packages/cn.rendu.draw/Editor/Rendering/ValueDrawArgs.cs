using System;
using RD.Draw.Editor.Metadata;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// "画这个值"这件事的全部输入。
    ///
    /// 打包成一个不可变结构体, 而不是给每个 <see cref="IValueControl"/> 的 Draw 一长串参数 ——
    /// 这样新增一种控件策略不需要改动任何已有策略的签名。
    /// </summary>
    internal readonly struct ValueDrawArgs
    {
        public ValueDrawArgs(object target, CachedMember member, Type type, object value, string label)
        {
            Target = target;
            Member = member;
            Type   = type;
            Value  = value;
            Label  = label;
        }

        /// <summary>所属宿主对象(按钮类控件要靠它触发宿主回调, 例如路径选择框)。</summary>
        public object Target { get; }

        /// <summary>成员元数据: 读特性(<c>[FilePath]</c>/<c>[MinValue]</c>/…)、写值都靠它。</summary>
        public CachedMember Member { get; }

        /// <summary>值的声明类型 —— 决定用哪个控件, 不取 <c>value?.GetType()</c>。</summary>
        public Type Type { get; }

        /// <summary>当前值(可能为 null)。</summary>
        public object Value { get; }

        /// <summary>控件标签(已按表格单元格/[HideLabel] 处理好, 可能为空串)。</summary>
        public string Label { get; }
    }
}