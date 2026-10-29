using System;
using UnityEngine;

namespace RD.Serialization.Editor
{
    /// <summary>
    /// 自研的 ScriptableObject 基类(编辑器数据资产载体)。
    /// 把对象上所有 [Serialize] 标注的字段/属性 反射序列化为 JSON 文本,
    /// 通过 Unity 原生序列化落盘, 保持数据资产的读写能力。
    /// 支持类型: 数值/字符串/布尔/枚举/一维数组/List/Dictionary(键为字符串或数值)/嵌套 POCO。
    /// JSON 读写内核(Serialization)与本类同在 RD.Serialization.Editor 程序集; 成员规则与绘制引擎同源
    /// ([Serialize] + Unity 属性包判定, 后者复用引擎公开的 Engine.IsUnityValueBagType), 全仓只有一份;
    /// [CustomEditor] 整族注册在 cn.rendu.inspector(SerializedScriptableObjectEditor)。
    /// </summary>
    public class SerializedScriptableObject : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        [HideInInspector]
        private string m_Data = string.Empty;

        [NonSerialized]
        private bool m_Deserializing;

        public void OnBeforeSerialize()
        {
            if (m_Deserializing) return;
            m_Data = Serialization.Serialize(this);
        }

        public void OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(m_Data)) return;

            m_Deserializing = true;
            try
            {
                Serialization.Deserialize(this, m_Data);
            }
            finally
            {
                m_Deserializing = false;
            }

            OnAfterInspectorDeserialize();
        }

        /// <summary>数据反序列化完成后调用(可重写以重算派生状态)。</summary>
        protected virtual void OnAfterInspectorDeserialize()
        {
        }
    }
}
