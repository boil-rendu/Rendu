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
    //[DetailedInfoBox("UI 数据表 点击展开详细介绍", @"李胜扬")]
    //[AddComponentMenu("UIBind/★★★UI Data Table 数据表★★★")]
    [HideLabel]
    [Serializable]
    [HideMonoScript]
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public sealed partial class UIBindDataTable : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        [HideInInspector]
        private List<UIBindDataTableItem> m_DataItems = new List<UIBindDataTableItem>();

        [NonSerialized]
        private Dictionary<string, UIData> m_DataDic = new Dictionary<string, UIData>();

        public IReadOnlyDictionary<string, UIData> DataDic => m_DataDic;

        //Unity 原生序列化前的数据同步
        public void OnBeforeSerialize()
        {
            if (m_DataItems == null) return;
            m_DataItems.Clear();
            if (m_DataDic == null) return;

            foreach (var kv in m_DataDic)
            {
                if (kv.Value == null || string.IsNullOrEmpty(kv.Key)) continue;
                m_DataItems.Add(new UIBindDataTableItem { Name = kv.Key, Data = kv.Value });
            }
        }

        //Unity 原生序列化后恢复字典
        public void OnAfterDeserialize()
        {
            m_DataDic = new Dictionary<string, UIData>();
            if (m_DataItems == null) return;

            foreach (var item in m_DataItems)
            {
                if (item?.Data == null || string.IsNullOrEmpty(item.Name)) continue;
                m_DataDic[item.Name] = item.Data;
            }
        }

        private bool m_Initialized;

        private void Awake()
        {
            InitDataTable();
        }

        /// <summary>
        /// 显式初始化数据表
        /// 解决同一帧内激活-关闭导致 Awake 不触发的问题
        /// </summary>
        public void InitDataTable()
        {
            if (m_Initialized) return;
            m_Initialized = true;
            InitializeBinds(transform);
        }

        public UIData FindData(string dataName)
        {
            if (string.IsNullOrEmpty(dataName)) return null;

            return this.m_DataDic.GetValueOrDefault(dataName);
        }

        public T FindDataValue<T>(string dataName) where T : UIDataValue
        {
            var uiData = FindData(dataName);
            if (uiData == null)
            {
                Logger.LogErrorContext(this, $"{name} 未找到这个数据请检查 {dataName}");
                return default;
            }

            if (uiData.DataValue == null)
            {
                Logger.LogErrorContext(this, $"{name} 数据没有初始化没有值 {dataName}");
                return default;
            }

            return (T)uiData.DataValue;
        }

        #region 递归初始化所有绑定数据

        private void InitializeBinds(Transform transform)
        {
            #if UIMACRO_BIND_INITIALIZE
            Logger.LogErrorContext(transform,$"{transform.name} 初始化调用所有子类 UIDataBind 绑定");
            #endif
            var binds = ListPool<UIDataBind>.Get();
            transform.GetComponents(binds);
            foreach (var bind in binds)
            {
                bind.Initialize(true);
            }

            ListPool<UIDataBind>.Put(binds);

            foreach (Transform child in transform)
            {
                InitializeBindsDeep(child);
            }
        }

        private void InitializeBindsDeep(Transform transform)
        {
            if (transform.HasComponent<UIBindDataTable>())
            {
                return;
            }

            var binds = ListPool<UIDataBind>.Get();
            transform.GetComponents(binds);
            foreach (var bind in binds)
            {
                bind.Initialize(true);
            }

            ListPool<UIDataBind>.Put(binds);

            foreach (Transform child in transform)
            {
                InitializeBindsDeep(child);
            }
        }

        #endregion
    }

    /// <summary>
    /// 数据表序列化条目 (Dictionary<string, UIData> 的序列化包装)
    /// </summary>
    [Serializable]
    internal class UIBindDataTableItem
    {
        public string Name;

        public UIData Data;
    }
}