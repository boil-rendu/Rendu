//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using RD;
using RD.Draw;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    //[DetailedInfoBox("UI 事件表 点击展开详细介绍", @"李胜扬")]
    //[AddComponentMenu("UIBind/★★★UI Event Table 事件表★★★")]
    [HideLabel]
    [Serializable]
    [HideMonoScript]
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public sealed partial class UIBindEventTable : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        [HideInInspector]
        private List<UIBindEventTableItem> m_EventItems = new List<UIBindEventTableItem>();

        [NonSerialized]
        private Dictionary<string, UIEventBase> m_EventDic = new Dictionary<string, UIEventBase>();

        public IReadOnlyDictionary<string, UIEventBase> EventDic => m_EventDic;

        //Unity 原生序列化前的数据同步
        public void OnBeforeSerialize()
        {
            if (m_EventItems == null) return;
            m_EventItems.Clear();
            if (m_EventDic == null) return;

            foreach (var kv in m_EventDic)
            {
                var uiEvent = kv.Value;
                if (uiEvent == null || string.IsNullOrEmpty(kv.Key)) continue;
                m_EventItems.Add(new UIBindEventTableItem
                {
                    Name              = kv.Key,
                    IsTaskEvent       = uiEvent.IsTaskEvent,
                    AllEventParamType = new List<EUIEventParamType>(uiEvent.AllEventParamType),
                });
            }
        }

        //Unity 原生序列化后恢复字典
        //注意: UIEventBase 的泛型子类(UIEventP1<T> 等)无法被 [SerializeReference] 序列化
        //     (Unity 不支持泛型特化), 因此这里只存 名称/是否异步任务事件/参数类型列表
        //     读取时再通过 Helper 还原成正确的泛型实例
        public void OnAfterDeserialize()
        {
            m_EventDic = new Dictionary<string, UIEventBase>();
            if (m_EventItems == null) return;

            foreach (var item in m_EventItems)
            {
                if (item == null || string.IsNullOrEmpty(item.Name)) continue;

                var paramList = item.AllEventParamType ?? new List<EUIEventParamType>();
                var uiEvent = item.IsTaskEvent
                    ? UITaskEventBaseHelper.CreatorUITaskEventBase(item.Name, paramList)
                    : UIEventBaseHelper.CreatorUIEventBase(item.Name, paramList);

                if (uiEvent == null) continue;
                m_EventDic[item.Name] = uiEvent;
            }
        }

        private bool m_Initialized;

        private void Awake()
        {
            InitEventTable();
        }

        /// <summary>
        /// 显式初始化事件表
        /// 解决同一帧内激活-关闭导致 Awake 不触发的问题
        /// </summary>
        public void InitEventTable()
        {
            if (m_Initialized) return;
            m_Initialized = true;
            InitializeBinds(transform);
        }

        public UIEventBase FindEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Logger.LogError($"空的事件名称  请检查");
                return null;
            }

            return m_EventDic.TryGetValue(eventName, out var value) ? value : null;
        }

        public T FindEvent<T>(string eventName) where T : UIEventBase
        {
            return (T)FindEvent(eventName);
        }

        /// <summary>
        /// 清除指定事件
        /// </summary>
        public bool ClearEvent(string eventName)
        {
            if (!m_EventDic.TryGetValue(eventName, out var uiEvent))
            {
                Logger.LogError($"没有这个事件定义{eventName}");
                return false;
            }

            return uiEvent.Clear();
        }

        /// <summary>
        /// 清除所有事件
        /// 危险!! 运行时没这个需求
        /// </summary>
        public void ClearAllEvents()
        {
            foreach (var uiEvent in m_EventDic.Values)
            {
                uiEvent.Clear();
            }

            m_EventDic.Clear();
        }

        private void OnDestroy()
        {
            ClearAllEvents();
        }

        #region 递归初始化所有绑定数据

        private void InitializeBinds(Transform transform)
        {
            #if UIMACRO_BIND_INITIALIZE
            Logger.LogErrorContext(transform,$"{transform.name} 初始化调用所有子类 UIEventBind 绑定");
            #endif

            var binds = ListPool<UIEventBind>.Get();
            transform.GetComponents(binds);
            foreach (var bind in binds)
            {
                bind.Initialize(true);
            }

            ListPool<UIEventBind>.Put(binds);

            foreach (Transform child in transform)
            {
                InitializeBindsDeep(child);
            }
        }

        private void InitializeBindsDeep(Transform transform)
        {
            if (transform.HasComponent<UIBindEventTable>())
            {
                return;
            }

            var binds = ListPool<UIEventBind>.Get();
            transform.GetComponents(binds);
            foreach (var bind in binds)
            {
                bind.Initialize(true);
            }

            ListPool<UIEventBind>.Put(binds);

            foreach (Transform child in transform)
            {
                InitializeBindsDeep(child);
            }
        }

        #endregion
    }

    /// <summary>
    /// 事件表序列化条目 (Dictionary&lt;string, UIEventBase&gt; 的序列化包装)
    /// UIEventBase 的泛型子类无法被 Unity 的 [SerializeReference] 序列化,
    /// 因此仅序列化 名称/是否异步任务事件/参数类型列表, 读取时再还原成泛型实例
    /// </summary>
    [Serializable]
    internal class UIBindEventTableItem
    {
        public string Name;

        /// <summary>
        /// 是否为异步任务事件 (UITaskEventP* 而非 UIEventP*)
        /// </summary>
        public bool IsTaskEvent;

        /// <summary>
        /// 参数类型列表, 决定还原时泛型实例的具体类型
        /// </summary>
        public List<EUIEventParamType> AllEventParamType = new List<EUIEventParamType>();
    }
}