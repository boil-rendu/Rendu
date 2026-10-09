using System;
using RD.Inspector;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    [Serializable]
    [LabelText("UI大小-高")]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UIBind/Data/UI大小-高 【RectSizeHeight】 UIDataBindRectSizeHeight")]
    /// <summary>
    /// UI 大小-高 数据绑定（Float → RectTransform.sizeDelta.y）
    /// 只改高度，宽度保持当前值
    /// </summary>
    public class UIDataBindRectSizeHeight : UIDataBindSelectBase
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
        /// <summary>目标 UI 变换组件</summary>
        private RectTransform m_RectTransform;

        /// <summary>复用的 size 缓存</summary>
        private Vector2 m_SizeData;

        /// <summary>刷新时补齐 RectTransform 引用</summary>
        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_RectTransform ??= GetComponent<RectTransform>();
        }

        /// <summary>数据变化：只更新高度，宽度取当前值</summary>
        protected override void OnValueChanged()
        {
            if (m_RectTransform == null) return;

            m_SizeData.y              = GetFirstValue<float>();
            m_SizeData.x              = m_RectTransform.sizeDelta.x;
            m_RectTransform.sizeDelta = m_SizeData;
        }
    }
}
