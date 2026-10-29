using System;
using System.Collections.Generic;
using System.Reflection;

namespace RD.Draw.Editor.Metadata
{
    /// <summary>成员的种类: 决定它被分发到哪条绘制路径。</summary>
    internal enum MemberKind
    {
        Field,
        Property,
        MethodButton,
        MethodOnInspectorGUI,
    }

    /// <summary>
    /// 一个可绘制成员的元数据(字段 / 属性 / 按钮方法)。
    ///
    /// 它是"反射结果"的稳定视图: 反射拿到的 MemberInfo 每次绘制都要重新解释一遍
    /// (可见性、禁用、颜色、标签、按钮……), 而这些解释只跟类型有关, 所以在这里缓存一次。
    /// </summary>
    internal sealed class CachedMember
    {
        public MemberInfo Info;
        public MemberKind Kind;
        public int Seq;
        public double Order;
        public bool IsStatic;
        public object[] Attrs;
        public Type ValueType;

        /// <summary>
        /// 按特性类型缓存的筛选结果。
        /// 绘制一帧里同一个成员会被反复问特性(可见性、禁用、颜色、标签、按钮……),
        /// 若每次现场过滤 Attrs 并新建 List, 一个百来号成员的窗口每秒会产生上万次分配。
        /// 成员是按类型缓存的, 这里的结果因此可以长期复用, 稳态下零分配。
        /// </summary>
        private Dictionary<Type, Attribute[]> m_AttrByType;

        public string RawName => Info.Name;

        private string m_TypeLabel;
        private bool m_TypeLabelResolved;

        /// <summary>
        /// 成员"类型"上的 [LabelText](惰性解析一次后缓存), 没有时为 null。
        /// 只认枚举: 类是"一个泛型基类承载 N 个枚举"这类场景的主角, 而类/结构上的
        /// [LabelText] 语义是类型自身的显示名(组件菜单等), 拿它当字段标签会把
        /// 类引用字段的标签改成组件名。
        /// </summary>
        public string TypeLabel
        {
            get
            {
                if (m_TypeLabelResolved) return m_TypeLabel;
                m_TypeLabelResolved = true;

                var type = ValueType;
                if (type != null && type.IsEnum)
                {
                    var text = (Attribute.GetCustomAttribute(type, typeof(LabelTextAttribute), true)
                                as LabelTextAttribute)?.Text;
                    if (string.IsNullOrEmpty(text) == false)
                    {
                        m_TypeLabel = text;
                    }
                }

                return m_TypeLabel;
            }
        }

        /// <summary>取该成员上指定类型的全部特性(保持声明顺序), 没有时返回空数组。首次访问后结果被缓存。</summary>
        public T[] GetAttrs<T>() where T : Attribute
        {
            var key = typeof(T);
            if (m_AttrByType == null)
            {
                m_AttrByType = new Dictionary<Type, Attribute[]>();
            }
            else if (m_AttrByType.TryGetValue(key, out var cached))
            {
                return (T[])cached;
            }

            var count = 0;
            foreach (var attr in Attrs)
            {
                if (attr is T) count++;
            }

            var result = count == 0 ? Array.Empty<T>() : new T[count];
            if (count > 0)
            {
                var index = 0;
                foreach (var attr in Attrs)
                {
                    if (attr is T t) result[index++] = t;
                }
            }

            m_AttrByType[key] = result;
            return result;
        }

        /// <summary>取该成员上指定类型的第一个特性, 没有时返回 null。</summary>
        public T GetAttr<T>() where T : Attribute
        {
            var attrs = GetAttrs<T>();
            return attrs.Length > 0 ? attrs[0] : null;
        }
    }
}
