using System;
using System.Collections.Generic;
using RD.Panel;
using UnityEngine;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 滑动条事件绑定（UGUI Slider）
    /// 监听 onValueChanged，把「当前滑动值 float」作为事件参数派发
    /// </summary>
    [InfoBox("提示: 可用事件参数 <参数1:float(当前滑动值)>")]
    [LabelText("滑动条<Float>")]
    [RequireComponent(typeof(Slider))]
    [AddComponentMenu("UIBind/Event/滑动条 【Slider】 UIEventBindSlider")]
    public class UIEventBindSlider : UIEventBind
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("滑动条")]
        private Slider m_Slider;

        /// <summary>非异步事件</summary>
        protected override bool IsTaskEvent => false;

        [NonSerialized]
        private readonly List<EUIEventParamType> m_FilterParamType = new()
        {
            EUIEventParamType.Float
        };

        /// <summary>过滤出参数为 float 的事件</summary>
        protected override List<EUIEventParamType> GetFilterParamType => m_FilterParamType;

        private void Awake()
        {
            m_Slider ??= GetComponent<Slider>();
        }

        /// <summary>启用时监听滑动值变化</summary>
        private void OnEnable()
        {
            if (m_Slider == null) return;
            m_Slider.onValueChanged.AddListener(OnValueChanged);
        }

        /// <summary>禁用时移除监听</summary>
        private void OnDisable()
        {
            if (m_Slider == null) return;
            m_Slider.onValueChanged.RemoveListener(OnValueChanged);
        }

        /// <summary>值变化回调：派发当前滑动值</summary>
        private void OnValueChanged(float value)
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
