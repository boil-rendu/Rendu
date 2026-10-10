#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using RD.EditorKit;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UIEventBase 的编辑器扩展（partial，UNITY_EDITOR）
    /// 提供事件在 Inspector 上的「移除」按钮、绑定目标列表展示，以及类型名/参数名查询
    /// </summary>
    public abstract partial class UIEventBase
    {
        /// <summary>存在参数时才显示「当前所有参数列表」</summary>
        private bool ShowIfAllEventParamType => AllEventParamType.Count >= 1;

        /// <summary>取第 index 个参数的类型名字符串</summary>
        protected string GetParamTypeString(int index)
        {
            var current = AllEventParamType[index];
            return current.GetParamTypeString();
        }

        /// <summary>取事件类型名（如 UIEventP1&lt;int&gt;）</summary>
        public abstract string GetEventType();

        /// <summary>取事件句柄类型名（如 UIEventHandleP1&lt;int&gt;）</summary>
        public abstract string GetEventHandleType();

        /// <summary>改名（编辑器内）</summary>
        public void ChangeName(string eventName)
        {
            m_EventName = eventName;
        }

        //移除注册
        /// <summary>移除注册回调：点「移除」按钮时触发</summary>
        [HideInInspector]
        public Action<UIEventBase> OnRemoveAction;

        //确定移除
        /// <summary>确定移除：解除本事件与所有绑定目标的关系（逆序解绑）</summary>
        public void OnRemoveVariableCallBack()
        {
            if (m_Binds == null || m_Binds.Count <= 0)
            {
                return;
            }

            for (var i = m_Binds.Count - 1; i >= 0; i--)
            {
                m_Binds[i].RemoveBind(this);
            }
        }

        [GUIColor(0, 1, 1)]
        [Button("移除")]
        [PropertyOrder(-99)]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        private void RemoveEvent()
        {
            try
            {
                OnRemoveAction?.Invoke(this);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        private bool ShowIfBinds     => m_Binds.Count >= 1;
        private bool ShowIfBindsTips => m_Binds.Count <= 0;

        [ShowInInspector]
        [SerializeField]
        [LabelText("事件绑定")]
        [ReadOnly]
        [ShowIf("ShowIfBinds")]
        private List<UIEventBind> m_Binds = new List<UIEventBind>();

        /// <summary>获取当前绑定数量</summary>
        public int GetBindCount()
        {
            return m_Binds?.Count ?? 0;
        }

        /// <summary>登记一个绑定（去重）</summary>
        internal void AddBind(UIEventBind bind)
        {
            if (m_Binds?.IndexOf(bind) == -1)
            {
                m_Binds?.Add(bind);
            }
        }

        /// <summary>移除一个绑定</summary>
        internal void RemoveBind(UIEventBind bind)
        {
            m_Binds?.Remove(bind);
        }

        /// <summary>清空所有绑定</summary>
        internal void ClearBinds()
        {
            m_Binds?.Clear();
        }
    }
}
#endif
