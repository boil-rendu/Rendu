using RD.Panel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 自适应大小组件：把自身 RectTransform 的实际尺寸作为布局尺寸上报（ILayoutElement）
    /// 与 ContentSizeFitter 相反 —— 它是「以自己的 rect 为准去影响父级布局」
    /// </summary>
    [AddComponentMenu("UIFramework/Widget/自适应大小 【ContentSizeFilterByRect】")]
    public class ContentSizeFilterByRect : UIBehaviour, ILayoutElement
    {
        /// <summary>自适应方向</summary>
        [LabelText("自适应模式")]
        public enum FitMode
        {
            /// <summary>只适配宽度</summary>
            [LabelText("Width_宽")]
            Width,

            /// <summary>只适配高度</summary>
            [LabelText("Height_高")]
            Height,

            /// <summary>宽高都适配</summary>
            [LabelText("Both_宽高")]
            Both
        }

        [SerializeField]
        protected FitMode m_Fit = FitMode.Both;

        /// <summary>自适应方向（默认宽高都适配）</summary>
        public FitMode fit
        {
            get { return m_Fit; }
            set { m_Fit = value; }
        }

        [System.NonSerialized]
        private RectTransform m_Rect;

        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                {
                    m_Rect = GetComponent<RectTransform>();
                }
                return m_Rect;
            }
        }

        float ILayoutElement.minWidth
        {
            get { return ((ILayoutElement)this).preferredWidth; }
        }

        /// <summary>宽度取自身 rect.width（FitMode 不含 Width 时返回 -1 表示不参与）</summary>
        float ILayoutElement.preferredWidth
        {
            get
            {
                if (m_Fit == FitMode.Both || m_Fit == FitMode.Width)
                {
                    return rectTransform.rect.width;
                }
                else
                {
                    return -1;
                }
            }
        }

        float ILayoutElement.flexibleWidth
        {
            get { return -1; }
        }

        float ILayoutElement.minHeight
        {
            get { return ((ILayoutElement)this).preferredHeight; }
        }

        /// <summary>高度取自身 rect.height（FitMode 不含 Height 时返回 -1 表示不参与）</summary>
        float ILayoutElement.preferredHeight
        {
            get
            {
                if (m_Fit == FitMode.Both || m_Fit == FitMode.Height)
                {
                    return rectTransform.rect.height;
                }
                else
                {
                    return -1;
                }
            }
        }

        float ILayoutElement.flexibleHeight
        {
            get { return -1; }
        }

        int ILayoutElement.layoutPriority
        {
            get { return 1; }
        }

        void ILayoutElement.CalculateLayoutInputHorizontal()
        {
        }

        void ILayoutElement.CalculateLayoutInputVertical()
        {
        }

        /// <summary>启用时触发一次重建</summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            ForceRebuild();
        }

        /// <summary>禁用时触发一次重建</summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            ForceRebuild();
        }

        /// <summary>标记自身所在布局需要重建</summary>
        public void ForceRebuild()
        {
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
    }
}