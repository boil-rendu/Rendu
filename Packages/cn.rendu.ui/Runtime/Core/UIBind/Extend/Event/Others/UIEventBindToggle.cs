using System;
using System.Collections.Generic;
using RD.Panel;
using UnityEngine;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 开关事件绑定（UGUI Toggle）
    /// 监听 onValueChanged，把「当前状态 bool」作为事件参数派发
    /// </summary>
    [InfoBox("提示: 可用事件参数 <参数1:bool(当前状态)>")]
    [LabelText("开关<bool>")]
    [RequireComponent(typeof(Toggle))]
    [AddComponentMenu("UIBind/Event/开关 【Toggle】 UIEventBindToggle")]
    public class UIEventBindToggle : UIEventBind
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("开关")]
        private Toggle m_Toggle;

        /// <summary>非异步事件</summary>
        protected override bool IsTaskEvent => false;

        [NonSerialized]
        private readonly List<EUIEventParamType> m_FilterParamType = new()
        {
            EUIEventParamType.Bool
        };

        /// <summary>过滤出参数为 bool 的事件</summary>
        protected override List<EUIEventParamType> GetFilterParamType => m_FilterParamType;

        private void Awake()
        {
            m_Toggle ??= GetComponent<Toggle>();
        }

        /// <summary>启用时监听开关状态变化</summary>
        private void OnEnable()
        {
            if (m_Toggle == null) return;
            m_Toggle.onValueChanged.AddListener(OnValueChanged);
        }

        /// <summary>禁用时移除监听</summary>
        private void OnDisable()
        {
            if (m_Toggle == null) return;
            m_Toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        /// <summary>值变化回调：派发当前开关状态</summary>
        private void OnValueChanged(bool value)
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
