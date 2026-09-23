using System;
using RD.Inspector;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    [Serializable]
    [LabelText("UI大小")]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UIBind/Data/UI大小 【RectSize】 UIDataBindRectSize")]
    public class UIDataBindRectSize : UIDataBindSelectBase
    {
        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.Vector2;
        }

        protected override int SelectMax()
        {
            return 1;
        }

        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("UI变换组件")]
        private RectTransform m_RectTransform;

        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_RectTransform ??= GetComponent<RectTransform>();
        }

        protected override void OnValueChanged()
        {
            if (m_RectTransform == null) return;

            var dataValue = GetFirstValue<Vector2>();
            m_RectTransform.sizeDelta = dataValue;
        }
    }
}
