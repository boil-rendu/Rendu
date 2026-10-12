using System;
using RD.Panel;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI 位置数据绑定（Vector2 → RectTransform 位置）
    /// 可选局部坐标或世界坐标；参数个数上限 1
    /// </summary>
    [Serializable]
    [LabelText("UI位置")]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UIBind/Data/UI位置 【RectPos2】 UIDataBindRectPos2 s")]
    public class UIDataBindRectPos2 : UIDataBindSelectBase
    {
        /// <summary>过滤出 Vector2 类型的数据</summary>
        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.Vector2;
        }

        /// <summary>最多绑定 1 个数据</summary>
        protected override int SelectMax()
        {
            return 1;
        }

        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("UI变换组件")]
        private RectTransform m_RectTransform;

        /// <summary>true 用 localPosition，false 用世界 position</summary>
        [SerializeField]
        [LabelText("使用局部坐标否则世界坐标")]
        private bool m_LocalPosition = true;

        /// <summary>刷新时补齐 RectTransform 引用</summary>
        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_RectTransform ??= GetComponent<RectTransform>();
        }

        /// <summary>数据变化：把 Vector2 应用到位置</summary>
        protected override void OnValueChanged()
        {
            if (m_RectTransform == null) return;

            var dataValue = GetFirstValue<Vector2>();

            if (m_LocalPosition)
            {
                m_RectTransform.localPosition = dataValue;
            }
            else
            {
                m_RectTransform.position = dataValue;
            }
        }
    }
}
