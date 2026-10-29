using System;
using UnityEngine;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// Unity 对象引用: 用对象选择器, 而不是 <c>StructField</c>。
    ///
    /// 单独成档的原因: 它不是值类型, 塞进 StructField 那档会走错控件分支。
    /// </summary>
    internal sealed class UnityObjectValueControl : ValueControlBase
    {
        public UnityObjectValueControl(WidgetBuilder builder) : base(builder)
        {
        }

        public override bool CanHandle(in ValueDrawArgs args)
        {
            return typeof(UnityEngine.Object).IsAssignableFrom(args.Type);
        }

        public override void Build(in ValueDrawArgs args)
        {
            Builder.ObjectField(args.Target, args.Member, args.Label, args.Value as UnityEngine.Object, args.Type, 0f);
        }
    }
}