#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using RD.Inspector;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UIData 的编辑器扩展（UNITY_EDITOR）
    /// 负责：数据改名派发变更事件、Inspector 上展示「所有绑定关联」、以及移除数据时解绑清理
    /// </summary>
    public sealed partial class UIData
    {
        /// <summary>数据变更事件（改名时触发）</summary>
        public event Action<UIData> OnDataChangAction;

        /// <summary>重命名数据并派发变更事件</summary>
        public void OnDataChange(string name)
        {
            m_Name = name;
            try
            {
                OnDataChangAction?.Invoke(this);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        //移除注册
        /// <summary>移除注册回调：点「移除」按钮时触发</summary>
        [HideInInspector]
        public Action<UIData> OnDataRemoveAction;

        [GUIColor(0, 1, 1)]
        [Button("移除")]
        [PropertyOrder(100)]
        [ShowIf("@UIOperationHelper.CommonShowIf()")]
        private void OnRemoveDataClick()
        {
            try
            {
                OnDataRemoveAction?.Invoke(this);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        //确定移除
        /// <summary>确定移除：解除本数据与所有绑定目标的关系（逆序解绑）</summary>
        public void OnDataRemoveCallBack()
        {
            if (m_Binds == null || m_Binds.Count <= 0)
            {
                return;
            }

            for (var i = m_Binds.Count - 1; i >= 0; i--)
            {
                m_Binds[i].RemoveBindData(this);
            }
        }

        private bool ShowIfBindsTips => m_Binds.Count <= 0;
        private bool ShowIfBinds     => m_Binds.Count >= 1;

        [SerializeField]
        [HideReferenceObjectPicker]
        [LabelText("所有绑定关联")]
        [ReadOnly]
        [PropertyOrder(101)]
        [ShowIf("ShowIfBinds")]
        private List<UIDataBind> m_Binds = new List<UIDataBind>();

        /// <summary>获取当前绑定数量</summary>
        public int GetBindCount()
        {
            return m_Binds?.Count ?? 0;
        }

        /// <summary>登记一个绑定关联（去重）</summary>
        internal void AddBind(UIDataBind bind)
        {
            if (m_Binds?.IndexOf(bind) == -1)
            {
                m_Binds?.Add(bind);
            }
        }

        /// <summary>移除一个绑定关联</summary>
        internal void RemoveBind(UIDataBind bind)
        {
            m_Binds?.Remove(bind);
        }

        /// <summary>清空所有绑定关联</summary>
        internal void ClearBinds()
        {
            m_Binds?.Clear();
        }
    }
}
#endif
