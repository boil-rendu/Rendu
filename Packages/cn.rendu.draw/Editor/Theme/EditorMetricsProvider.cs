#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace RD.Draw.Editor
{
    /// <summary>
    /// 度量源: 尺寸直接取编辑器默认控件(按钮 / 输入框 / 分组盒 / 滚动区 / 工具栏 / mini 文本)的既定规格,
    /// 渲染后端本身就是保留模式(UIElements), 这些值与真正画出来的控件一致。不再借立即模式那套皮肤:
    /// 那是另一套控件的常量, 与保留模式的控件无关。
    ///
    /// 兜底的常量就是编辑器皮肤的默认值, 与量到的真值一致; 没有活着的可解析 panel 时钉这些值。
    /// </summary>
    internal sealed class EditorMetricsProvider : IMetricsProvider
    {
        /// <summary>进程级单例: 与静态缓存同一套失效约定(域重载后重建)。</summary>
        public static readonly EditorMetricsProvider Instance = new();

        /// <summary>标签列宽的兜底值(EditorGUIUtility.labelWidth 取不到时用)。</summary>
        private const float LabelWidthDefault = 150f;

        //-------------------------------------- 保留模式控件规格 --------------------------------------

        public float RowSpacing        => 2f;
        public float FieldMinHeight    => 18f;
        public float ButtonHeight      => 20f;
        public float BoxSpacing        => 5f;
        public float BoxMargin         => 2f;
        public float ButtonMargin      => 0f;
        public float MiniFontSize      => 10f;
        public float ScrollbarWidth    => 15f;
        public float ScrollbarHeight   => 15f;
        public float ScrollViewInsets  => 15f;
        public float BoxInsets         => 8f;
        public float ToolbarHeight     => 21f;

        //-------------------------------------- 控件底纹灰度 --------------------------------------

        /// <summary>
        /// 编辑器控件底纹是乘在纹理上的: 最终像素 = 底色 × 纹理灰度。UIElements 铺的是纯色,
        /// 必须乘同一个灰度才与编辑器控件同色; 这里取编辑器默认皮肤下控件底纹的既定灰度。
        /// </summary>
        public float ButtonTintGray    => 0.35f;
        public float PopupTintGray     => 0.50f;
        public float TextFieldTintGray => 0.32f;

        /// <summary>
        /// 标签列宽 —— 直接用 EditorGUIUtility.labelWidth(编辑器标签列宽偏好, 检视器同样遵循)。
        /// 这一项**不缓存**: 用户随时可能拖标签列宽, 缓存等于"改了设置要等重编译"。
        /// </summary>
        public float LabelWidth
        {
            get
            {
                try
                {
                    var value = EditorGUIUtility.labelWidth;
                    return value > 0f ? value : LabelWidthDefault;
                }
                catch
                {
                    return LabelWidthDefault;
                }
            }
        }
    }
}

#endif
