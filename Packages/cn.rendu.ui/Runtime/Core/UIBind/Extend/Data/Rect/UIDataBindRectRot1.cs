using System;
using RD.Panel;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI 旋转数据绑定（Float → RectTransform 旋转）
    /// 目标轴上有值的分量会被乘以数据值，默认绕 Z 轴
    /// </summary>
    [Serializable]
    [LabelText("UI旋转")]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UIBind/Data/UI旋转 【RectRot1】 UIDataBindRectRot1 l")]
    public class UIDataBindRectRot1 : UIDataBindSelectBase
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

        /// <summary>旋转轴权重（哪个轴有值就绕哪个轴，默认 Z 轴 1 倍）</summary>
        [SerializeField]
        [InfoBox("那个轴有值就是控制那个轴 单位1, 多的就是倍数 默认使用 Z轴旋转1")]
        [LabelText("目标轴")]
        private Vector3 m_TargetRot = Vector3.forward;

        /// <summary>刷新时补齐 RectTransform 引用</summary>
        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_RectTransform ??= GetComponent<RectTransform>();
        }

        /// <summary>数据变化：按目标轴权重合成欧拉角并应用</summary>
        protected override void OnValueChanged()
        {
            if (m_RectTransform == null) return;

            var dataValue = GetFirstValue<float>();

            m_RectTransform.rotation = Quaternion.Euler(m_TargetRot * dataValue);
        }
    }
}
