using RD.Panel;
using UnityEngine;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 图片填充数据绑定（Float → Image.fillAmount）
    /// 速度 &gt; 0 时以补间动画过渡，支持只增/只减/双向三种模式
    /// </summary>
    [RequireComponent(typeof(Image))]
    [LabelText("ImageFill 图片Fill")]
    [AddComponentMenu("UIBind/Data/图片Fill 【ImageFill】 UIDataBindImageFill")]
    public sealed class UIDataBindImageFill : UIDataBindSelectBase
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("图片")]
        private Image m_Image;

        [InfoBox(">0时 则设置值时会以动画的形式慢慢变化 需要运行时")]
        [SerializeField]
        [LabelText("动画速度")]
        #if UNITY_EDITOR
        [OnValueChanged("OnTweenSpeedValueChanged")]
        #endif
        private float m_TweenSpeed = 0.0f;

        [SerializeField]
        private ETweenType m_TweenType = ETweenType.DoubleWay;

        private float m_TargetValue;
        private bool  m_PlayingTween = false;

        /// <summary>过滤出 Float 类型的数据</summary>
        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.Float;
        }

        /// <summary>最多绑定 1 个数据</summary>
        protected override int SelectMax()
        {
            return 1;
        }

        /// <summary>刷新时补齐 Image 引用并立即对齐到当前值（不做动画）</summary>
        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_Image ??= GetComponent<Image>();
            if (m_Image == null) return;

            var dataValue = GetFirstValue<float>();
            m_TargetValue      = dataValue;
            m_Image.fillAmount = dataValue;
            m_PlayingTween     = false;
        }

        /// <summary>数据变化：运行且有速度时启动过渡，否则直接赋值</summary>
        protected override void OnValueChanged()
        {
            if (m_Image == null) return;

            var dataValue = GetFirstValue<float>();
            if (m_TweenSpeed > 0.0f && Application.isPlaying)
            {
                m_TargetValue  = dataValue;
                m_PlayingTween = true;
            }
            else
            {
                m_TargetValue      = dataValue;
                m_Image.fillAmount = dataValue;
                m_PlayingTween     = false;
            }
        }

        #if UNITY_EDITOR
        /// <summary>编辑器内速度改为 0 时，立刻把值对齐到目标，避免卡在半途</summary>
        private void OnTweenSpeedValueChanged()
        {
            //如果突然吧动画速度改为0
            //且当前正在动画中 那么会错误 虽然只有editor才会出现
            //这里还是处理
            if (m_TweenSpeed <= 0)
            {
                if (!Mathf.Approximately(m_Image.fillAmount, m_TargetValue))
                {
                    m_Image.fillAmount = m_TargetValue;
                }
            }
        }
        #endif

        /// <summary>每帧推进过渡动画</summary>
        private void Update()
        {
            if (m_PlayingTween &&
                m_TweenSpeed > 0.0f &&
                !Mathf.Approximately(m_Image.fillAmount, m_TargetValue))
            {
                switch (m_TweenType)
                {
                    case ETweenType.IncreaseOnly:
                        UpdateIncreaseOnly();
                        break;
                    case ETweenType.DecreaseOnly:
                        UpdateDecreaseOnly();
                        break;
                    case ETweenType.DoubleWay:
                        UpdateDoubleWay();
                        break;
                }
            }
        }

        /// <summary>只增模式：到顶后回绕到 0</summary>
        private void UpdateIncreaseOnly()
        {
            if (m_TargetValue > m_Image.fillAmount)
            {
                UpdateDoubleWay();
                return;
            }

            var movement = m_TweenSpeed * Time.deltaTime;
            var newValue = m_Image.fillAmount + movement;
            if (newValue >= 1)
            {
                m_Image.fillAmount = 0;
            }
            else
            {
                m_Image.fillAmount = newValue;
            }
        }

        /// <summary>只减模式：到底后回绕到 1</summary>
        private void UpdateDecreaseOnly()
        {
            if (m_TargetValue < m_Image.fillAmount)
            {
                UpdateDoubleWay();
                return;
            }

            var movement = m_TweenSpeed * Time.deltaTime;
            var newValue = m_Image.fillAmount - movement;
            if (newValue <= 0)
            {
                m_Image.fillAmount = 1;
            }
            else
            {
                m_Image.fillAmount = newValue;
            }
        }

        /// <summary>双向模式：按目标方向推进，越过则直接对齐并结束动画</summary>
        private void UpdateDoubleWay()
        {
            var offset   = m_TargetValue - m_Image.fillAmount;
            var movement = m_TweenSpeed * Time.deltaTime;
            if (movement > Mathf.Abs(offset))
            {
                m_Image.fillAmount = m_TargetValue;
                m_PlayingTween     = false;
            }
            else
            {
                if (offset > 0.0f)
                {
                    m_Image.fillAmount += movement;
                }
                else
                {
                    m_Image.fillAmount -= movement;
                }
            }
        }
    }
}
