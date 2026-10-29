using UnityEngine;
using UnityEngine.UIElements;

namespace RD.Draw.Editor
{
    /// <summary>
    /// 后端的样式施加: 把状态栈顶的值与 观感落到元素上。
    ///
    /// 它单独成类, 而不是 <see cref="Driver"/> 的一个私有方法, 因为**每个被复用的元素都要过这里**:
    /// 按位置复用元素, 不重刷就会把上一轮的颜色、禁用态、标签列宽留在新控件上
    /// (表现是整个面板点不动, 或标签列钉歪)。这段逻辑必须与"取元素"那条路绑在一起, 两者一起搬出来,
    /// 免得留在 Driver 里和 Take 隔着几百行互相调用。
    ///
    /// 颜色压暗那套系数是对齐  观感的经验值, 与后端选择无关 —— 换后端不该改这些数字。
    /// </summary>
    internal sealed class Styler
    {
        /// <summary>状态栈: 决定当前该刷什么颜色、该不该禁用、标签列多宽。</summary>
        private readonly StateStack m_StateStack;

        public Styler(StateStack stateStack)
        {
            m_StateStack = stateStack;
        }

        /// <summary>行与行之间的空隙(取  standardVerticalSpacing)。</summary>
        private static float RowSpacing => RDTheme.RowSpacing;

        /// <summary>标签列宽的下限(EditorGUIUtility.labelWidth 取不到时用。</summary>
        private const float DefaultLabelWidth = RDTheme.LabelWidthDefault;

        /// <summary>标签列宽的上限(再宽的面板标签列也不至于拉得太开)。</summary>
        private const float MaxLabelWidth = RDTheme.LabelWidthMax;

        /// <summary>
        /// 输入区的文字左留白按 0 统一: 字段紧贴标签列右边缘,
        /// 加留白反而与右边缘错开。各控件 USS 自带的 padding 不一致由 USS 自己处理。
        /// </summary>
        internal const float InputPaddingLeft = RDTheme.InputPaddingLeft;

        /// <summary>
        /// 单行字段的最小高度。正常有标签的字段行高由标签行盒撑起; HideLabel 的字段标签没有内容,
        /// 这条主题下输入区自己不带固有高, 整行会塌成一条薄片(弹窗文字被裁半截)。
        /// 这里给所有字段一个与主题单行控件一致的硬下限兜底, 与旁边正常行同高。
        /// </summary>
        // 这几个是现场量 运行时值: 用属性而非 static readonly, 免得在样式体系就绪前
        // 就被初始化成兜底值、之后再也不刷新
        internal static float FieldMinHeight => RDTheme.FieldMinHeight;

        /// <summary>跟随 EditorGUIUtility.labelWidth(标签列宽), 不自己算比例。</summary>
        internal static float AutoLabelWidth => Mathf.Clamp(RDTheme.LabelWidth, DefaultLabelWidth, MaxLabelWidth);


        /// <summary>
        /// 控件底纹是乘在纹理上的: 最终像素 = 底色 × 纹理灰度。
        /// UIElements 铺的是纯色, 必须乘同一个灰度才与编辑器控件同色。
        ///
        /// 灰度值一律从皮肤底纹现场量(见 <see cref="RDTheme"/> 的 ButtonTintGray / PopupTintGray / TextFieldTintGray),
        /// 不在这里按控件类型拍压暗系数 —— 换算出来的系数在换皮肤、换主题、换 Unity 版本后都会错,
        /// 而且每个控件一套系数就等于把"同一个算式"抄了很多遍。
        /// </summary>
        private static float ControlTintBrightness => RDTheme.PopupTintGray;

        /// <summary>文本/数字输入框的底纹灰度(比 popup 深一截)。</summary>
        private static float TextTintBrightness => RDTheme.TextFieldTintGray;

        /// <summary>按钮底纹灰度。</summary>
        private static float TintBrightness => RDTheme.ButtonTintGray;

        /// <summary>
        /// 文本/数字类字段的 USS 类。不能用输入区的 "unity-text-field__input" 判别 ——
        /// LongField/IntegerField 的输入区顶层不带它, 判别会落空导致全部走亮档。
        /// 这些类挂在字段元素自身上, 是确定的。
        /// </summary>
        private static readonly string[] TextFieldClasses =
            { "unity-text-field", "unity-long-field", "unity-integer-field", "unity-double-field" };

        /// <summary>把业务色压暗到  控件纹理的等效亮度(保持 alpha)。</summary>
        internal static Color TextureTinted(Color c, float brightness) =>
            new(c.r * brightness, c.g * brightness, c.b * brightness, c.a);

        /// <summary>按钮底色: 与"底色 × 纹理灰度"同一算式, 系数取自皮肤底纹。</summary>
        internal static Color ButtonTinted(Color c) => TextureTinted(c, TintBrightness);

