using System;
using UnityEditor;

namespace RD.Inspector.Editor
{
    /// <summary>
    /// 基于 EditorPrefs 的字符串偏好
    /// </summary>
    public class StringPrefs
    {
        private readonly string m_Key;
        private readonly string m_DefaultValue;

        public StringPrefs(string key, string label = null, string defaultValue = "")
        {
            m_Key          = key;
            m_DefaultValue = defaultValue;
        }

        public string Value
        {
            get => EditorPrefs.GetString(m_Key, m_DefaultValue);
            set => EditorPrefs.SetString(m_Key, value);
        }
    }

    /// <summary>
    /// 基于 EditorPrefs 的浮点偏好
    /// </summary>
    public class FloatPrefs
    {
        private readonly string m_Key;
        private readonly float m_DefaultValue;

        public FloatPrefs(string key, string label = null, float defaultValue = 0f)
        {
            m_Key          = key;
            m_DefaultValue = defaultValue;
        }

        public float Value
        {
            get => EditorPrefs.GetFloat(m_Key, m_DefaultValue);
            set => EditorPrefs.SetFloat(m_Key, value);
        }
    }

    /// <summary>
    /// 基于 EditorPrefs 的整数偏好
    /// </summary>
    public class IntPrefs
    {
        private readonly string m_Key;
        private readonly int m_DefaultValue;

        public IntPrefs(string key, string label = null, int defaultValue = 0)
        {
            m_Key          = key;
            m_DefaultValue = defaultValue;
        }

        public int Value
        {
            get => EditorPrefs.GetInt(m_Key, m_DefaultValue);
            set => EditorPrefs.SetInt(m_Key, value);
        }
    }

    /// <summary>
    /// 基于 EditorPrefs 的布尔偏好
    /// </summary>
    public class BoolPrefs
    {
        private readonly string m_Key;
        private readonly bool m_DefaultValue;

        public BoolPrefs(string key, string label = null, bool defaultValue = false)
        {
            m_Key          = key;
            m_DefaultValue = defaultValue;
        }

        public bool Value
        {
            get => EditorPrefs.GetBool(m_Key, m_DefaultValue);
            set => EditorPrefs.SetBool(m_Key, value);
        }
    }

    /// <summary>
    /// 基于 EditorPrefs 的枚举偏好
    /// </summary>
    public class EnumPrefs<T> where T : struct, Enum
    {
        private readonly string m_Key;
        private readonly T m_DefaultValue;

        public EnumPrefs(string key, string label = null, T defaultValue = default)
        {
            m_Key          = key;
            m_DefaultValue = defaultValue;
        }

        public T Value
        {
            get
            {
                var stored = EditorPrefs.GetInt(m_Key, Convert.ToInt32(m_DefaultValue));
                return Enum.IsDefined(typeof(T), stored) ? (T)Enum.ToObject(typeof(T), stored) : m_DefaultValue;
            }
            set => EditorPrefs.SetInt(m_Key, Convert.ToInt32(value));
        }
    }
}
