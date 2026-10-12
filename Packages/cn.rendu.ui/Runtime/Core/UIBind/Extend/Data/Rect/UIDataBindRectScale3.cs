using System;
using RD.Panel;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI 缩放数据绑定（Vector3 → RectTransform 缩放）
    /// 直接把 Vector3 应用到 localScale
    /// </summary>
    [Serializable]
    [LabelText("UI缩放")]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UIBind/Data/UI缩放 【RectScale3】 UIDataBindRectScale3")]
    public class UIDataBindRectScale3 : UIDataBindSelectBase
    {
        /// <summary>过滤出 Vector3 类型的数据</summary>
        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.Vector3;
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

        /// <summary>刷新时补齐 RectTransform 引用</summary>
        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_RectTransform ??= GetComponent<RectTransform>();
        }

        /// <summary>数据变化：把 Vector3 应用到 localScale</summary>
        protected override void OnValueChanged()
        {
            if (m_RectTransform == null) return;

            var dataValue = GetFirstValue<Vector3>();

            m_RectTransform.localScale = dataValue;
        }
    }
}
