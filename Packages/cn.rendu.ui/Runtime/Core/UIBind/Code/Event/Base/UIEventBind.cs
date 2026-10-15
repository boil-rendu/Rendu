using System;
using System.Collections.Generic;
using RD.Draw;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    [ExecuteInEditMode]
    public abstract partial class UIEventBind : MonoBehaviour
    {
        [SerializeField]
        [ReadOnly]
        [HideReferenceObjectPicker]
        [Required("必须选择")]
        [HideLabel]
        [PropertyOrder(-999)]
        private UIBindEventTable m_EventTable;

        public UIBindEventTable EventTable => m_EventTable;

        [SerializeField]
        [LabelText("事件名称")]
        [PropertyOrder(-99)]
        protected string m_EventName = null;

        public string EventName => this.m_EventName;

        /// <summary>
        /// 当前的UI事件
        /// 运行时绑定后由事件表获取 无需序列化 (跨组件引用无法持久化)
        /// </summary>
        [NonSerialized]
        protected UIEventBase m_UIEvent;

        private UIEventBase GetEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                //Logger.LogErrorContext(this,$"{name} 尝试获取一个空名称的事件 请检查");
                return null;
            }

            if (m_EventTable == null)
            {
                Logger.LogErrorContext(this, $"{name} 事件表==ull 请检查");
                return null;
            }

            var uiEvent = m_EventTable.FindEvent(eventName);
            if (uiEvent == null)
            {
                Logger.LogErrorContext(this, $"{name}没找到这个事件 {eventName} 请检查配置");
            }

            return uiEvent;
        }

        protected abstract bool IsTaskEvent { get; }

        protected abstract List<EUIEventParamType> GetFilterParamType { get; }

        private bool m_Binded;

        internal void Initialize(bool refresh = false)
        {
            if (!refresh && m_Binded) return;

            m_Binded = true;
            OnRefreshEvent();
        }

        private void RefreshEventName()
        {
            if (m_UIEvent != null)
            {
                m_EventName = m_UIEvent.EventName;
            }
        }

        protected virtual void RefreshBind()
        {
        }

        /// <summary>
        /// 销毁时清理
        /// 编辑器下额外做事件解绑 (UnbindEvent 定义于编辑器分部类)
        /// </summary>
        protected virtual void OnDestroy()
        {
#if UNITY_EDITOR
            UnbindEvent();
#endif
        }

        private void OnRefreshEvent()
        {
            RefreshEventTable();
            #if UNITY_EDITOR
            UnbindEvent();
            #endif
            RefreshEventName();
            m_UIEvent = GetEvent(m_EventName);
            RefreshEventName();
            #if UNITY_EDITOR
            BindEvent();
            #endif
            RefreshBind();
        }

        private void RefreshEventTable()
        {
            if (m_EventTable == null)
            {
                m_EventTable = this.GetComponentInParentHard<UIBindEventTable>();
            }
        }
    }
}