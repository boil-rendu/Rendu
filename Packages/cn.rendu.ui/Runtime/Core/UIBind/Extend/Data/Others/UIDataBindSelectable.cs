using RD.EditorKit;
using UnityEngine;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 是否可选择数据绑定（Bool → Selectable.interactable）
    /// 适用于 Toggle / InputField / Button / Dropdown 等所有 Selectable 派生组件
    /// </summary>
    [DetailedInfoBox("可选择的Toggle/Button ...类的",
                     @"改变是否可触摸 Toggle / inputField / button / dropdown 这些都可以用 
在某些情况下不允许点击时  灰色点击那种 就可以用这个 因为他们都继承Selectable")]
    [RequireComponent(typeof(Selectable))]
    [LabelText("是否可选择")]
    [AddComponentMenu("UIBind/Data/交互 【Selectable】 UIDataBindSelectable")]
    public sealed class UIDataBindSelectable : UIDataBindBool
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("可选组件")]
        private Selectable m_Selectable;

        /// <summary>刷新时补齐 Selectable 引用</summary>
        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_Selectable ??= GetComponent<Selectable>();
        }

        /// <summary>数据变化：设置是否可交互</summary>
        protected override void OnValueChanged()
        {
            if (m_Selectable == null) return;

            m_Selectable.interactable = GetResult();
        }
    }
}
