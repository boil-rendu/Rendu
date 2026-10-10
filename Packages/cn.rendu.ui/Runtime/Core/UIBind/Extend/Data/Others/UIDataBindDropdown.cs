using System;
using RD.EditorKit;
using UnityEngine;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 下拉菜单数据绑定（Int ↔ UGUI Dropdown）
    /// 双向：数据变化时设置 value；用户切换时把索引写回数据
    /// </summary>
    [Serializable]
    [LabelText("下拉菜单")]
    [RequireComponent(typeof(Dropdown))]
    [AddComponentMenu("UIBind/Data/下拉菜单 【Dropdown】 UIDataBindDropdown")]
    public class UIDataBindDropdown : UIDataBindSelectBase
    {
        /// <summary>过滤出 Int 类型的数据</summary>
        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.Int;
        }

        /// <summary>最多绑定 1 个数据</summary>
        protected override int SelectMax()
        {
            return 1;
        }

        [ReadOnly]
        [Required("必须有此组件")]
        [SerializeField]
        [LabelText("下拉菜单")]
        private Dropdown m_Dropdown;

        /// <summary>刷新时补齐 Dropdown 引用并挂上值变化监听</summary>
        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_Dropdown ??= GetComponent<Dropdown>();
            if (m_Dropdown != null)
            {
                m_Dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
                m_Dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            }
        }

        /// <summary>用户切换回调：把索引写回数据</summary>
        private void OnDropdownValueChanged(int index)
        {
            SetFirstValue<int>(index);
        }

        /// <summary>数据变化：把值应用到 Dropdown.value</summary>
        protected override void OnValueChanged()
        {
            if (m_Dropdown == null) return;

            var dataValue = GetFirstValue<int>();

            if (m_Dropdown.value != dataValue)
            {
                m_Dropdown.value = dataValue;
            }
        }
    }
}
