using System;
using System.Collections.Generic;
using RD.EditorKit;
using UnityEngine;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 滚动条事件绑定（UGUI Scrollbar）
    /// 监听 onValueChanged，把「当前滚动值 float(0-1)」作为事件参数派发
    /// </summary>
    [InfoBox("提示: 可用事件参数 <参数1:float(当前滚动值 0-1)>")]
    [LabelText("滚动条<Float>")]
    [RequireComponent(typeof(Scrollbar))]
    [AddComponentMenu("UIBind/Event/滚动条 【Scrollbar】 UIEventBindScrollbar")]
    public class UIEventBindScrollbar : UIEventBind
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("滚动条")]
        private Scrollbar m_Scrollbar;

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
            m_Scrollbar ??= GetComponent<Scrollbar>();
        }

        /// <summary>启用时监听滚动值变化</summary>
        private void OnEnable()
        {
            if (m_Scrollbar == null) return;
            m_Scrollbar.onValueChanged.AddListener(OnValueChanged);
        }

        /// <summary>禁用时移除监听</summary>
        private void OnDisable()
        {
            if (m_Scrollbar == null) return;
            m_Scrollbar.onValueChanged.RemoveListener(OnValueChanged);
        }

        /// <summary>值变化回调：派发当前滚动值</summary>
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
