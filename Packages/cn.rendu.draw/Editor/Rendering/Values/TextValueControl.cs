using System;
using UnityEditor;
using UnityEngine;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 字符串: 输入框 + 可选的路径选择按钮([FilePath] / [FolderPath])。
    ///
    /// 它同时是所有未命中其它画法的类型的**兜底**: 兜底不放进 <c>CanHandle</c> 里
    /// (那会让"我没匹配上"与"我就是兜底"两件事挤在一个布尔里), 而是由
    /// <see cref="ValueControlChain"/> 在没有任何策略命中时显式调用它。
    /// </summary>
    internal sealed class TextValueControl : ValueControlBase
    {
        /// <summary>路径选择按钮的宽度。</summary>
        private const float PathPickerButtonWidth = 26f;

        private readonly DrawContext m_Ctx;
        private readonly ValueAccessor m_Values;

        public TextValueControl(WidgetBuilder builder, DrawContext ctx, ValueAccessor values) : base(builder)
        {
            m_Ctx = ctx;
            m_Values = values;
        }

        /// <summary>只接管 string; 其余类型作为兜底由注册表直接调用 <see cref="Build"/>, 不走这里。</summary>
        public override bool CanHandle(in ValueDrawArgs args) => args.Type == typeof(string);

        public override void Build(in ValueDrawArgs args)
        {
            var member = args.Member;
            var target = args.Target;
            var file   = member.GetAttr<FilePathAttribute>();
            var folder = member.GetAttr<FolderPathAttribute>();

            Builder.TextField(args.Target, args.Member, args.Label, args.Value as string ?? "", 0f);

            if (file == null && folder == null) return;

            // 路径选择按钮: 点击 → 同步弹窗 → 立即写回(不经过 ChangeApplier)。
            Builder.Button("…", DrawButtonSpec.Of(width: PathPickerButtonWidth, expandWidth: false), () =>
            {
                var start = folder != null ? folder.Parent : file.ParentFolder;
                if (string.IsNullOrEmpty(start)) start = Application.dataPath;

                string result = null;
                if (folder != null)
                {
                    var dir = EditorUtility.OpenFolderPanel("选择文件夹", start, "");
                    if (string.IsNullOrEmpty(dir) == false) result = ToAssetPath(dir);
                }
                else
                {
                    var ext = file.Extensions ?? "";
                    ext = ext.TrimStart('.').Replace("|", ";").Replace(",", ";");
                    if (string.IsNullOrEmpty(ext)) ext = "*";
                    var sel = EditorUtility.OpenFilePanel("选择文件", start, ext);
                    if (string.IsNullOrEmpty(sel) == false) result = ToAssetPath(sel);
                }

                if (result == null) return;

                m_Ctx.Mutate(target, () => m_Values.Write(member, target, result));
                foreach (var attr in member.GetAttrs<OnValueChangedAttribute>())
                {
                    var method = m_Values.FindMethod(target, attr.MethodName);
                    if (method != null) m_Values.InvokeSafe(target, method);
                }
                m_Ctx.Driver.RequestRepaint();
            });
        }

        /// <summary>把绝对路径转成工程内相对路径(落在 Assets 之外的保持原样)。</summary>
        private static string ToAssetPath(string fullPath)
        {
            var full = fullPath.Replace('\\', '/');
            var data = Application.dataPath.Replace('\\', '/');
            if (full.StartsWith(data, StringComparison.OrdinalIgnoreCase))
            {
                return "Assets" + full.Substring(data.Length);
            }

            return full;
        }
    }
}