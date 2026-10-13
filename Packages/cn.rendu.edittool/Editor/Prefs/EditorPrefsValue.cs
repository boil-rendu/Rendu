using System;
using UnityEditor;

namespace RD.EditTool.Editor
{
    /// <summary>
    /// 基于 <see cref="EditorPrefs"/> 的强类型偏好, 单泛型替代原本 5 个具类
    /// (StringPrefs / FloatPrefs / IntPrefs / BoolPrefs / EnumPrefs&lt;T&gt;, 2026/10 Phase 2B 合并)。
    ///
    /// 支持的 T: <see cref="string"/>, <see cref="int"/>, <see cref="float"/>, <see cref="bool"/>, 枚举。
    /// 其它 T 在 <see cref="Value"/> 访问时抛 <see cref="NotSupportedException"/>。
    ///
    /// 用法:
    /// <code>
    /// private readonly EditorPrefsValue&lt;string&gt; m_Last = new("MyTool_Last", "默认值");
    /// string current = m_Last.Value;            // 读
    /// m_Last.Value = "new value";              // 写
    /// </code>
    ///
    /// 命名说明: "EditorPrefsValue" 显式带 "EditorPrefs" 前缀, 一是与 Unity 的
    /// <c>UnityEditor.EditorPrefs</c> (静态类) 隔开, 二是跟 cn.rendu.ui 的
    /// <c>RD.Client.{String,Float,Int,Bool,Enum}Prefs</c> (PlayerPrefs 系) 区分 —
    /// 那是另一个包里 PlayerPrefs 的强类型包装, 与本类型无任何关系, 也不构成命名冲突
    /// (类型名不同), 但要避免在 using 中同时引入这两个命名空间时产生混淆。
    /// </summary>
    public sealed class EditorPrefsValue<T>
    {
        private readonly string m_Key;
        private readonly T m_DefaultValue;

        public EditorPrefsValue(string key, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key 不能为空", nameof(key));
            m_Key          = key;
            m_DefaultValue = defaultValue;
        }

        public T Value
        {
            get => Get(m_Key, m_DefaultValue);
            set => Set(m_Key, value);
        }

        // EditorPrefs 原生支持 string/int/float/bool; enum 复用 int 通道。
        // 通过 typeof(T) 静态选分支 — IL2CPP 后端可裁剪掉未用分支。
        private static T Get(string key, T defaultValue)
        {
            if (typeof(T) == typeof(string)) return (T)(object)EditorPrefs.GetString(key, (string)(object)defaultValue);
            if (typeof(T) == typeof(int))    return (T)(object)EditorPrefs.GetInt(key,    (int)(object)defaultValue);
            if (typeof(T) == typeof(float))  return (T)(object)EditorPrefs.GetFloat(key,  (float)(object)defaultValue);
            if (typeof(T) == typeof(bool))   return (T)(object)EditorPrefs.GetBool(key,   (bool)(object)defaultValue);
            if (typeof(T).IsEnum)
            {
                var stored = EditorPrefs.GetInt(key, Convert.ToInt32(defaultValue));
                return Enum.IsDefined(typeof(T), stored) ? (T)Enum.ToObject(typeof(T), stored) : defaultValue;
            }
            throw new NotSupportedException(
                $"EditorPrefsValue<{typeof(T).Name}> 不支持, 仅支持 string/int/float/bool/enum");
        }

        private static void Set(string key, T value)
        {
            if (typeof(T) == typeof(string)) { EditorPrefs.SetString(key, (string)(object)value); return; }
            if (typeof(T) == typeof(int))    { EditorPrefs.SetInt(key,    (int)(object)value);    return; }
            if (typeof(T) == typeof(float))  { EditorPrefs.SetFloat(key,  (float)(object)value);  return; }
            if (typeof(T) == typeof(bool))   { EditorPrefs.SetBool(key,   (bool)(object)value);   return; }
            if (typeof(T).IsEnum)            { EditorPrefs.SetInt(key,    Convert.ToInt32(value)); return; }
            throw new NotSupportedException(
                $"EditorPrefsValue<{typeof(T).Name}> 不支持, 仅支持 string/int/float/bool/enum");
        }
    }
}
