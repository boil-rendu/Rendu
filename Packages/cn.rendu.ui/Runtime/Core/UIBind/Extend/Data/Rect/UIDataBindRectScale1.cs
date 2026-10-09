using System;
using RD.Inspector;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI 缩放数据绑定（Float → RectTransform 缩放）
    /// 目标轴上有值的分量会被乘以数据值，默认 X 轴
    /// </summary>
    [Serializable]
    [LabelText("UI缩放")]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UIBind/Data/UI缩放 【RectScale1】 UIDataBindRectScale1")]
    public class UIDataBindRectScale1 : UIDataBindSelectBase
    {
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

        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("UI变换组件")]
        private RectTransform m_RectTransform;

        /// <summary>缩放轴权重（哪个轴有值就缩放哪个轴，默认 X 轴 1 倍）</summary>
        [SerializeField]
        [InfoBox("那个轴有值就是控制那个轴 单位1, 多的就是倍数 默认使用 Z轴旋转1")]
        [LabelText("目标轴")]
        private Vector3 m_Target = Vector3.right;

        /// <summary>刷新时补齐 RectTransform 引用</summary>
        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_RectTransform ??= GetComponent<RectTransform>();
        }

        /// <summary>数据变化：按目标轴权重合成缩放并应用</summary>
        protected override void OnValueChanged()
        {
            if (m_RectTransform == null) return;

            var dataValue = GetFirstValue<float>();

            m_RectTransform.localScale = m_Target * dataValue;
        }
    }
}
