using RD.Draw;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 按子物体大小自适应：汇总直接子节点的尺寸后设置自身的 rect 大小
    /// 同时实现 ILayoutElement（对外上报尺寸）、ILayoutSelfController / ILayoutGroup（被动接收重建）
    /// 纵向计算 preferredHeight、横向计算 preferredWidth，二者逻辑对称
    /// </summary>
    [AddComponentMenu("UIFramework/Widget/自适应大小To子物体 【ContentSizeFilterByChildren】")]
    public class ContentSizeFilterByChildren : UIBehaviour, ILayoutElement, ILayoutSelfController, ILayoutGroup
    {
        /// <summary>自适应方向</summary>
        [LabelText("自适应模式")]
        public enum FitMode
        {
            /// <summary>只自适应宽度</summary>
            [LabelText("Width_宽")]
            Width,

            /// <summary>只自适应高度</summary>
            [LabelText("Height_高")]
            Height,

            /// <summary>宽高都自适应</summary>
            [LabelText("Both_宽高")]
            Both
        }

        /// <summary>子物体尺寸的汇总算法</summary>
        [LabelText("大小模式")]
        public enum SizeMode
        {
            /// <summary>各子物体首选尺寸相加（含间隔、内边距）</summary>
            [LabelText("Add_所有最高优先级大小相加")]
            Add, //查询 GameObject 上实现 ILayoutElement 的所有组件。使用具有最高优先级且具有此设置值的那个。如果多个组件具有此设置并且具有相同的优先级，则使用其中的最大值

            /// <summary>取子物体中最大的首选尺寸</summary>
            [LabelText("Max_最大子物体的大小")]
            Max,

            /// <summary>各子物体当前 rect 尺寸相加</summary>
            [LabelText("RectAdd_所有Rect值大小相加")]
            RectAdd, //直接取当前rect显示的值

            /// <summary>取子物体中最大的 rect 尺寸</summary>
            [LabelText("RectMax_最大子物体Rect的大小")]
            RectMax, //直接取当前rect显示的值
        }

        [SerializeField]
        protected SizeMode m_Size = SizeMode.Add;

        [SerializeField]
        protected FitMode m_Fit = FitMode.Height;

        /// <summary>自适应方向（默认只自适应高度）</summary>
        public FitMode fit
        {
            get
            {
                return m_Fit;
            }
            set
            {
                m_Fit = value;
            }
        }

        [SerializeField]
        [LabelText("最大限制")]
        protected float m_MaxSize = int.MaxValue;

        /// <summary>结果尺寸的上限</summary>
        public float maxSize
        {
            get
            {
                return m_MaxSize;
            }
            set
            {
                m_MaxSize = value;
            }
        }

        [SerializeField]
        [LabelText("布局优先级")]
        protected int m_layoutPriority = 1;

        /// <summary>布局优先级（越大越晚参与布局）</summary>
        public int layoutPriority
        {
            get
            {
                return m_layoutPriority;
            }
            set
            {
                m_layoutPriority = value;
            }
        }

        [SerializeField]
        [LabelText("最小宽度")]
        protected float m_minWidth = -1;

        /// <summary>最小宽度（大于 0 时生效）</summary>
        public float minWidth
        {
            get
            {
                return m_minWidth;
            }
            set
            {
                m_minWidth = value;
            }
        }

        [SerializeField]
        [LabelText("最小高度")]
        protected float m_minHeight = -1;

        /// <summary>最小高度（大于 0 时生效）</summary>
        public float minHeight
        {
            get
            {
                return m_minHeight;
            }
            set
            {
                m_minHeight = value;
            }
        }

        [SerializeField]
        [LabelText("间隔")]
        protected float m_Space = 0;

        [SerializeField]
        [LabelText("偏移")]
        protected float m_Offset = 0;

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

        private HorizontalOrVerticalLayoutGroup m_LayoutGroup = null;

        /// <summary>按当前 preferredWidth / preferredHeight 把自身尺寸写到 rect 上（只处理需自适应的轴）</summary>
        public void AutoSize()
        {
            if (m_Fit is FitMode.Width or FitMode.Both)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ((ILayoutElement)this).preferredWidth);
            }

            if (m_Fit is FitMode.Height or FitMode.Both)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ((ILayoutElement)this).preferredHeight);
            }
        }

        void ILayoutController.SetLayoutHorizontal()
        {
            AutoSize();
        }

        void ILayoutController.SetLayoutVertical()
        {
            AutoSize();
        }

        float ILayoutElement.minWidth
        {
            get
            {
                return m_minWidth;
            }
        }

        /// <summary>
        /// 横向首选宽度的计算：
        /// 先取自身 LayoutGroup 的左右 padding（水平排布时再补 spacing），
        /// 再按 SizeMode 汇总所有「激活且未 ignoreLayout」的直接子节点，
        /// 最后依次叠加 offset、夹到 maxSize 上限、与 minWidth 取大；不适配宽度时返回 -1
        /// </summary>
        float ILayoutElement.preferredWidth
        {
            get
            {
                float size = -1;
                float max = -2;
                if (m_Fit is FitMode.Width or FitMode.Both)
                {
                    max = 0;
                    size = 0;
                    float spacing = m_Space;
                    if (m_LayoutGroup != null)
                    {
                        size = size + m_LayoutGroup.padding.left + m_LayoutGroup.padding.right;
                        if (m_LayoutGroup is HorizontalLayoutGroup)
                            spacing = m_LayoutGroup.spacing;
                    }

                    int One = 1;
                    for (int i = 0; i < rectTransform.childCount; ++i)
                    {
                        var child = rectTransform.GetChild(i) as RectTransform;
                        if (child.gameObject.activeSelf)
                        {
                            var ignore = child.GetComponent<ILayoutIgnorer>();
                            if (ignore != null && ignore.ignoreLayout) continue;

                            switch (m_Size)
                            {
                                case SizeMode.Add:
                                    size += Mathf.Max(0, LayoutUtility.GetPreferredWidth(child));

                                    if (One == 0)
                                    {
                                        size += spacing;
                                    }

                                    if (One == 1)
                                    {
                                        One = 0;
                                    }

                                    break;
                                case SizeMode.Max:
                                    max = Mathf.Max(max, LayoutUtility.GetPreferredWidth(child) + size);

                                    break;
                                case SizeMode.RectAdd:
                                    size += child.rect.width;
                                    if (One == 0)
                                    {
                                        size += spacing;
                                    }

                                    if (One == 1)
                                    {
                                        One = 0;
                                    }

                                    break;
                                case SizeMode.RectMax:
                                    max = Mathf.Max(max, child.rect.width + size);

                                    break;
                                default:
                                    Debug.LogError($"未实现的功能 {m_Size}");
                                    break;
                            }
                        }
                    }

                    if (m_Size is SizeMode.Max or SizeMode.RectMax)
                    {
                        size = max;
                    }

                    size += m_Offset;

                    size = Mathf.Min(maxSize, size);
                    if (minWidth > 0)
                    {
                        size = Mathf.Max(minWidth, size);
                    }
                }

                return size;
            }
        }

        float ILayoutElement.flexibleWidth
        {
            get
            {
                return -1;
            }
        }

        float ILayoutElement.minHeight
        {
            get
            {
                return m_minHeight;
            }
        }

        /// <summary>
        /// 纵向首选高度的计算，与 preferredWidth 完全对称
        /// 取自身 LayoutGroup 的上下 padding（垂直排布时再补 spacing），按 SizeMode 汇总子节点
        /// </summary>
        float ILayoutElement.preferredHeight
        {
            get
            {
                float size = -1;
                float max = 0;
                if (m_Fit is FitMode.Height or FitMode.Both)
                {
                    size = 0;
                    float spacing = m_Space;
                    if (m_LayoutGroup != null)
                    {
                        size = size + m_LayoutGroup.padding.top + m_LayoutGroup.padding.bottom;
                        if (m_LayoutGroup is VerticalLayoutGroup)
                            spacing = m_LayoutGroup.spacing;
                    }

                    int One = 1;

                    for (int i = 0; i < rectTransform.childCount; ++i)
                    {
                        var child = rectTransform.GetChild(i) as RectTransform;
                        if (child.gameObject.activeSelf)
                        {
                            var ignore = child.GetComponent<ILayoutIgnorer>();
                            if (ignore != null && ignore.ignoreLayout) continue;

                            switch (m_Size)
                            {
                                case SizeMode.Add:
                                    size += Mathf.Max(0, LayoutUtility.GetPreferredHeight(child));

                                    if (One == 0)
                                    {
                                        size += spacing;
                                    }

                                    if (One == 1)
                                    {
                                        One = 0;
                                    }

                                    break;
                                case SizeMode.Max:
                                    max = Mathf.Max(max, LayoutUtility.GetPreferredHeight(child) + size);

                                    break;
                                case SizeMode.RectAdd:
                                    size += child.rect.height;
                                    if (One == 0)
                                    {
                                        size += spacing;
                                    }

                                    if (One == 1)
                                    {
                                        One = 0;
                                    }

                                    break;
                                case SizeMode.RectMax:
                                    max = Mathf.Max(max, child.rect.height + size);

                                    break;
                                default:
                                    Debug.LogError($"未实现的功能 {m_Size}");
                                    break;
                            }
                        }
                    }

                    if (m_Size is SizeMode.Max or SizeMode.RectMax)
                    {
                        size = max;
                    }

                    size += m_Offset;

                    size = Mathf.Min(maxSize, size);
                    if (minHeight > 0)
                    {
                        size = Mathf.Max(minHeight, size);
                    }
                }

                return size;
            }
        }

        float ILayoutElement.flexibleHeight
        {
            get
            {
                return -1;
            }
        }

        int ILayoutElement.layoutPriority
        {
            get
            {
                return m_layoutPriority;
            }
        }

        void ILayoutElement.CalculateLayoutInputHorizontal()
        {
            AutoSize();
        }

        void ILayoutElement.CalculateLayoutInputVertical()
        {
            AutoSize();
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

        /// <summary>缓存自身的水平/垂直布局组件（用于取 padding 与 spacing）</summary>
        protected override void Awake()
        {
            m_LayoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
        }

        #if UNITY_EDITOR
        /// <summary>编辑器改参数时刷新缓存并重建，便于即时预览</summary>
        protected override void OnValidate()
        {
            OnEnable();
            m_LayoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
            ForceRebuild();
        }
        #endif
    }
}