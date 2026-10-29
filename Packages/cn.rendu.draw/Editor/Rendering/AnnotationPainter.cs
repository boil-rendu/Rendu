using UnityEngine;
using RD.Draw.Editor.Metadata;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 批注绘制: 标题([Title])、提示批注([InfoBox]/[DetailedInfoBox])。
    ///
    /// 它**不是渲染器**, 而是被注入到多个渲染器里的**绘制能力**: 批注是"贴在成员旁边的说明",
    /// 与成员本身的值无关, 所以谁都不必知道批注怎么画, 只知道"在前面画一段批注"。
    /// 叫 Painter 而不是 Renderer, 正是为了和"渲染一段结构"的那些类区分开
    /// (命名约定见 <see cref="ValueMemberRenderer"/> 的类注释)。
    /// </summary>
    internal sealed class AnnotationPainter
    {
        private readonly DrawContext m_Ctx;
        private readonly WidgetBuilder m_Builder;
        private readonly VisibilityEvaluator m_Visibility;
        private readonly ILabelFormatter m_Labels;

        public AnnotationPainter(DrawContext ctx, WidgetBuilder builder, VisibilityEvaluator visibility,
                                 ILabelFormatter labels)
        {
            m_Ctx        = ctx;
            m_Builder    = builder;
            m_Visibility = visibility;
            m_Labels     = labels;
        }

        /// <summary>画成员上的标题与提示批注(在成员的值之前)。</summary>
        public void PaintTitlePrelude(object target, CachedMember member)
        {
            foreach (var attr in member.Attrs)
            {
                switch (attr)
                {
                    case TitleAttribute title:
                        if (string.IsNullOrEmpty(title.Title) == false)
                        {
                            // Split: 主标题靠左、副标题同一行靠右(没有副标题时退化为 Left)
                            if (title.Alignment == TitleAlignment.Split &&
                                string.IsNullOrEmpty(title.Subtitle) == false)
                            {
                                m_Builder.BeginHorizontal();
                                m_Builder.Label(title.Title, DrawTextSpec.Of(DrawAlign.Left, title.Bold));
                                m_Builder.FlexibleSpace();
                                m_Builder.Label(title.Subtitle, DrawTextSpec.Of(DrawAlign.Right, mini: true));
                                m_Builder.EndHorizontal();
                            }
                            else
                            {
                                var anchor = m_Labels.TitleAnchor(title.Alignment);
                                m_Builder.Label(title.Title, DrawTextSpec.Of(anchor, title.Bold));
                                if (string.IsNullOrEmpty(title.Subtitle) == false)
                                {
                                    m_Builder.Label(title.Subtitle, DrawTextSpec.Of(anchor, mini: true));
                                }
                            }

                            if (title.HorizontalLine)
                            {
                                m_Builder.Separator(RDTheme.TitleGroupLineThickness, RDTheme.TitleGroupLineColor);
                            }

                            m_Builder.Space(4);
                        }

                        break;

                    case InfoBoxAttribute box:
                        if (m_Visibility.IsAnnotationVisible(box.VisibleIfMemberName, target) == false) break;
                        m_Builder.HelpBox(box.Message, ToMessageKind(box.InfoMessageType));
                        break;

                    case DetailedInfoBoxAttribute detail:
                        if (m_Visibility.IsAnnotationVisible(detail.VisibleIfMemberName, target) == false) break;
                        m_Builder.HelpBox(detail.Message + "\n" + detail.Details,
                                          ToMessageKind(detail.InfoMessageType));
                        break;
                }
            }
        }

        /// <summary>提示框信息等级 → 绘制层的提示等级。</summary>
        public static DrawMessageKind ToMessageKind(InfoMessageType type)
        {
            switch (type)
            {
                case InfoMessageType.Info:    return DrawMessageKind.Info;
                case InfoMessageType.Warning: return DrawMessageKind.Warning;
                case InfoMessageType.Error:   return DrawMessageKind.Error;
                default:                      return DrawMessageKind.None;
            }
        }

        /// <summary>对齐枚举 → 绘制层的文本规格(只读文本: 自动换行 + 富文本)。</summary>
        public static DrawTextSpec ToTextSpec(TextAlignment alignment, bool wrap = false, bool richText = false,
                                              bool bold = false, bool mini = false)
        {
            DrawAlign panelAlign;
            switch (alignment)
            {
                case TextAlignment.Center: panelAlign = DrawAlign.Center; break;
                case TextAlignment.Right:  panelAlign = DrawAlign.Right; break;
                default:                  panelAlign = DrawAlign.Left; break;
            }

            return DrawTextSpec.Of(panelAlign, bold, mini, wrap, richText);
        }
    }
}
