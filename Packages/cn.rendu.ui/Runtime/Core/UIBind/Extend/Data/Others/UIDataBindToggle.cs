using RD.Inspector;
using UnityEngine;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 开关数据绑定（Bool → UGUI Toggle.isOn）
    /// 只在值不同时才赋值，避免反复触发 onValueChanged
    /// </summary>
    [DetailedInfoBox("可选择的Toggle/Button ...类的",
                     @"改变是否可触摸 Toggle / inputField / button / dropdown 这些都可以用 
在某些情况下不允许点击时  灰色点击那种 就可以用这个")]
    [RequireComponent(typeof(Toggle))]
    [LabelText("是否可选择")]
    [AddComponentMenu("UIBind/Data/开关 【Toggle】 UIDataBindSelectable")]
    public sealed class UIDataBindToggle : UIDataBindBool
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("开关")]
        private Toggle m_Toggle;

        /// <summary>刷新时补齐 Toggle 引用</summary>
        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_Toggle ??= GetComponent<Toggle>();
        }

        /// <summary>数据变化：设置开关状态</summary>
        protected override void OnValueChanged()
        {
            if (m_Toggle == null) return;
            var result = GetResult();
            if (m_Toggle.isOn != result)
            {
                m_Toggle.isOn = result;
            }
        }
    }
}
