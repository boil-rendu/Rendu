using System;
using RD.Draw;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    [Serializable]
    [HideReferenceObjectPicker]
    /// <summary>
    /// 数据变化引用：为某个 UIData 持有一份「用于比对/暂存的同类型数据值副本」
    /// 类型不匹配时会通过 UIDataHelper.GetNewDataValue 重新创建副本
    /// </summary>
    internal class UIDataChangeRef
    {
        //当前的变量
        [SerializeField]
        [HideInInspector]
        /// <summary>当前绑定的数据（运行时引用）</summary>
        private UIData m_Data;

        /// <summary>当前绑定的数据</summary>
        public UIData Data => m_Data;

        [ShowInInspector]
        [LabelText("名称")]
        [ReadOnly]
        /// <summary>数据名称（只读展示）</summary>
        private string m_DataName;

        #if UNITY_EDITOR
        [ShowInInspector]
        [LabelText("值")]
        [ReadOnly]
        /// <summary>数据值（仅编辑器只读展示）</summary>
        private string m_DataValue;
        #endif

        [SerializeField]
        [SerializeReference]
        /// <summary>用于比对变化的数据值副本</summary>
        private UIDataValue m_ChangeData;

        /// <summary>变化比对用的数据值副本</summary>
        public UIDataValue ChangeData => m_ChangeData;

        private UIDataChangeRef()
        {
        }

        /// <summary>按数据构造并立即刷新</summary>
        public UIDataChangeRef(UIData data)
        {
            Refresh(data);
        }

        //刷新数据
        /// <summary>刷新：同步数据引用/名称，类型不匹配时换用新的数据值副本</summary>
        public void Refresh(UIData data)
        {
            m_Data     = data;
            m_DataName = m_Data.Name;

            if (m_ChangeData == null || m_ChangeData.UIBindDataType != data.DataValue.UIBindDataType)
            {
                m_ChangeData = UIDataHelper.GetNewDataValue(data.DataValue);
            }

            #if UNITY_EDITOR
            m_DataValue = m_Data.GetValueToString();
            #endif
        }
    }
}
