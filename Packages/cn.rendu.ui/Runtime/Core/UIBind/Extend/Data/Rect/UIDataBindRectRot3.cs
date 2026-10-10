using System;
using RD.EditorKit;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI 旋转数据绑定（Vector3 → RectTransform 旋转）
    /// 直接把 Vector3 当作欧拉角应用
    /// </summary>
    [Serializable]
    [LabelText("UI旋转")]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UIBind/Data/UI旋转 【RectRot3】 UIDataBindRectRot3")]
    public class UIDataBindRectRot3 : UIDataBindSelectBase
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

        /// <summary>数据变化：把 Vector3 作为欧拉角应用</summary>
        protected override void OnValueChanged()
        {
            if (m_RectTransform == null) return;

            var dataValue = GetFirstValue<Vector3>();

            m_RectTransform.rotation = Quaternion.Euler(dataValue);
        }
    }
}
