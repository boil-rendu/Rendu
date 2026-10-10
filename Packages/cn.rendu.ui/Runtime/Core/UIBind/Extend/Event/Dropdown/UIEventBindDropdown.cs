using System;
using System.Collections.Generic;
using RD.EditorKit;
using UnityEngine;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 下拉菜单事件绑定（UGUI Dropdown）
    /// 监听 onValueChanged，把「被选中的索引 int」作为事件参数派发
    /// </summary>
    [InfoBox("提示: 可用事件参数 <参数1:int(下拉菜单被选择的索引值)>")]
    [LabelText("下拉菜单<int>")]
    [RequireComponent(typeof(Dropdown))]
    [AddComponentMenu("UIBind/Event/下拉菜单 【Dropdown】 UIEventBindDropdown")]
    public class UIEventBindDropdown : UIEventBind
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("下拉菜单")]
        private Dropdown m_Dropdown;

        /// <summary>非异步事件</summary>
        protected override bool IsTaskEvent => false;

        [NonSerialized]
        private readonly List<EUIEventParamType> m_FilterParamType = new()
        {
            EUIEventParamType.Int
        };

        /// <summary>过滤出参数为 int 的事件</summary>
        protected override List<EUIEventParamType> GetFilterParamType => m_FilterParamType;

        private void Awake()
        {
            m_Dropdown ??= GetComponent<Dropdown>();
        }

        /// <summary>启用时监听下拉菜单值变化</summary>
        private void OnEnable()
        {
            if (m_Dropdown == null) return;
            m_Dropdown.onValueChanged.AddListener(OnValueChanged);
        }

        /// <summary>禁用时移除监听</summary>
        private void OnDisable()
        {
            if (m_Dropdown == null) return;
            m_Dropdown.onValueChanged.RemoveListener(OnValueChanged);
        }

        /// <summary>值变化回调：派发被选中的索引</summary>
        private void OnValueChanged(int value)
        {
            try
            {
                m_UIEvent?.Invoke(value);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }
    }
}
