//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using RD;
using RD.Panel;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    //[DetailedInfoBox("UI 组件表 点击展开详细介绍", @"李胜扬")]
    //[AddComponentMenu("UIBind/★★★UI Component Table 组件表★★★")]
    [HideLabel]
    [Serializable]
    [HideMonoScript]
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public sealed partial class UIBindComponentTable : MonoBehaviour, ISerializationCallbackReceiver
    {
        //序列化数据源 (Unity 原生可序列化键值列表)
        [SerializeField]
        [HideInInspector]
        internal List<UIBindPairData> m_AllBindPair = new List<UIBindPairData>();

        //运行时缓存字典 (编辑器下由 CheckAllBindName 同步)
        [NonSerialized]
        private Dictionary<string, Component> m_AllBindDic = new Dictionary<string, Component>();

        public IReadOnlyDictionary<string, Component> AllBindDic => m_AllBindDic;

        //m_AllBindPair 为源数据 无需额外处理
        public void OnBeforeSerialize()
        {
        }

        //Unity 原生序列化后恢复字典
        public void OnAfterDeserialize()
        {
            m_AllBindDic = new Dictionary<string, Component>();
            if (m_AllBindPair == null) return;

            foreach (var pair in m_AllBindPair)
            {
                if (pair == null || string.IsNullOrEmpty(pair.Name) || pair.Component == null) continue;
                if (!m_AllBindDic.ContainsKey(pair.Name))
                {
                    m_AllBindDic.Add(pair.Name, pair.Component);
                }
            }
        }

        private bool m_Initialized;

        private void Awake()
        {
            InitComponentTable();
        }

        public void InitComponentTable()
        {
            if (m_Initialized) return;
            m_Initialized = true;

            //可能的扩展初始化内容
        }

        private Component FindComponent(string comName)
        {
            m_AllBindDic.TryGetValue(comName, out var value);
            if (value == null)
            {
                Logger.LogErrorContext(this, $" {name} 组件表中没有这个组件 {comName}");
            }

            return value;
        }

        public T FindComponent<T>(string comName) where T : Component
        {
            return (T)FindComponent(comName);
        }
    }

    /// <summary>
    /// 组件绑定数据对应关系
    /// </summary>
    [Serializable]
    internal class UIBindPairData
    {
        public string Name;

        public Component Component;
    }
}