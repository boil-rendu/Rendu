using System;
using System.Collections.Generic;
using RD.Draw;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI 事件基类（同步/异步事件的公共部分，partial）
    /// 持有事件名与参数类型列表；具体派发由子类 UIEventP*（同步）/ UITaskEventP*（异步）实现
    /// </summary>
    [Serializable]
    [HideReferenceObjectPicker]
    public abstract partial class UIEventBase
    {
        [LabelText("同步事件")]
        [SerializeField]
        [ReadOnly]
        [HideIf("IsTaskEvent")]
        [PropertyOrder(-10)]
        #if UNITY_EDITOR
        [InfoBox("此事件没有任何关联", InfoMessageType.Warning, "ShowIfBindsTips")]
        #endif
        private string m_EventName;

        /// <summary>事件名（异步事件在 Inspector 中可见）</summary>
        [LabelText("异步事件")]
        [ReadOnly]
        [ShowInInspector]
        [ShowIf("IsTaskEvent")]
        [PropertyOrder(-10)]
        #if UNITY_EDITOR
        [InfoBox("此事件没有任何关联", InfoMessageType.Warning, "ShowIfBindsTips")]
        #endif
        public string EventName => m_EventName;

        /// <summary>当前所有参数类型列表</summary>
        [SerializeField]
        [ReadOnly]
        [LabelText("当前所有参数列表")]
        #if UNITY_EDITOR
        [ShowIf("ShowIfAllEventParamType")]
        #endif
        public List<EUIEventParamType> AllEventParamType = new List<EUIEventParamType>();

        /// <summary>是否为异步（Task）事件</summary>
        public abstract bool IsTaskEvent { get; }

        /// <summary>刷新参数类型列表</summary>
        public void RefreshAllEventParamType(List<EUIEventParamType> targetType)
        {
            AllEventParamType.Clear();
            AllEventParamType.AddRange(targetType);
        }

        /// <summary>设置事件名</summary>
        public void SetName(string name)
        {
            m_EventName = name;
        }

        protected UIEventBase()
        {
        }

        protected UIEventBase(string name)
        {
            m_EventName = name;
        }

        /// <summary>清空事件（释放所有句柄）</summary>
        public abstract bool Clear();
    }
}
