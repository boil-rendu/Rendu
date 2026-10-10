using System;
using RD.EditorKit;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI 大小数据绑定（Vector2 → RectTransform.sizeDelta）
    /// 同时设置宽高
    /// </summary>
    [Serializable]
    [LabelText("UI大小")]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UIBind/Data/UI大小 【RectSize】 UIDataBindRectSize")]
    public class UIDataBindRectSize : UIDataBindSelectBase
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

        /// <summary>刷新时补齐 RectTransform 引用</summary>
        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_RectTransform ??= GetComponent<RectTransform>();
        }

        /// <summary>数据变化：把 Vector2 应用到 sizeDelta</summary>
        protected override void OnValueChanged()
        {
            if (m_RectTransform == null) return;

            var dataValue = GetFirstValue<Vector2>();
            m_RectTransform.sizeDelta = dataValue;
        }
    }
}