        /// <summary>输入区底色: 按控件类别选压暗档位, 复刻  "下拉框亮、数字/文本输入框暗"的层次。</summary>
        internal static Color InputTinted(VisualElement field, Color tint)
        {
            foreach (var cls in TextFieldClasses)
            {
                if (field.ClassListContains(cls))
                {
                    return TextureTinted(tint, TextTintBrightness);
                }
            }

            return TextureTinted(tint, ControlTintBrightness);
        }

        /// <summary>
        /// 容器/占位元素复用前清掉其他角色留下的残留样式。
        ///
        /// 元素按"位置 + 类型"复用, 同一个位置切页签(或切模块)前后可能是完全不同的东西:
        /// 上一轮的 Space(定高)/Separator(定高+底色)/FlexibleSpace(伸展)/Indent(左内边距)/
        /// 带锁宽的纵向分组, 这一轮作为行或分组复用, 这些值若不清掉, 行就被钉死在别人的尺寸上
        /// —— 定高的行装不下 20px 的字段, 内容溢出来叠到下一行, 切几次页签面板越来越烂。
        /// 各 Begin*/占位方法在调用它之后马上写入自己需要的值, 这里只负责清场。
        /// </summary>
        internal static void ResetContainerResidue(VisualElement element)
        {
            element.style.width           = StyleKeyword.Null;
            element.style.height          = StyleKeyword.Null;
            element.style.maxWidth        = StyleKeyword.Null;
            element.style.maxHeight       = StyleKeyword.Null;
            element.style.flexGrow        = StyleKeyword.Null;
            element.style.flexShrink      = StyleKeyword.Null;
            element.style.alignSelf       = StyleKeyword.Null;
            element.style.marginLeft      = StyleKeyword.Null;
            element.style.marginRight     = StyleKeyword.Null;
            element.style.paddingLeft     = StyleKeyword.Null;
            element.style.paddingRight    = StyleKeyword.Null;
            element.style.paddingTop      = StyleKeyword.Null;
            element.style.paddingBottom   = StyleKeyword.Null;
            element.style.backgroundColor = StyleKeyword.Null;
            // 表头横条会给自己加上下边线: 同一位置的元素下一轮若被复用成普通行/占位,
            // 不清掉就会留下两条横线(表头消失了, 线还在)
            element.style.borderTopWidth    = StyleKeyword.Null;
            element.style.borderBottomWidth = StyleKeyword.Null;
        }

        internal void ApplyState(VisualElement element)
        {
            if (m_StateStack.CurrentColor.HasValue) element.style.color = m_StateStack.CurrentColor.Value;
            else element.style.color = StyleKeyword.Null;

            // 底色会乘进控件纹理(下拉框/输入框的底色一起变色); UIElements 的 color
            // 只影响文字, 这里显式把 BaseField 的输入区底色也染上, 检视器里那排彩色下拉框才对得上。
            // 色栈弹空时要清回默认(StyleKeyword.Null 交还 USS), 否则复用的控件残留上一次的颜色
            if (element.ClassListContains("unity-base-field"))
            {
                var input = element.Q(null, BaseField<string>.inputUssClassName);
                if (input != null)
                {
                    // 底色按控件类别压暗到  纹理乘算的等效亮度; 色栈弹空时清回默认。
                    // 输入区文字也要显式染(USS 对它有显式字色, 继承不进来), 对齐 亮色字
                    input.style.backgroundColor = m_StateStack.CurrentColor.HasValue
                        ? InputTinted(element, m_StateStack.CurrentColor.Value) : StyleKeyword.Null;
                    input.style.color = m_StateStack.CurrentColor.HasValue ? m_StateStack.CurrentColor.Value : StyleKeyword.Null;

                    // 各控件 USS 的 input margin/padding 互不相同(Popup/TextField/Toggle 差几个像素),
                    // 只锁标签宽对不齐输入区起点: 这里归零 + 统一文字左留白, 把起点钉死
                    input.style.marginLeft  = 0f;
                    input.style.paddingLeft = InputPaddingLeft;

                    // 塌行保护: 输入区高度给下限(见 FieldMinHeight), HideLabel 字段不再塌成薄片
                    input.style.minHeight = FieldMinHeight;

                    // flex 项的 min-width 默认是 auto(= 内容宽): 不清零, 值一长输入区就永不收缩,
                    // 一路顶到字段外、压到后面的 × 按钮下面, 最后把整块面板顶出横向滚动条
                    input.style.minWidth = 0f;
                }

                // 标签文字保持默认字色, 不跟着色栈走: 只有输入区染色的观感才和原来一致,
                // 整列字段名被染成彩色会变成另一副样子(色栈弹空后元素是按位置复用的, 所以显式写 Null 清回默认)
                var fieldLabel = element.Q<Label>(null, BaseField<string>.labelUssClassName);
                if (fieldLabel != null) fieldLabel.style.color = StyleKeyword.Null;
            }
            else
            {
                // 普通元素里的第一个 Label(标题/表格头/HelpBox 文字等): 有压栈列宽就对齐它,
                // 没有则清掉 —— 同一位置上一轮可能画过带列宽的别的文字, 残留会把标题钉歪
                var label = element.Q<Label>();
                if (label != null)
                {
                    label.style.minWidth = m_StateStack.LabelWidth.HasValue ? m_StateStack.LabelWidth.Value : StyleKeyword.Null;
                }
            }

            // 禁用态(SetEnabled=false)把控件整体置灰且不收事件; UIElements 的 SetEnabled 同理。
            // 弹空(不在任何 PushEnabled 区间里)时必须显式启用, 不能只写"禁用"不写"启用":
            // 元素按位置复用, 上一轮在禁用区间里画过的元素会永久留着禁用态 ——
            // 表现就是整个面板点不动(点击不发事件), 且再也不会自己好
            if (m_StateStack.CurrentEnabled) element.SetEnabled(m_StateStack.CurrentEnabled);
            else element.SetEnabled(true);
        }

