using System;
using System.Collections.Generic;
using RD.EditorKit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 双击事件绑定（实现 IPointerClickHandler，自行按时间间隔判定双击）
    /// 基类只派发无参事件；带 PointerEventData 的版本见 UIEventBindDoubleClickPointerEventData
    /// </summary>
    [LabelText("双击<null>")]
    [AddComponentMenu("UIBind/Event/双击 【DoubleClick】 UIEventBindDoubleClick")]
    public class UIEventBindDoubleClick : UIEventBind, IPointerClickHandler
    {
        [SerializeField]
        [LabelText("双击间隔时间")]
        private float m_DoubleClickInterval = 0.3f;

        [SerializeField]
        [LabelText("拖拽时不响应点击")]
        private bool m_SkipWhenDrag;

        [SerializeField]
        [LabelText("可选组件")]
        private Selectable m_Selectable;

        /// <summary>上一次点击的时间戳</summary>
        private float m_LastClickTime = 0f;

        /// <summary>点击回调：主动/拖拽过滤 → 距上次点击在间隔内则判定为双击并派发</summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (m_Selectable != null && !m_Selectable.interactable)
            {
                return;
            }

            if (m_SkipWhenDrag && eventData.dragging)
            {
                return;
            }

            try
            {
                float timeSinceLastClick = Time.time - m_LastClickTime;

                if (timeSinceLastClick <= m_DoubleClickInterval)
                {
                    OnUIEvent(eventData);
                    m_LastClickTime = 0;
                    return;
                }

                m_LastClickTime = Time.time;
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        /// <summary>非异步事件</summary>
        protected override bool IsTaskEvent => false;

        [NonSerialized]
        private readonly List<EUIEventParamType> m_BaseFilterParamType = new();

        /// <summary>过滤出无参事件</summary>
        protected override List<EUIEventParamType> GetFilterParamType => m_BaseFilterParamType;

        private void Awake()
        {
            m_Selectable ??= GetComponent<Selectable>();
        }

        /// <summary>判定为双击后的派发点（子类可覆写以带参派发）</summary>
        protected virtual void OnUIEvent(PointerEventData eventData)
        {
            m_UIEvent?.Invoke();
        }
    }
}
