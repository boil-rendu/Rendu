using System;
using UnityEngine;

namespace RD.Utilities
{
    /// <summary>
    /// PlayerPrefs 的强类型包装: 把一个设置项当变量用。
    /// 首次读时惰性加载并缓存, 之后的读写走内存(值没变连写都跳过), 写即时进 PlayerPrefs
    /// (刷盘由 Unity 负责); 支持按 <see cref="IGroupKey"/>(如 userid)分组 —— key 自动拼成
    /// "分组键|key", 分组版本变化时删旧键、把值搬到新键。
    ///
    /// 支持的 T: <see cref="string"/> / <see cref="int"/> / <see cref="float"/> /
    /// <see cref="bool"/> / 枚举; 其它 T 在首次读写时抛 <see cref="NotSupportedException"/>。
    /// 数组请用 <see cref="ArrPlayerPrefsValue{T}"/>。
    ///
    /// 用法:
    /// <code>
    /// private readonly PrefsValue&lt;int&gt; m_Width = new("MyTool_Width", null, 300);
    /// int w = m_Width.Value;      // 读(首次惰性加载, 之后走缓存)
    /// m_Width.Value = 400;        // 写(即时进 PlayerPrefs)
    /// int w2 = m_Width;           // 隐式转换, 等价于读
    /// </code>
    /// </summary>
    public sealed class PrefsValue<T>
    {
        private T      m_value;
        private string m_key;
        private string m_valueKey;
        private RwFlag m_rwFlag;

        private int       m_groupKeyVer;
        private IGroupKey m_groupKey;
        private T         m_defValue;

        /// <summary>造一条偏好。</summary>
        /// <param name="key">偏好键; 分组下会拼成 "分组键|key"(见 <see cref="IGroupKey"/>)。</param>
        /// <param name="groupKey">分组键(如 userid); null = 不分组。</param>
        /// <param name="defValue">读不到时的默认值。</param>
        public PrefsValue(string key, IGroupKey groupKey = null, T defValue = default)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("key 不能为空", nameof(key));
            }

            m_defValue    = defValue;
            m_value       = defValue;
            m_groupKey    = groupKey;
            m_groupKeyVer = int.MinValue;
            m_rwFlag      = RwFlag.None;
            m_valueKey    = key;
            m_key         = key;