        internal void ApplyLabelAndWidth<T>(BaseField<T> field, string label, float width)
        {
            // 空串触发 Unity 的 no-label 变体(标签元素从布局里拿掉, 控件占满整行)—— 与 
            // GUIContent.none 同义; 直接把 label 透传过去, 不要手动 display/塞空格干扰它。
            field.label = label ?? "";
            var hint = LayoutPolicy.ForField(width);

            // 标签列走类型化的 labelElement 属性(必然生效), 不经 Q 按类名查找 —— 查不到就整列对不齐。
            // 这里只锁"下限": 短标签统一成列宽(各行控件列对齐), 长标签按内容把列撑开 —— 后面的控件框
            // 相应变短, 文字完整单行显示(与  一致:  也是长标签占宽、控件变短, 不截断也不折行)
            // HideLabel 走的是 no-label 变体, labelElement 已经被拿掉, 下面的样式写到不存在的元素上没影响。
            var labelElement = field.labelElement;
            labelElement.style.minWidth    = m_StateStack.LabelWidth ?? AutoLabelWidth;
            labelElement.style.width       = StyleKeyword.Null;   // 清掉可能残留的锁宽
            labelElement.style.flexShrink  = 0f;
            labelElement.style.flexGrow    = 0f;
            labelElement.style.marginLeft  = 0f;
            labelElement.style.marginTop   = 0f;   // 行高由控件行自己定, 标签元素不带额外 margin
            labelElement.style.marginBottom = 0f;
            labelElement.style.whiteSpace  = WhiteSpace.NoWrap;
            labelElement.style.overflow    = Overflow.Visible;

            // 控件默认占满标签右侧的可用宽(UIElements 的 BaseField 默认是内容宽,
            // 不撑开的话下拉框只剩文字那么大); 显式给了宽度才定宽
            if (hint.Width > 0f)
            {
                field.style.width    = hint.Width;
                field.style.flexGrow = 0f;
            }
            else
            {
                // 复用的字段可能带着上一轮别的控件留下的锁宽: 不清掉就被钉死在旧宽上, 弹窗文字被裁
                field.style.width = StyleKeyword.Null;
                // 占满宽: 横排里 flexGrow, 竖排里换 alignSelf —— 否则字段会被撑高(见 Layout)
                Layout.FillWidth(field);
            }

            // 同 Label: 主题给 BaseField 自带的 margin 归零, 行距唯一来源是 RowSpacing
            field.style.marginTop    = 0f;
            field.style.marginBottom = RowSpacing;
            field.style.marginLeft   = 0f;
            field.style.marginRight  = 0f;

            // 塌行保护: HideLabel 时 Unity 把标签元素从布局里拿掉(no-label 变体), 字段行高就只剩
            // 输入区自己撑 —— 某些主题下输入区不带固有高, 整行塌成薄片、弹窗文字被裁。这里给字段
            // 本身一个与单行控件一致的硬下限, 无论塌陷在哪一层都能兜住
            field.style.minHeight = FieldMinHeight;
            // 行高也钉死: 只给下限时, 主题的输入区高度会把整行撑高/压低, 与 
            // singleLineHeight 对不上 —— 两端的行距差就是这么攒出来的
            field.style.height    = FieldMinHeight;

            // flex 项的 min-width 默认是 auto(= 内容宽): 不归零, 值一长字段就永不收缩,
            // 把整行撑出单元格、文字压到后面的 × 按钮下面, 再顶出横向滚动条。
            // 没指定宽度时对应 MinWidth(0)(允许收缩, 多出来的字裁掉);
            // 指定了宽度时  Width 是硬约束, 这里同样锁死 min 且不许收缩。
            field.style.minWidth   = hint.MinWidth;
            field.style.flexShrink = hint.Width > 0f ? 0f : 1f;
        }
    }
}
