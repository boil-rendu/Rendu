using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using RD.Draw;
using RD.Draw.Editor;
using UnityEngine;

namespace RD.Inspector.Editor
{
    /// <summary>
    /// 自研的 ScriptableObject 基类。
    /// 把对象上所有 [InspectorSerialize] 标注的字段/属性 反射序列化为 JSON 文本,
    /// 通过 Unity 原生序列化落盘, 保持数据资产的读写能力。
    /// 支持类型: 数值/字符串/布尔/枚举/一维数组/List/Dictionary(键为字符串或数值)/嵌套 POCO。
    /// </summary>
    public class SerializedScriptableObject : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        [HideInInspector]
        private string m_InspectorData = string.Empty;

        [NonSerialized]
        private bool m_Deserializing;

        public void OnBeforeSerialize()
        {
            if (m_Deserializing) return;
            m_InspectorData = Serialization.Serialize(this);
        }

        public void OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(m_InspectorData)) return;

            m_Deserializing = true;
            try
            {
                Serialization.Deserialize(this, m_InspectorData);
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

    /// <summary>[InspectorSerialize] 成员集合的 JSON 序列化工具。</summary>
    public static class Serialization
    {
        private const BindingFlags MemberFlags =
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        /// <summary>
        /// 递归写出深度上限。
        /// 环引用(如 A.B = A)会让 WriteValue 无限展开, 表现为编辑器里无法捕获的 StackOverflow —— 直接闪退,
        /// 只能靠深度剪断: 到顶后写成 null, 与"不支持的对象一律写 null"的既有语义保持一致。
        /// </summary>
        private const int MaxDepth = 32;

        private static readonly Dictionary<Type, List<MemberInfo>> s_MemberCache =
                new Dictionary<Type, List<MemberInfo>>();

        //-------------------------------------- 序列化 --------------------------------------

        public static string Serialize(object root)
        {
            var sb = new StringBuilder(2048);
            if (root == null)
            {
                sb.Append("null");
                return sb.ToString();
            }

            WriteObject(sb, root, 0);
            return sb.ToString();
        }

        /// <summary>
        /// 取一个类型的可序列化成员。规则按类型分三条:
        /// Unity 值类型(见 <see cref="UnityValueTypeMembers"/>) — Unity 属性包类型(见
        /// <see cref="UnityPropertyBagMembers"/>) — 其余一律只认带 [InspectorSerialize] 的成员。
        /// internal: 校验工程要直接验证这三条规则, 不启动 IMGUI。
        /// </summary>
        internal static List<MemberInfo> GetDataMembers(Type type)
        {
            if (s_MemberCache.TryGetValue(type, out var cached)) return cached;

            List<MemberInfo> members;
            if (IsUnitySerializableValueType(type))
            {
                members = UnityValueTypeMembers(type);
            }
            else if (Engine.IsUnityPropertyBagType(type))
            {
                members = UnityPropertyBagMembers(type);
            }
            else
            {
                members = AnnotatedMembers(type);
            }

            s_MemberCache[type] = members;
            return members;
        }

        /// <summary>用户类型的规则: 只认标了 [InspectorSerialize] 的字段与属性。</summary>
        private static List<MemberInfo> AnnotatedMembers(Type type)
        {
            var members = new List<MemberInfo>();
            var seen = new HashSet<string>();

            foreach (var fi in type.GetFields(MemberFlags))
            {
                if (fi.IsDefined(typeof(InspectorSerializeAttribute), true) == false) continue;
                if (seen.Add(fi.Name)) members.Add(fi);
            }

            foreach (var pi in type.GetProperties(MemberFlags))
            {
                if (pi.IsDefined(typeof(InspectorSerializeAttribute), true) == false) continue;
                if (pi.GetIndexParameters().Length > 0) continue;
                if (seen.Add(pi.Name)) members.Add(pi);
            }

            return members;
        }

        /// <summary>
        /// Unity 内置的可序列化值类型(Vector2/3/4、Quaternion、Rect、Bounds、Color、LayerMask……)。
        /// 它们的数据成员既没法标 [InspectorSerialize](不是我们的类型), 也不该要求用户去改,
        /// 而"只认带特性的成员"这条规则会在它们身上推出空集合 —— 值被写成 {} , 读回来静默清零。
        /// </summary>
        private static bool IsUnitySerializableValueType(Type type)
        {
            if (type.IsValueType == false || type.IsPrimitive || type.IsEnum) return false;

            var ns = type.Namespace;
            return ns != null &&
                   (ns == "UnityEngine" || ns.StartsWith("UnityEngine.", StringComparison.Ordinal));
        }

        /// <summary>
        /// 取 Unity 值类型的数据成员: 先公开实例字段(Vector2 的 x/y、Quaternion 的 x/y/z/w……),
        /// 没有再退到全部实例字段 —— Rect / Bounds / LayerMask 把数据放在私有的 m_* 里, 而那正是
        /// Unity 自己序列化的那套状态, 比去拼公开派生属性(x/y/width/height/center/max……)精确得多。
        /// </summary>
        private static List<MemberInfo> UnityValueTypeMembers(Type type)
        {
            var members = new List<MemberInfo>();
            const BindingFlags flags = BindingFlags.Instance;

            foreach (var fi in type.GetFields(flags | BindingFlags.Public))
            {
                if (fi.IsInitOnly || fi.IsLiteral) continue;
                members.Add(fi);
            }

            if (members.Count > 0) return members;

            foreach (var fi in type.GetFields(flags | BindingFlags.NonPublic))
            {
                if (fi.IsInitOnly || fi.IsLiteral) continue;
                members.Add(fi);
            }

            return members;
        }

        /// <summary>
        /// Unity"属性包"类型(曲线/渐变)的判定在引擎侧: Engine.IsUnityPropertyBagType
        /// (绘制与序列化共用同一条规则; 曲线数据存于原生 m_Ptr, 不能按字段序列化,
        /// 也不能用 FormatterServices 绕过构造函数去造它)。
        /// </summary>

        /// <summary>
        /// 取属性包类型的数据成员: 只认公开读写属性(曲线的 keys/preWrapMode/postWrapMode、
        /// 渐变的 colorKeys/alphaKeys/mode)。
        /// </summary>
        private static List<MemberInfo> UnityPropertyBagMembers(Type type)
        {
            var members = new List<MemberInfo>();
            foreach (var pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (pi.GetIndexParameters().Length > 0) continue;
                if (pi.GetGetMethod() == null || pi.GetSetMethod() == null) continue;
                members.Add(pi);
            }

            return members;
        }

        private static void WriteObject(StringBuilder sb, object obj, int depth)
        {
            sb.Append('{');
            var type = obj.GetType();
            var members = GetDataMembers(type);
            if (members.Count == 0)
            {
                // 一个成员都取不到就只能写成 {} , 读回来必然是 default —— 这跟"静默丢数据"没有区别。
                // 把类型名点出来, 让"忘了标 [InspectorSerialize]"当场可见, 而不是等到读档才发现值没了。
                Log.WarnOnce($"no-member:{type.FullName}",
                                      $"[Inspector] {type.FullName} 没有任何可序列化成员, 本次只能写成 {{}}, " +
                                      "读回来会变回 default —— 它的数据成员需要标 [InspectorSerialize]。");
            }

            for (var i = 0; i < members.Count; i++)
            {
                if (i > 0) sb.Append(',');
                WriteString(sb, members[i].Name);
                sb.Append(':');
                WriteValue(sb, ReadMemberSafe(members[i], obj, type), depth + 1);
            }

            sb.Append('}');
        }

        /// <summary>
        /// 读成员值; 单个成员读失败(自定义 getter 抛异常、原生对象不可用等)只写 null 并记一条警告。
        /// 与写回侧(ApplyMembers)的容错对称: 一个坏成员不该让整份配置存不出来。
        /// </summary>
        private static object ReadMemberSafe(MemberInfo member, object obj, Type ownerType)
        {
            try
            {
                return ReadMember(member, obj);
            }
            catch (Exception e)
            {
                Log.WarnOnce($"read-member:{ownerType.FullName}.{member.Name}",
                                      $"[Inspector] 读取成员失败 {ownerType.Name}.{member.Name}: {e.Message}");
                return null;
            }
        }

        private static object ReadMember(MemberInfo member, object obj)
        {
            switch (member)
            {
                case FieldInfo fi:
                    return fi.GetValue(obj);
                case PropertyInfo pi:
                    return pi.GetValue(obj, null);
                default:
                    return null;
            }
        }

        private static bool IsPrimitiveLeaf(object value)
        {
            if (value == null) return true;
            var t = value.GetType();
            return t.IsPrimitive || t.IsEnum || value is string || value is decimal;
        }

        private static void WriteValue(StringBuilder sb, object value, int depth)
        {
            if (value == null || depth > MaxDepth)
            {
                sb.Append("null");
                return;
            }

            if (value is string str)
            {
                WriteString(sb, str);
                return;
            }

            if (value is bool b)
            {
                sb.Append(b ? "true" : "false");
                return;
            }

            if (value is char ch)
            {
                sb.Append((int)ch);
                return;
            }

            if (value is float f)
            {
                sb.Append(f.ToString("R", CultureInfo.InvariantCulture));
                return;
            }

            if (value is double d)
            {
                sb.Append(d.ToString("R", CultureInfo.InvariantCulture));
                return;
            }

            if (value is decimal dec)
            {
                sb.Append(dec.ToString(CultureInfo.InvariantCulture));
                return;
            }

            var type = value.GetType();
            if (type.IsPrimitive || type.IsEnum)
            {
                var longValue = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                sb.Append(longValue.ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (value is UnityEngine.Object)
            {
                sb.Append("null");
                return;
            }

            if (value is IDictionary dict)
            {
                sb.Append('{');
                var first = true;
                foreach (DictionaryEntry entry in dict)
                {
                    if (first == false) sb.Append(',');
                    first = false;
                    WriteString(sb, Convert.ToString(entry.Key, CultureInfo.InvariantCulture));
                    sb.Append(':');
                    WriteValue(sb, entry.Value, depth + 1);
                }

                sb.Append('}');
                return;
            }

            if (value is IEnumerable && value is not string)
            {
                sb.Append('[');
                var first = true;
                foreach (var item in (IEnumerable)value)
                {
                    if (first == false) sb.Append(',');
                    first = false;
                    WriteValue(sb, item, depth + 1);
                }

                sb.Append(']');
                return;
            }

            WriteObject(sb, value, depth);
        }

        private static void WriteString(StringBuilder sb, string text)
        {
            if (text == null)
            {
                sb.Append("null");
                return;
            }

            sb.Append('"');
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                switch (c)
                {
                    case '"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        if (c < 0x20)
                        {
                            sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            sb.Append(c);
                        }

                        break;
                }
            }

            sb.Append('"');
        }

        //-------------------------------------- 反序列化 --------------------------------------

        public static void Deserialize(object root, string json)
        {
            if (string.IsNullOrEmpty(json) || root == null) return;
            var pos = 0;
            var value = ParseValue(json, ref pos);
            if (value is Dictionary<string, object> map)
            {
                ApplyMembers(root, map);
            }
        }

        private static void ApplyMembers(object obj, Dictionary<string, object> map)
        {
            var members = GetDataMembers(obj.GetType());
            foreach (var member in members)
            {
                if (map.TryGetValue(member.Name, out var raw) == false) continue;

                var targetType = member is FieldInfo field
                                         ? field.FieldType
                                         : ((PropertyInfo)member).PropertyType;

                var converted = ConvertTo(targetType, raw);
                try
                {
                    if (member is FieldInfo fi)
                    {
                        fi.SetValue(obj, converted);
                    }
                    else if (member is PropertyInfo pi && pi.CanWrite)
                    {
                        pi.SetValue(obj, converted, null);
                    }
                }
                catch (Exception e)
                {
                    // 单个成员写回失败(类型对不上、setter 抛异常等)不该拖垮整个资产,
                    // 记一条警告后继续还原其余成员。
                    Log.Warn(
                        $"[Inspector] 反序列化成员失败 {obj.GetType().Name}.{member.Name}: {e.Message}");
                }
            }
        }

        private static object ConvertTo(Type target, object value)
        {
            if (value == null)
            {
                return target.IsValueType ? Activator.CreateInstance(target) : null;
            }

            if (target == typeof(string))
            {
                return value is string s ? s : Convert.ToString(value, CultureInfo.InvariantCulture);
            }

            if (target == typeof(bool))
            {
                return value is bool b ? b : Convert.ToBoolean(value, CultureInfo.InvariantCulture);
            }

            if (target.IsPrimitive || target.IsEnum)
            {
                var v = value;
                if (target == typeof(float)) return Convert.ToSingle(v, CultureInfo.InvariantCulture);
                if (target == typeof(double)) return Convert.ToDouble(v, CultureInfo.InvariantCulture);
                if (target == typeof(ulong)) return Convert.ToUInt64(v, CultureInfo.InvariantCulture);
                if (target == typeof(long)) return Convert.ToInt64(v, CultureInfo.InvariantCulture);
                // 枚举不实现 IConvertible, Convert.ChangeType 会抛 InvalidCastException, 必须走 Enum.ToObject。
                if (target.IsEnum) return Enum.ToObject(target, Convert.ToInt64(v, CultureInfo.InvariantCulture));
                return Convert.ChangeType(v, target, CultureInfo.InvariantCulture);
            }

            if (target == typeof(decimal)) return Convert.ToDecimal(value, CultureInfo.InvariantCulture);

            if (typeof(IDictionary).IsAssignableFrom(target))
            {
                return ConvertDictionary(target, value as Dictionary<string, object>);
            }

            if (value is IList list && typeof(IEnumerable).IsAssignableFrom(target) && target != typeof(string))
            {
                return ConvertCollection(target, list);
            }

            if (value is Dictionary<string, object> map)
            {
                var instance = CreateInstance(target);
                if (instance != null) ApplyMembers(instance, map);
                return instance;
            }

            return value;
        }

        private static object ConvertDictionary(Type target, Dictionary<string, object> map)
        {
            var args = target.GetGenericArguments();
            if (args.Length != 2 || map == null) return null;

            IDictionary dict;
            try
            {
                dict = (IDictionary)Activator.CreateInstance(target);
            }
            catch
            {
                // 接口(如 IDictionary<K,V>)或无默认构造的字典类型无法实例化: 放弃还原, 保留字段默认值
                return null;
            }

            foreach (var pair in map)
            {
                var key = ConvertTo(args[0], pair.Key);
                var val = ConvertTo(args[1], pair.Value);
                dict[key] = val;
            }

            return dict;
        }

        private static object ConvertCollection(Type target, IList source)
        {
            var elementType = GetElementType(target);
            if (elementType == null) return null;

            if (target.IsArray)
            {
                var array = Array.CreateInstance(elementType, source.Count);
                for (var i = 0; i < source.Count; i++)
                {
                    array.SetValue(ConvertTo(elementType, source[i]), i);
                }

                return array;
            }

            object container;
            try
            {
                container = Activator.CreateInstance(target);
            }
            catch
            {
                // 接口(如 IList<T>)无法实例化: 放弃还原
                return null;
            }

            if (container is IList list)
            {
                for (var i = 0; i < source.Count; i++)
                {
                    list.Add(ConvertTo(elementType, source[i]));
                }

                return list;
            }

            // HashSet<T> / ICollection<T> 这类不是 IList, 只能靠 Add(T) 填充,
            // 否则会在 (IList) 强转处抛 InvalidCastException 把整个反序列化打断。
            var add = target.GetMethod("Add", new[] { elementType });
            if (add == null) return null;
            for (var i = 0; i < source.Count; i++)
            {
                add.Invoke(container, new[] { ConvertTo(elementType, source[i]) });
            }

            return container;
        }

        /// <summary>取集合/数组的元素类型, 取不到时返回 null(调用方据此放弃还原)。</summary>
        private static Type GetElementType(Type target)
        {
            if (target.IsArray) return target.GetElementType();

            if (target.IsGenericType)
            {
                var args = target.GetGenericArguments();
                if (args.Length == 1) return args[0];
            }

            foreach (var itf in target.GetInterfaces())
            {
                if (itf.IsGenericType && itf.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return itf.GetGenericArguments()[0];
                }
            }

            return null;
        }

        private static object CreateInstance(Type type)
        {
            if (type.IsValueType) return Activator.CreateInstance(type);

            // 曲线/渐变必须走真实构造函数: FormatterServices.GetUninitializedObject 会绕开它,
            // 对 AnimationCurve 而言那意味着 m_Ptr 还是空的, 随后写 keys 就是在对空指针操作。
            if (Engine.IsUnityPropertyBagType(type))
            {
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch
                {
                    return null;
                }
            }
            try
            {
                return FormatterServices.GetUninitializedObject(type);
            }
            catch
            {
                try
                {
                    return Activator.CreateInstance(type, true);
                }
                catch
                {
                    return Activator.CreateInstance(type);
                }
            }
        }

        //-------------------------------------- 简易 JSON 解析 --------------------------------------

        private static object ParseValue(string json, ref int pos)
        {
            SkipWs(json, ref pos);
            if (pos >= json.Length) return null;

            var c = json[pos];
            switch (c)
            {
                case '{':
                    return ParseObject(json, ref pos);
                case '[':
                    return ParseArray(json, ref pos);
                case '"':
                    return ParseString(json, ref pos);
                case 't':
                    pos += 4;
                    return true;
                case 'f':
                    pos += 5;
                    return false;
                case 'n':
                    pos += 4;
                    return null;
                default:
                    return ParseNumber(json, ref pos);
            }
        }

        private static void SkipWs(string json, ref int pos)
        {
            while (pos < json.Length && (json[pos] == ' ' || json[pos] == '\t' ||
                                         json[pos] == '\r' || json[pos] == '\n'))
            {
                pos++;
            }
        }

        private static Dictionary<string, object> ParseObject(string json, ref int pos)
        {
            var map = new Dictionary<string, object>();
            pos++; // {
            SkipWs(json, ref pos);
            if (pos < json.Length && json[pos] == '}')
            {
                pos++;
                return map;
            }

            while (pos < json.Length)
            {
                SkipWs(json, ref pos);
                var key = ParseString(json, ref pos);
                SkipWs(json, ref pos);
                if (pos < json.Length && json[pos] == ':') pos++;
                var val = ParseValue(json, ref pos);
                map[key] = val;
                SkipWs(json, ref pos);
                if (pos < json.Length && json[pos] == ',')
                {
                    pos++;
                    continue;
                }

                if (pos < json.Length && json[pos] == '}')
                {
                    pos++;
                    break;
                }
            }

            return map;
        }

        private static List<object> ParseArray(string json, ref int pos)
        {
            var list = new List<object>();
            pos++; // [
            SkipWs(json, ref pos);
            if (pos < json.Length && json[pos] == ']')
            {
                pos++;
                return list;
            }

            while (pos < json.Length)
            {
                list.Add(ParseValue(json, ref pos));
                SkipWs(json, ref pos);
                if (pos < json.Length && json[pos] == ',')
                {
                    pos++;
                    continue;
                }

                if (pos < json.Length && json[pos] == ']')
                {
                    pos++;
                    break;
                }
            }

            return list;
        }

        private static string ParseString(string json, ref int pos)
        {
            pos++; // 开引号
            var sb = new StringBuilder();
            while (pos < json.Length)
            {
                var c = json[pos];
                if (c == '"')
                {
                    pos++;
                    break;
                }

                if (c == '\\' && pos + 1 < json.Length)
                {
                    var esc = json[pos + 1];
                    switch (esc)
                    {
                        case '"':
                        case '\\':
                        case '/':
                            sb.Append(esc);
                            pos += 2;
                            break;
                        case 'n':
                            sb.Append('\n');
                            pos += 2;
                            break;
                        case 'r':
                            sb.Append('\r');
                            pos += 2;
                            break;
                        case 't':
                            sb.Append('\t');
                            pos += 2;
                            break;
                        case 'u':
                            // 手写解析器不能假设输入一定是完整 JSON: 截断的 \uXXXX 直接 Substring 会抛异常,
                            // 而这条路径跑在 OnAfterDeserialize 里, 抛出去就是每次域重载刷一条报错。
                            if (pos + 6 <= json.Length &&
                                int.TryParse(json.Substring(pos + 2, 4), NumberStyles.HexNumber,
                                             CultureInfo.InvariantCulture, out var code))
                            {
                                sb.Append((char)code);
                                pos += 6;
                            }
                            else
                            {
                                pos = json.Length;
                            }

                            break;
                        default:
                            sb.Append(esc);
                            pos += 2;
                            break;
                    }

                    continue;
                }

                sb.Append(c);
                pos++;
            }

            return sb.ToString();
        }

        private static object ParseNumber(string json, ref int pos)
        {
            var start = pos;
            var isFloat = false;
            while (pos < json.Length)
            {
                var c = json[pos];
                if (c is >= '0' and <= '9' || c == '-' || c == '+') pos++;
                else if (c == '.' || c == 'e' || c == 'E') { isFloat = true; pos++; }
                else break;
            }

            var text = json.Substring(start, pos - start);
            if (isFloat)
            {
                return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : 0d;
            }

            return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l) ? l : 0L;
        }
    }
}