            EnsureSupported();   // 不支持的 T 尽早炸, 别等第一次读写才暴露
            UpdateKey();
        }

        /// <summary>当前值: 首次读时惰性加载并缓存, 之后走内存。</summary>
        public T Value
        {
            get
            {
                UpdateKey();

                // 即没读过也没写过: 才真的去 PlayerPrefs 拿
                if (!m_rwFlag.Overlaps(RwFlag.Full))
                {
                    if (HasKey(m_key))
                    {
                        m_value = Read(m_key);
                    }
                    else if (m_groupKey != null && HasKey(m_valueKey))
                    {
                        // 第一次时, 如果分组下的值拿不到, 就去尝试拿全局的值
                        m_value = Read(m_valueKey);
                    }
                    else
                    {
                        m_value = m_defValue;
                    }

                    m_rwFlag.MarkSelf(RwFlag.Read);
                }

                return m_value;
            }

            set
            {
                UpdateKey();
                // 已经加载过且值没变: 跳过落盘
                if (m_rwFlag.Overlaps(RwFlag.Full) && value.Equals(m_value))
                {
                    return;
                }

                m_value = value;
                m_rwFlag.MarkSelf(RwFlag.Write);
                Write(m_key, value);
            }
        }

        /// <summary>删掉这条偏好; 之后的读取回到默认值。</summary>
        public void Delete()
        {
            UpdateKey(true);
            m_groupKeyVer = int.MinValue;
            m_value       = m_defValue;
            m_rwFlag.MarkSelf(RwFlag.Write);
            PlayerPrefs.DeleteKey(m_key);
        }

        /// <summary>隐式读: <c>int w = m_Width;</c> 等价于 <c>m_Width.Value</c>。</summary>
        public static implicit operator T(PrefsValue<T> value)
        {
            return value.Value;
        }

        /// <summary>
        /// 对齐分组键版本: 版本变了就把 key 换成 "分组键|key", 已写过的值删旧键、写新键
        /// (Delete 只换键不搬值); 同时清掉读取标记 —— 下次读取按新键重新加载。
        /// </summary>
        /// <param name="notWrite">true = 只换 key, 不把值搬到新键(Delete 用)。</param>
        private void UpdateKey(bool notWrite = false)
        {
            if (m_groupKey == null)
            {
                return;
            }

            var curVer = m_groupKey.Version;
            if (curVer == m_groupKeyVer)
            {
                return;
            }

            m_groupKeyVer = curVer;
            var oldKey = m_key;
            m_key = StrUtil.Concat(m_groupKey.Key, "|", m_valueKey);

            if (m_rwFlag.Has(RwFlag.Write) && oldKey != m_key)
            {
                // 之前已经有写入: 删掉旧的, 按需写入新的
                PlayerPrefs.DeleteKey(oldKey);
                if (!notWrite)
                {
                    Write(m_key, m_value);
                }
            }

            // 清除读取标记
            m_rwFlag.UnmarkSelf(RwFlag.Read);
        }

        private static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        /// <summary>T 是否在支持列表里(string/int/float/bool/枚举); 不支持时构造即抛。</summary>
        /// <remarks>
        /// 放在构造里而不是等第一次读写: key 不存在时读取走默认值分支, 永远到不了
        /// <see cref="Read"/> 的类型分派 —— 不支持的 T 会静默返回默认值, 是个陷阱。
        /// </remarks>
        private static void EnsureSupported()
        {
            if (typeof(T) == typeof(string)) return;
            if (typeof(T) == typeof(int))    return;
            if (typeof(T) == typeof(float))  return;
            if (typeof(T) == typeof(bool))   return;
            if (typeof(T).IsEnum)            return;

            throw Unsupported();
        }

        /// <summary>
        /// 读一条: PlayerPrefs 原生只有 string/int/float, bool 与枚举都复用 int 通道
        /// (bool 存 0/1, 枚举存数值)。经 typeof(T) 静态选分支 —— 与 cn.rendu.edittool 的
        /// EditorPrefsValue&lt;T&gt; 同一套写法, IL2CPP 后端可裁掉未用分支。
        /// </summary>
        private T Read(string key)
        {
            if (typeof(T) == typeof(string)) return (T)(object)PlayerPrefs.GetString(key);
            if (typeof(T) == typeof(int))    return (T)(object)PlayerPrefs.GetInt(key);
            if (typeof(T) == typeof(float))  return (T)(object)PlayerPrefs.GetFloat(key);
            if (typeof(T) == typeof(bool))   return (T)(object)(PlayerPrefs.GetInt(key) == 1);
            if (typeof(T).IsEnum)
            {
                var stored = PlayerPrefs.GetInt(key);
                // 存的值不在枚举里(枚举定义改过): 退回默认值, 别把界面卡在非法态
                return Enum.IsDefined(typeof(T), stored) ? (T)Enum.ToObject(typeof(T), stored) : m_defValue;
            }

            throw Unsupported();
        }

        /// <summary>写一条: 与 <see cref="Read"/> 同一套类型分派。</summary>
        private void Write(string key, T value)
        {
            if (typeof(T) == typeof(string)) { PlayerPrefs.SetString(key, (string)(object)value); return; }
            if (typeof(T) == typeof(int))    { PlayerPrefs.SetInt(key, (int)(object)value); return; }
            if (typeof(T) == typeof(float))  { PlayerPrefs.SetFloat(key, (float)(object)value); return; }
            if (typeof(T) == typeof(bool))   { PlayerPrefs.SetInt(key, (bool)(object)value ? 1 : 0); return; }
            if (typeof(T).IsEnum)            { PlayerPrefs.SetInt(key, Convert.ToInt32(value)); return; }

            throw Unsupported();
        }

        private static NotSupportedException Unsupported()
        {
            return new NotSupportedException(
                $"PrefsValue<{typeof(T).Name}> 不支持, 仅支持 string/int/float/bool/enum");
        }
    }

    /// <summary>
    /// 数组偏好: T[] 序列化成一条字符串存 PlayerPrefs(元素分隔见 <see cref="StrConv"/>),
    /// 空数组等价于删掉这条偏好。适合"最近打开的列表"这类小数据。
    /// </summary>
    public struct ArrPlayerPrefsValue<T> where T : IComparable
    {
        private readonly PrefsValue<string> m_value;

        public ArrPlayerPrefsValue(string key, IGroupKey groupKey = null)
        {
            m_value = new PrefsValue<string>(key, groupKey, "");
        }

        /// <summary>读出数组; 没有存过时返回空数组。</summary>
        public T[] Get()
        {
            var value = m_value.Value;
            if (string.IsNullOrEmpty(value))
            {
                return Array.Empty<T>();
            }

            return StrConv.ToArr<T>(value, StrConv.ArrSplitLv1);
        }

        /// <summary>写入数组; null / 空数组等价于 <see cref="Delete"/>。</summary>
        public void Set(T[] value)
        {
            if (value == null || value.Length < 1)
            {
                Delete();
                return;
            }

            var sb = SbPool.Get();
            sb.Append(value[0].ToString());
            for (var i = 1; i < value.Length; i++)
            {
                sb.Append(StrConv.ChrArrSplitLv1).Append(value[i].ToString());
            }

            m_value.Value = SbPool.PutAndToStr(sb);
        }

        /// <summary>删掉这条偏好。</summary>
        public void Delete()
        {
            m_value.Delete();
        }
    }

    /// <summary>
    /// 分组键: 同一份偏好按分组各存一份(如按 userid), key 拼成 "分组键|key"。
    /// <see cref="Version"/> 变化时对齐到新 key(旧 key 删掉、值搬过去)。
    /// </summary>
    public interface IGroupKey
    {
        int    Version { get; }
        string Key     { get; }
    }

    /// <summary>读写标记: 惰性读(没读过才真的落盘读) + 跳过没变的写。</summary>
    [Flags]
    public enum RwFlag
    {
        None  = 0,
        Write = 1,
        Read  = 1 << 1,
        Full  = Write | Read
    }

    public static class RwFlagExt
    {
        public static void MarkSelf(this ref RwFlag owner, RwFlag flags)
        {
            owner |= flags;
        }

        public static void UnmarkSelf(this ref RwFlag owner, RwFlag flags)
        {
            owner &= ~flags;
        }

        public static bool Has(this RwFlag owner, RwFlag flags)
        {
            return (owner & flags) == flags;
        }

        public static bool Overlaps(this RwFlag owner, RwFlag flags)
        {
            return (owner & flags) != 0;
        }
    }
}
