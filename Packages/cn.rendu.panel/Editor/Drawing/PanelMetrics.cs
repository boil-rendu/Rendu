#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace RD.Panel.Editor
{
    /// <summary>
    /// 两个绘制后端(IMGUI / UI Toolkit)共用的布局度量, **一律以 IMGUI 为基准**。
    ///
    /// 取值方式分三种, 优先级从高到低:
    /// 1. 能现场量 IMGUI 的, 就量(行距 / 单行高 / 按钮高 / box 内边距 / mini 字号 / 标签列宽);
    /// 2. 量不到的, 取 IMGUI 侧原来写死的常量(缩进);
    /// 3. IMGUI 没有这个概念、纯属 UI Toolkit 内部约定的, 才留常量(输入区留白)。
    ///
    /// UI Toolkit 不反过来影响这里任何一个值 —— 它只负责跟随。
    /// 控件主题本身的差异(EditorStyles 与 USS 的行高/内边距)不在这个层面, 要完全一致只能只留一个后端。
    /// </summary>
    internal static class PanelMetrics
    {
        /// <summary>一级缩进的宽度 —— IMGUI 侧原来的常量。</summary>
        public const float IndentWidth = 10f;

        /// <summary>标签列宽的兜底值(EditorGUIUtility.labelWidth 取不到时用)。</summary>
        public const float LabelWidthDefault = 150f;

        /// <summary>标签列宽的上限(避免被别处改过的 labelWidth 把面板拉歪)。</summary>
        public const float LabelWidthMax = 300f;

        /// <summary>
        /// 输入区的文字左留白: 0 —— IMGUI 的 EditorGUILayout 字段紧贴标签列右边缘, 没有这层留白。
        /// (这个概念只有 UI Toolkit 那边存在, 按 IMGUI 的观感取 0。)
        /// </summary>
        public const float InputPaddingLeft = 0f;

        //-------------------------------------- 现场量 IMGUI --------------------------------------

        /// <summary>行与行之间的空隙 —— IMGUI 的 standardVerticalSpacing。</summary>
        public static float RowSpacing => Cached(ref s_RowSpacing, () => EditorGUIUtility.standardVerticalSpacing, 2f);

        /// <summary>单行字段的高度 —— IMGUI 的 singleLineHeight。</summary>
        public static float FieldMinHeight => Cached(ref s_FieldMinHeight, () => EditorGUIUtility.singleLineHeight, 18f);

        /// <summary>按钮高度 —— IMGUI 的按钮实际高度(UITK 按它锁高, 不再反过来给 IMGUI 塞值)。</summary>
        public static float ButtonHeight => Cached(ref s_ButtonHeight, MeasureButtonHeight, 20f);

        /// <summary>box 分组的内边距 —— IMGUI 的 box 样式内边距。</summary>
        public static float BoxSpacing => Cached(ref s_BoxSpacing, () => GUI.skin.box.padding.left, 5f);

        /// <summary>box 分组与外界的空隙 —— IMGUI 的 box 样式外边距。</summary>
        public static float BoxMargin => Cached(ref s_BoxMargin, () => GUI.skin.box.margin.top, 2f);

        /// <summary>mini 文本的字号 —— IMGUI 的 miniLabel 字号。</summary>
        public static float MiniFontSize => Cached(ref s_MiniFontSize, () => EditorStyles.miniLabel.fontSize, 10f);

        /// <summary>标签列宽 —— 直接用 IMGUI 的那个 EditorGUIUtility.labelWidth。</summary>
        public static float LabelWidth => Cached(ref s_LabelWidth, () => EditorGUIUtility.labelWidth, LabelWidthDefault);

        private static float s_RowSpacing;
        private static float s_FieldMinHeight;
        private static float s_ButtonHeight;
        private static float s_BoxSpacing;
        private static float s_BoxMargin;
        private static float s_MiniFontSize;
        private static float s_LabelWidth;

        /// <summary>
        /// IMGUI 里一个按钮实际占多高: 皮肤给了定高就用定高, 否则是单行高 + 上下内边距。
        /// GUILayout.Button 不传 GUILayoutOption 时就是这个高度。
        /// </summary>
        private static float MeasureButtonHeight()
        {
            var style = GUI.skin.button;
            if (style.fixedHeight > 0f) return style.fixedHeight;

            return EditorGUIUtility.singleLineHeight + style.padding.vertical;
        }

        /// <summary>
        /// 读一次 IMGUI 的运行时度量并缓存。样式体系就绪前(域重载后、首次 OnGUI 之前)读不到,
        /// 这时返回兜底值且**不缓存**, 等下一次再读 —— 免得第一个失败的读数被永久钉住。
        /// </summary>
        private static float Cached(ref float slot, Func<float> read, float fallback)
        {
            if (slot > 0f) return slot;

            float value;
            try
            {
                value = read();
            }
            catch
            {
                return fallback;
            }

            if (value <= 0f) return fallback;

            slot = value;
            return value;
        }
    }
}

#endif
