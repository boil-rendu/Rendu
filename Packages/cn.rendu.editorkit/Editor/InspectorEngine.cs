using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace RD.EditorKit.Editor
{
    /// <summary>
    /// Inspector 属性渲染引擎
    /// 依据反射 + Runtime 层特性即时绘制任意 C# 对象
    /// </summary>
    public static class InspectorEngine
    {
        private const int MaxDepth = 24;

        internal enum MemberKind
        {
            Field,
            Property,
            MethodButton,
            MethodOnInspectorGUI,
        }

        internal class CachedMember
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

        internal enum GroupKind
        {
            None,
            Box,
            Foldout,
            Vertical,
            Horizontal,
            Title,
            ButtonGroup,
        }

        /// <summary>
        /// 分组树节点。PropertyGroupAttribute 语义:
        /// 分组属性通过 GroupID("父/子" 路径) 把成员挂到一棵分组树上, GroupName 只取路径末段;
        /// 同级分组按 Order 排序; 没有挂分组属性的成员直接挂在根节点上(根节点永远可见)。
        /// 用"树"而不是"边遍历边开关 GUI 的状态机", 才能正确处理分组嵌套
        /// (如 [VerticalGroup("信息")] + [ButtonGroup("信息/更新版本")])与折叠,
        /// 也不会出现 Begin/End 失衡, 或"未分组成员被误判为折叠组而整组跳过"这类问题。
        /// </summary>
        internal sealed class GroupNode
        {
            public GroupKind Kind;
            public string GroupId = "";
            public string GroupName = "";
            public double Order;
            public int Seq;
            public bool HideWhenChildrenAreInvisible = true;
            public string VisibleIf;
            public bool ShowLabel = true;
            public bool CenterLabel;
            public bool Expanded;
            public bool HorizontalLine = true;
            public bool BoldTitle = true;
            public TitleAlignments Alignment;
            public GroupNode Parent;
            public readonly List<GroupNode> Children = new();
            public readonly List<CachedMember> Members = new();
        }

        /// <summary>由一个分组属性解析出的描述(一个分组属性实例)。</summary>
        private struct GroupDescriptor
        {
            public string GroupId;
            public double Order;
            public GroupKind Kind;
            public bool ShowLabel;
            public bool CenterLabel;
            public bool Expanded;
            public bool HorizontalLine;
            public bool BoldTitle;
            public TitleAlignments Alignment;
            public bool HideWhenChildrenAreInvisible;
            public string VisibleIf;
        }

        /// <summary>分组内待绘制的条目: 要么是一个成员, 要么是一个子分组。</summary>
        private struct GroupEntry
        {
            public CachedMember Member;
            public GroupNode Group;
            public double Order;
            public int Seq;
        }

        private static readonly Dictionary<Type, CachedMember[]> s_Cache = new();
        private static readonly Dictionary<Type, GroupNode> s_GroupTreeCache = new();
        private static readonly Dictionary<Type, List<TableColumn>> s_TableColumnCache = new();

        /// <summary>成员/方法查找的可见性范围: 只认本层声明, 逐层向基类走由查找方自己控制。</summary>
        private const BindingFlags MemberLookupFlags = BindingFlags.Public | BindingFlags.NonPublic |
                                                      BindingFlags.DeclaredOnly;

        /// <summary>按 (类型, 方法名, 形参个数, 是否只认静态) 缓存的方法查找结果, 含"没有"这个否定结果(null 值)。</summary>
        private static readonly Dictionary<(Type, string, int, bool), MethodInfo> s_MethodCache = new();

        /// <summary>按 (类型, 名字, 是否只认静态) 缓存的字段/属性查找结果, 含"没有"这个否定结果(null 值)。</summary>
        private static readonly Dictionary<(Type, string, bool), MemberInfo> s_FieldPropertyCache = new();

        /// <summary>
        /// 只按引用比较对象身份, 不受业务类型重写 Equals/GetHashCode 的影响。
        /// 绘制环检测若用默认比较器, 会把"两个 Equals 判定相等、但并非同一个"的实例误判为
        /// 同一个对象正在绘制, 从而整块跳过绘制。
        /// </summary>
        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new();

            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            int IEqualityComparer<object>.GetHashCode(object obj)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            }
        }

        private static readonly HashSet<object> s_DrawingObjects = new(ReferenceComparer.Instance);

        /// <summary>宿主未显式提供会话时的兜底, 使引擎可脱离宿主单独使用(也便于测试)。</summary>
        private static readonly InspectorSession s_DefaultSession = new();

        /// <summary>本次绘制期间生效的会话, 由 DrawObject 在进入/退出时挂载与还原。</summary>
        private static InspectorSession s_Session;

        /// <summary>当前生效的会话: 折叠状态与滚动位置都存在这里, 不再使用进程级静态字典。</summary>
        internal static InspectorSession Session => s_Session ?? s_DefaultSession;

        //-------------------------------------- 对外入口 --------------------------------------

        /// <summary>数据即将被写入前的回调: 宿主应在此刻拍 Undo 快照(必须在修改之前)。</summary>
        private static Action<object> s_BeforeChange;

        /// <summary>数据已被修改后的回调: 宿主应标记脏并请求重绘。</summary>
        private static Action<object> s_Changed;

        /// <summary>
        /// 绘制任意对象。
        /// host 为 EditorWindow 时表示窗口自绘(可绘制窗口自身); 传 null 表示宿主是 UnityEditor.Editor,
        /// 此时根对象允许是 MonoBehaviour / ScriptableObject —— 但其成员里的 Unity 对象仍不会被内窥, 除非标了 [InlineEditor]。
        /// onBeforeChange: 数据即将写入(供宿主记录 Undo); onChanged: 数据已变化(供宿主标记脏 / 重绘)。
        /// </summary>
        public static void DrawObject(object target, EditorWindow host,
                                      Action<object> onBeforeChange = null,
                                      Action<object> onChanged = null)
        {
            DrawObject(target, host, onBeforeChange, onChanged, null);
        }

        /// <summary>
        /// 绘制任意对象, 并显式指定本次绘制使用的状态会话。
        /// 该重载只供包内宿主使用: InspectorSession 是内部实现类型, 不对外暴露。
        /// </summary>
        internal static void DrawObject(object target, EditorWindow host,
                                        Action<object> onBeforeChange,
                                        Action<object> onChanged,
                                        InspectorSession session)
        {
            if (target == null) return;
            // 已销毁的 Unity 对象(fake null)不再绘制
            if (target is UnityEngine.Object unityTarget && unityTarget == null) return;

            var previousBefore = s_BeforeChange;
            var previousChanged = s_Changed;
            var previousSession = s_Session;
            s_BeforeChange = onBeforeChange;
            s_Changed = onChanged;
            s_Session = session ?? s_DefaultSession;
            try
            {
                // 根对象始终允许绘制: EditorWindow 自绘, 或 Editor 检视组件
                DrawRecursive(target, host, 0, null, true);
            }
            catch (Exception e)
            {
                InspectorLog.Error($"[Inspector] 绘制失败 {target.GetType().Name}\n{e}");
            }
            finally
            {
                s_BeforeChange = previousBefore;
                s_Changed = previousChanged;
                s_Session = previousSession;
            }
        }

        /// <summary>
        /// 执行一次可能修改数据的操作, 并把"变更前 / 变更后"通知给宿主。
        /// 引擎里所有写数据的地方(成员赋值、集合增删改、按钮方法调用)都走这里,
        /// 这样 UnityEditor.Editor 宿主才能统一做 Undo 快照与标记脏。
        /// </summary>
        private static void Mutate(object owner, Action action)
        {
            s_BeforeChange?.Invoke(owner);
            action();
            s_Changed?.Invoke(owner);
        }

        private static void DrawRecursive(object target, EditorWindow host, int depth, object parent,
                                           bool allowUnityObject = false)
        {
            if (target == null || depth > MaxDepth) return;
            // 默认不内窥 UnityEngine.Object(只允许宿主窗口自绘), [InlineEditor] 显式展开时传 allowUnityObject
            if (allowUnityObject == false && target is UnityEngine.Object unityObject &&
                ReferenceEquals(unityObject, host) == false)
            {
                return;
            }

            var type = target.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(string)) return;

            if (parent != null && ReferenceEquals(target, parent)) return;
            // [InlineEditor] 可能构成自引用, 用"正在绘制"集合剪断环
            if (s_DrawingObjects.Contains(target)) return;

            s_DrawingObjects.Add(target);
            try
            {
                var members = GetMembers(type);
                RenderMembers(target, members, host, depth);
            }
            finally
            {
                s_DrawingObjects.Remove(target);
            }
        }

        //-------------------------------------- 有意不实现的属性 --------------------------------------
        // 以下属性在 Inspector 的架构下天然成立, 因此不做(也不该做)额外实现:
        //   [HideMonoScript]            Inspector 只绘制 EditorWindow 自身与其成员, 从不绘制 MonoBehaviour
        //                               的 Script 引用栏, 因此没有可隐藏的对象。
        //   [HideReferenceObjectPicker] 本 Inspector 没有引用对象选择器(基类/接口的派生类型下拉),
        //                               用户类一律按实际类型内联展开, 等价于"始终隐藏选择器"。
        // 另: [HideInTables] / [Searchable] / [OnInspectorInit] 业务侧目前无使用, 暂不实现。

        //-------------------------------------- 成员收集与缓存 --------------------------------------

        internal static CachedMember[] GetMembers(Type type)
        {
            if (s_Cache.TryGetValue(type, out var cached))
            {
                return cached;
            }

            var list = new List<CachedMember>();
            var seq = 0;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
                                       BindingFlags.Public | BindingFlags.NonPublic |
                                       BindingFlags.FlattenHierarchy;

            foreach (var field in type.GetFields(flags))
            {
                if (field.IsDefined(typeof(HideInInspector), true)) continue;
                if (field.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true)) continue;
                if (IsEngineInternal(field)) continue;

                // 可见性规则: 公开字段、Unity 序列化字段([SerializeField])、
                // 序列化字段([InspectorSerialize]) 与显式 [ShowInInspector] 都显示, 其余私有成员隐藏
                var show = field.IsDefined(typeof(ShowInInspectorAttribute), true) ||
                           field.IsDefined(typeof(SerializeField), true) ||
                           field.IsDefined(typeof(InspectorSerializeAttribute), true) ||
                           (field.IsPublic && field.IsStatic == false);
                if (show == false) continue;

                list.Add(new CachedMember
                {
                    Info = field,
                    Kind = MemberKind.Field,
                    Seq = seq++,
                    IsStatic = field.IsStatic,
                    ValueType = field.FieldType,
                    Order = GetOrder(field),
                    Attrs = field.GetCustomAttributes(false),
                });
            }

            foreach (var prop in type.GetProperties(flags))
            {
                if (prop.IsDefined(typeof(HideInInspector), true)) continue;
                if (prop.GetMethod == null) continue;
                if (IsEngineInternal(prop)) continue;
                // 属性需 [ShowInInspector] 或 [InspectorSerialize] 才显示
                if (prop.IsDefined(typeof(ShowInInspectorAttribute), true) == false &&
                    prop.IsDefined(typeof(InspectorSerializeAttribute), true) == false)
                {
                    continue;
                }

                list.Add(new CachedMember
                {
                    Info = prop,
                    Kind = MemberKind.Property,
                    Seq = seq++,
                    IsStatic = prop.GetMethod.IsStatic,
                    ValueType = prop.PropertyType,
                    Order = GetOrder(prop),
                    Attrs = prop.GetCustomAttributes(false),
                });
            }

            foreach (var method in type.GetMethods(flags))
            {
                if (method.IsSpecialName) continue;
                if (method.GetParameters().Length > 0) continue;
                if (IsEngineInternal(method)) continue;

                var isButton = method.IsDefined(typeof(ButtonAttribute), true) ||
                               method.IsDefined(typeof(ButtonGroupAttribute), true);
                var isOnInspectorGUI = method.IsDefined(typeof(OnInspectorGUIAttribute), true);
                if (isButton == false && isOnInspectorGUI == false) continue;

                list.Add(new CachedMember
                {
                    Info = method,
                    Kind = isOnInspectorGUI ? MemberKind.MethodOnInspectorGUI : MemberKind.MethodButton,
                    Seq = seq++,
                    IsStatic = method.IsStatic,
                    Order = GetOrder(method),
                    Attrs = method.GetCustomAttributes(false),
                });
            }

            list.Sort((a, b) =>
            {
                if (a.Order != b.Order) return a.Order.CompareTo(b.Order);
                return a.Seq.CompareTo(b.Seq);
            });

            cached = list.ToArray();
            s_Cache[type] = cached;
            return cached;
        }

        private static double GetOrder(MemberInfo info)
        {
            foreach (var attr in info.GetCustomAttributes(false))
            {
                if (attr is PropertyOrderAttribute order)
                {
                    return order.Order;
                }
            }

            return 0;
        }

        /// <summary>
        /// 是否为引擎/框架内部的基类成员(EditorWindow/ScriptableObject 等)。
        /// 这类字段(如 wantsMouseMove)不应被当作业务字段绘制进窗口。
        /// </summary>
        private static bool IsEngineInternal(MemberInfo info)
        {
            if (info == null || info.DeclaringType == null) return false;
            var asm = info.DeclaringType.Assembly.GetName().Name;
            if (string.IsNullOrEmpty(asm)) return false;
            if (asm.StartsWith("UnityEngine", StringComparison.Ordinal)) return true;
            if (asm.StartsWith("UnityEditor", StringComparison.Ordinal)) return true;
            if (asm.StartsWith("mscorlib", StringComparison.Ordinal)) return true;
            return false;
        }

        // 这两个转发方法只是为了调用点读起来短一些, 实际结果由成员自己缓存(见 CachedMember.GetAttrs)。
        private static T GetAttr<T>(CachedMember m) where T : Attribute
        {
            return m.GetAttr<T>();
        }

        private static T[] GetAttrs<T>(CachedMember m) where T : Attribute
        {
            return m.GetAttrs<T>();
        }

        //-------------------------------------- 主绘制流程 --------------------------------------

        private static void RenderMembers(object target, CachedMember[] members, EditorWindow host, int depth)
        {
            if (members == null || members.Length == 0) return;

            var indent = depth > 0;
            if (indent)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUILayout.BeginVertical();
            }

            // 根节点代表"未分组"区域, 永远可见, 因此不存在"未分组成员被跳过"的情况。
            var root = GetGroupTree(target.GetType(), members);
            RenderGroupNode(root, target, host, depth, true);

            if (indent)
            {
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        //-------------------------------------- 分组树构建 --------------------------------------

        internal static GroupNode GetGroupTree(Type type, CachedMember[] members)
        {
            if (s_GroupTreeCache.TryGetValue(type, out var cached))
            {
                return cached;
            }

            var root = new GroupNode { Kind = GroupKind.None };
            foreach (var m in members)
            {
                var node = ResolveGroupNode(root, m);
                node.Members.Add(m);
            }

            SortGroupTree(root);
            s_GroupTreeCache[type] = root;
            return root;
        }

        /// <summary>按成员的(可多个)分组属性, 在分组树上定位它所属的最内层分组。</summary>
        private static GroupNode ResolveGroupNode(GroupNode root, CachedMember m)
        {
            var descriptors = new List<GroupDescriptor>();
            foreach (var attr in m.Attrs)
            {
                if (attr is PropertyGroupAttribute group)
                {
                    descriptors.Add(DescribeGroup(group));
                }
            }

            if (descriptors.Count == 0)
            {
                return root;
            }

            // 父分组先建: 按 GroupID 路径深度升序, 同深度保持属性声明顺序。
            descriptors.Sort((a, b) => PathDepth(a.GroupId).CompareTo(PathDepth(b.GroupId)));

            var current = root;
            foreach (var descriptor in descriptors)
            {
                var segments = descriptor.GroupId.Split('/');
                var path = "";
                for (var i = 0; i < segments.Length; i++)
                {
                    var segment = segments[i];
                    if (string.IsNullOrEmpty(segment)) continue;

                    path = path.Length == 0 ? segment : path + "/" + segment;
                    var isLeaf = i == segments.Length - 1;

                    var child = FindChild(current, path);
                    if (child == null)
                    {
                        child = new GroupNode
                        {
                            GroupId   = path,
                            GroupName = segment,
                            Parent    = current,
                            Kind      = GroupKind.Vertical, // 隐式中间层, 真实类型由它自己的分组属性补齐
                            Seq       = m.Seq,
                        };
                        current.Children.Add(child);
                    }

                    if (isLeaf)
                    {
                        ApplyDescriptor(child, descriptor, m.Seq);
                    }

                    current = child;
                }
            }

            return current;
        }

        private static GroupNode FindChild(GroupNode parent, string groupId)
        {
            for (var i = 0; i < parent.Children.Count; i++)
            {
                if (parent.Children[i].GroupId == groupId) return parent.Children[i];
            }

            return null;
        }

        private static int PathDepth(string groupId)
        {
            if (string.IsNullOrEmpty(groupId)) return 0;
            var depth = 1;
            for (var i = 0; i < groupId.Length; i++)
            {
                if (groupId[i] == '/') depth++;
            }

            return depth;
        }

        private static GroupDescriptor DescribeGroup(PropertyGroupAttribute group)
        {
            var id = string.IsNullOrEmpty(group.GroupID) ? "_DefaultGroup" : group.GroupID.Trim('/');
            var descriptor = new GroupDescriptor
            {
                GroupId                      = id,
                Order                        = group.Order,
                Kind                         = GroupKind.Vertical,
                ShowLabel                    = true,
                HideWhenChildrenAreInvisible = group.HideWhenChildrenAreInvisible,
                VisibleIf                    = group.VisibleIf,
            };

            switch (group)
            {
                case BoxGroupAttribute box:
                    descriptor.Kind        = GroupKind.Box;
                    descriptor.ShowLabel   = box.ShowLabel;
                    descriptor.CenterLabel = box.CenterLabel;
                    break;
                case FoldoutGroupAttribute fold:
                    descriptor.Kind     = GroupKind.Foldout;
                    descriptor.Expanded = fold.Expanded;
                    break;
                case VerticalGroupAttribute:
                    descriptor.Kind = GroupKind.Vertical;
                    break;
                case HorizontalGroupAttribute:
                    descriptor.Kind = GroupKind.Horizontal;
                    break;
                case TitleGroupAttribute title:
                    descriptor.Kind           = GroupKind.Title;
                    descriptor.HorizontalLine = title.HorizontalLine;
                    descriptor.BoldTitle      = title.BoldTitle;
                    descriptor.Alignment      = title.Alignment;
                    break;
                case ButtonGroupAttribute:
                    descriptor.Kind = GroupKind.ButtonGroup;
                    break;
                case TabGroupAttribute:
                    descriptor.Kind = GroupKind.Vertical; // 页签分组暂按垂直组降级
                    break;
            }

            return descriptor;
        }

        private static void ApplyDescriptor(GroupNode node, GroupDescriptor descriptor, int seq)
        {
            node.Kind                         = descriptor.Kind;
            node.Order                        = descriptor.Order;
            node.ShowLabel                    = descriptor.ShowLabel;
            node.CenterLabel                  = descriptor.CenterLabel;
            node.Expanded                     = descriptor.Expanded;
            node.HorizontalLine               = descriptor.HorizontalLine;
            node.BoldTitle                    = descriptor.BoldTitle;
            node.Alignment                    = descriptor.Alignment;
            node.HideWhenChildrenAreInvisible = descriptor.HideWhenChildrenAreInvisible;
            node.VisibleIf                    = descriptor.VisibleIf;
            if (seq < node.Seq) node.Seq = seq;
        }

        private static void SortGroupTree(GroupNode node)
        {
            node.Members.Sort((a, b) =>
            {
                if (a.Order != b.Order) return a.Order.CompareTo(b.Order);
                return a.Seq.CompareTo(b.Seq);
            });

            node.Children.Sort((a, b) =>
            {
                var oa = GroupSortOrder(a);
                var ob = GroupSortOrder(b);
                if (oa != ob) return oa.CompareTo(ob);
                return a.Seq.CompareTo(b.Seq);
            });

            foreach (var child in node.Children)
            {
                SortGroupTree(child);
            }
        }

        /// <summary>分组定位顺序: 显式指定 Order 时用它, 否则跟随组内最小 PropertyOrder 的位置。</summary>
        private static double GroupSortOrder(GroupNode node)
        {
            return node.Order != 0 ? node.Order : MinMemberOrder(node);
        }

        /// <summary>组内(含子孙)成员的最小 PropertyOrder。</summary>
        private static double MinMemberOrder(GroupNode node)
        {
            var min = double.MaxValue;
            foreach (var m in node.Members)
            {
                if (m.Order < min) min = m.Order;
            }

            foreach (var child in node.Children)
            {
                var childMin = MinMemberOrder(child);
                if (childMin < min) min = childMin;
            }

            return min == double.MaxValue ? 0d : min;
        }

        //-------------------------------------- 分组树绘制 --------------------------------------

        private static void RenderGroupNode(GroupNode node, object target, EditorWindow host, int depth, bool isRoot)
        {
            // 组自身的 VisibleIf 与 HideWhenChildrenAreInvisible(默认 true) 都可能导致整组不画。
            if (isRoot == false && IsGroupVisible(node, target) == false)
            {
                return;
            }

            var bodyOpened = true;
            if (isRoot == false)
            {
                bodyOpened = BeginGroup(node, target);
            }

            if (bodyOpened)
            {
                var entries = BuildEntries(node);
                for (var i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    if (entry.Group != null)
                    {
                        RenderGroupNode(entry.Group, target, host, depth + 1, false);
                        continue;
                    }

                    var m = entry.Member;
                    if (m == null) continue;
                    if (CheckVisibility(m, target) == false) continue;

                    try
                    {
                        RunStateUpdates(target, m);
                        DrawMember(target, m, host, depth);
                    }
                    catch (Exception e)
                    {
                        InspectorLog.Error($"[Inspector] 绘制成员失败 {m.RawName}\n{e}");
                    }
                }
            }

            if (isRoot == false && bodyOpened)
            {
                EndGroup(node);
            }
        }

        /// <summary>
        /// 组是否应该绘制。
        /// VisibleIf 为假时整组消失, 这一条与 HideWhenChildrenAreInvisible 无关 ——
        /// 否则一个"子项齐全但条件不满足"的组会退化成一个空框。
        /// 与 CheckVisibility 同样拆出 isPlaying 重载, 让这组规则能脱离 Unity 运行环境被测试。
        /// </summary>
        internal static bool IsGroupVisible(GroupNode node, object target)
        {
            return IsGroupVisible(node, target, Application.isPlaying);
        }

        internal static bool IsGroupVisible(GroupNode node, object target, bool isPlaying)
        {
            if (string.IsNullOrEmpty(node.VisibleIf) == false &&
                Truthy(ConditionValue(target, node.VisibleIf)) == false)
            {
                return false;
            }

            return node.HideWhenChildrenAreInvisible == false || HasVisibleContent(node, target, isPlaying);
        }

        private static bool HasVisibleContent(GroupNode node, object target, bool isPlaying)
        {
            foreach (var m in node.Members)
            {
                if (CheckVisibility(m, target, isPlaying)) return true;
            }

            foreach (var child in node.Children)
            {
                // 走 IsGroupVisible 而不是单看 HideWhenChildrenAreInvisible:
                // 子组自己的 VisibleIf 为假时它整体不画, 这里就不能算作"父组还有内容"。
                if (IsGroupVisible(child, target, isPlaying)) return true;
            }

            return false;
        }

        private static List<GroupEntry> BuildEntries(GroupNode node)
        {
            var entries = new List<GroupEntry>(node.Children.Count + node.Members.Count);
            foreach (var child in node.Children)
            {
                entries.Add(new GroupEntry
                {
                    Group = child,
                    Order = GroupSortOrder(child),
                    Seq   = child.Seq,
                });
            }

            foreach (var m in node.Members)
            {
                entries.Add(new GroupEntry
                {
                    Member = m,
                    Order  = m.Order,
                    Seq    = m.Seq,
                });
            }

            entries.Sort((a, b) =>
            {
                if (a.Order != b.Order) return a.Order.CompareTo(b.Order);
                return a.Seq.CompareTo(b.Seq);
            });
            return entries;
        }

        /// <summary>
        /// 绘制分组头部/外框。返回 true 表示已开启一个需由 EndGroup 收尾的 GUI 作用域。
        /// target 用于取该宿主对象自己的分组展开状态。
        /// </summary>
        private static bool BeginGroup(GroupNode node, object target)
        {
            var name = node.GroupName;

            switch (node.Kind)
            {
                case GroupKind.Box:
                    GUILayout.BeginVertical(GUI.skin.GetStyle("box"));
                    if (node.ShowLabel && string.IsNullOrEmpty(name) == false)
                    {
                        GUILayout.Label(name, CreateLabelStyle(node.CenterLabel));
                    }

                    return true;

                case GroupKind.Vertical:
                    GUILayout.BeginVertical();
                    return true;

                case GroupKind.Horizontal:
                case GroupKind.ButtonGroup:
                    GUILayout.BeginHorizontal();
                    return true;

                case GroupKind.Title:
                    GUILayout.BeginVertical(GUI.skin.GetStyle("box"));
                    if (string.IsNullOrEmpty(name) == false)
                    {
                        GUILayout.Label(name, node.BoldTitle ? EditorStyles.boldLabel : EditorStyles.label);
                    }

                    if (node.HorizontalLine)
                    {
                        var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 2f);
                        EditorGUI.DrawRect(rect, new Color(0.4f, 0.4f, 0.4f, 0.3f));
                    }

                    GUILayout.Space(4);
                    return true;

                case GroupKind.Foldout:
                {
                    GUILayout.BeginVertical(GUI.skin.GetStyle("box"));
                    var expanded = GetGroupExpanded(target, node);
                    expanded = GUILayout.Toggle(expanded, string.IsNullOrEmpty(name) ? node.GroupId : name,
                        EditorStyles.foldout);
                    SetGroupExpanded(target, node, expanded);
                    if (expanded == false)
                    {
                        GUILayout.EndVertical();
                        return false;
                    }

                    return true;
                }
            }

            return true;
        }

        private static void EndGroup(GroupNode node)
        {
            switch (node.Kind)
            {
                case GroupKind.Horizontal:
                case GroupKind.ButtonGroup:
                    GUILayout.EndHorizontal();
                    break;
                default:
                    GUILayout.EndVertical();
                    break;
            }
        }

        private static GUIStyle CreateLabelStyle(bool center)
        {
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = center ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft,
            };
            return style;
        }

        //-------------------------------------- 成员分发 --------------------------------------

        private static void DrawMember(object target, CachedMember m, EditorWindow host, int depth)
        {
            switch (m.Kind)
            {
                case MemberKind.MethodButton:
                    DrawButton(target, m);
                    break;
                case MemberKind.MethodOnInspectorGUI:
                    InvokeSafe(target, m.Info);
                    break;
                default:
                    DrawValueMember(target, m, host, depth);
                    break;
            }
        }

        //-------------------------------------- OnStateUpdate / 表达式 状态 --------------------------------------

        /// <summary>每帧执行成员的 OnStateUpdate 表达式(如 $property.State.Expanded = true)。</summary>
        internal static void RunStateUpdates(object target, CachedMember m, InspectorSession session = null)
        {
            var updates = GetAttrs<OnStateUpdateAttribute>(m);
            if (updates.Length == 0) return;

            var state = new InspectorPropertyState
            {
                Expanded = GetExpanded(target, m, session),
            };
            var value = GetSafe(target, m);

            foreach (var attr in updates)
            {
                if (string.IsNullOrEmpty(attr.Action)) continue;
                if (InspectorExpressionEvaluator.IsExpression(attr.Action))
                {
                    InspectorExpressionEvaluator.Evaluate(attr.Action, target, value, state);
                }
                else
                {
                    // 普通方法名支持: 直接调用
                    var method = FindMethod(target, attr.Action);
                    if (method != null) InvokeSafe(target, method);
                }
            }

            SetExpanded(target, m, state.Expanded, session);
        }

        internal static bool GetExpanded(object target, CachedMember m, InspectorSession session = null)
        {
            // 没有记录时为展开: 折叠状态只在用户显式收起后才存在
            return (session ?? Session).GetToggle(target, m.RawName, true);
        }

        internal static void SetExpanded(object target, CachedMember m, bool value, InspectorSession session = null)
        {
            (session ?? Session).SetToggle(target, m.RawName, value);
        }

        /// <summary>
        /// 读取分组展开状态; 没有记录时返回分组属性声明的默认值(见 FoldoutGroupAttribute.Expanded)。
        /// 这里必须带上宿主对象: 分组路径在同类型的所有实例上是同一个字符串,
        /// 若只按路径存取, 同窗口里的两个同类型对象会共用一次折叠。
        /// </summary>
        internal static bool GetGroupExpanded(object target, GroupNode node, InspectorSession session = null)
        {
            return (session ?? Session).GetGroupToggle(target, node.GroupId, node.Expanded);
        }

        internal static void SetGroupExpanded(object target, GroupNode node, bool value, InspectorSession session = null)
        {
            (session ?? Session).SetGroupToggle(target, node.GroupId, value);
        }

        /// <summary>条件成员求值: 支持以 @ 开头的表达式与普通成员名。</summary>
        private static object ConditionValue(object target, string memberName)
        {
            if (InspectorExpressionEvaluator.IsExpression(memberName))
            {
                return InspectorExpressionEvaluator.Evaluate(memberName, target, null, null);
            }

            return GetMemberValue(target, memberName);
        }

        private static void DrawButton(object target, CachedMember m)
        {
            var attr = GetAttr<ButtonAttribute>(m);
            var label = attr?.ButtonName;
            if (string.IsNullOrEmpty(label)) label = FriendlyName(m.RawName);

            var height = attr != null && attr.ButtonHeight > 0 ? attr.ButtonHeight : 24;

            var content = new GUIContent(label);
            var texture = attr?.Icon != SdfIconType.None ? InspectorIcons.FromSdf(attr.Icon) : null;
            if (texture != null)
            {
                content.image = texture;
            }

            if (string.IsNullOrEmpty(attr?.Tooltip) == false)
            {
                content.tooltip = attr.Tooltip;
            }

            // 只覆盖本次按钮的背景色并在结束后还原：直接写回 Color.white 会把外层作用域设的颜色一起吃掉,
            // 而按钮回调里若抛异常, 没有 finally 就会把错误颜色留在整帧的后续绘制上。
            var previousBackground = GUI.backgroundColor;
            var color = ResolveGUIColor(m);
            if (color != null) GUI.backgroundColor = color.Value;
            try
            {
                // Unity ImagePosition 没有 ImageRight，图标统一居左
                if (GUILayout.Button(content, GUI.skin.button,
                                     GUILayout.Height(height), GUILayout.MinHeight(height)))
                {
                    InvokeSafe(target, m.Info);
                }
            }
            finally
            {
                GUI.backgroundColor = previousBackground;
            }
        }

        private static void InvokeSafe(object target, MethodInfo method)
        {
            // 按钮 / [OnValueChanged] 回调可能修改数据, 因此纳入变更通知
            Mutate(target, () =>
            {
                try
                {
                    if (method.IsStatic)
                    {
                        method.Invoke(null, null);
                    }
                    else if (target != null)
                    {
                        method.Invoke(target, null);
                    }
                }
                catch (Exception e)
                {
                    InspectorLog.Error($"[Inspector] 方法执行失败 {method.Name}\n{e}");
                }
            });
        }

        private static void InvokeSafe(object target, MemberInfo info)
        {
            if (info is MethodInfo method) InvokeSafe(target, method);
        }

        //-------------------------------------- 字段 / 属性 --------------------------------------

        /// <summary>
        /// 绘制字段/属性。LabelWidth: 绘制期间覆盖 EditorGUIUtility.labelWidth。
        /// 表格单元格(inTableCell)内标签由表头承担, 因此强制 labelWidth = 0。
        /// </summary>
        private static void DrawValueMember(object target, CachedMember m, EditorWindow host, int depth,
                                             bool inTableCell = false)
        {
            var prevLabelWidth = EditorGUIUtility.labelWidth;
            if (inTableCell)
            {
                EditorGUIUtility.labelWidth = 0f;
            }
            else
            {
                var labelWidth = GetAttr<LabelWidthAttribute>(m);
                if (labelWidth != null) EditorGUIUtility.labelWidth = labelWidth.Width;
            }

            try
            {
                DrawValueMemberBody(target, m, host, depth, inTableCell);
            }
            finally
            {
                EditorGUIUtility.labelWidth = prevLabelWidth;
            }
        }

        private static void DrawValueMemberBody(object target, CachedMember m, EditorWindow host, int depth,
                                                bool inTableCell)
        {
            var type = m.ValueType;
            if (type == null) return;

            DrawTitlePrelude(target, m);

            // Required 校验: 值缺失时给出提示(表格单元格内不画, 避免撑高行)
            if (inTableCell == false) DrawRequired(target, m);

            // InlineEditor: 不做引用选择器, 直接把值的检视界面就地展开
            if (GetAttr<InlineEditorAttribute>(m) != null && IsInlineEditorTarget(type))
            {
                DrawInlineEditor(target, m, host, depth);
                return;
            }

            var isContainer = IsContainerType(type);
            if (isContainer)
            {
                DrawContainerMember(target, m, host, depth);
                return;
            }

            if (GetAttr<DisplayAsStringAttribute>(m) != null)
            {
                DrawDisplayString(target, m);
                return;
            }

            var readOnly = GetAttr<ReadOnlyAttribute>(m) != null;
            if (m.Kind == MemberKind.Property && (m.Info as PropertyInfo)?.SetMethod == null)
            {
                readOnly = true;
            }

            var disabled = readOnly || CheckDisabled(m, target);

            var value = GetSafe(target, m);
            var hideLabel = GetAttr<HideLabelAttribute>(m) != null;
            var label = hideLabel ? "" : GetLabel(m);

            object newValue = null;
            var changed = false;
            try
            {
                EditorGUI.BeginDisabledGroup(disabled);

                var prevColor = GUI.color;
                var color = ResolveGUIColor(m);
                if (color != null) GUI.color = color.Value;

                try
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();

                    newValue = DrawScalarControl(target, m, type, value, label);

                    changed = EditorGUI.EndChangeCheck();
                    DrawInlineButtons(target, m);
                }
                finally
                {
                    EditorGUILayout.EndHorizontal();
                    GUI.color = prevColor;
                    EditorGUI.EndDisabledGroup();
                }
            }
            catch (Exception e)
            {
                InspectorLog.Error($"[Inspector] 绘制成员失败 {m.RawName}\n{e}");
                return;
            }

            if (changed)
            {
                try
                {
                    Mutate(target, () => SetValue(target, m, newValue));
                    foreach (var attr in GetAttrs<OnValueChangedAttribute>(m))
                    {
                        var method = FindMethod(target, attr.MethodName);
                        if (method != null) InvokeSafe(target, method);
                    }

                    host?.Repaint();
                }
                catch (Exception e)
                {
                    InspectorLog.Error($"[Inspector] 赋值失败 {m.RawName}\n{e}");
                }
            }
        }

        /// <summary>
        /// 以只读文本形式展示成员(DisplayAsString)。
        /// </summary>
        private static void DrawDisplayString(object target, CachedMember m)
        {
            var attr = GetAttr<DisplayAsStringAttribute>(m);
            if (attr == null) return;

            var value = GetSafe(target, m);
            var text = value?.ToString() ?? "";
            if (attr.MaxCharCount > 0 && text.Length > attr.MaxCharCount)
            {
                text = text.Substring(0, attr.MaxCharCount) + "...";
            }

            var style = new GUIStyle(EditorStyles.label);
            style.wordWrap = true;
            style.richText = true;
            switch (attr.Alignment)
            {
                case TextAlign.Center: style.alignment = TextAnchor.MiddleCenter; break;
                case TextAlign.Right: style.alignment = TextAnchor.MiddleRight; break;
                default: style.alignment = TextAnchor.MiddleLeft; break;
            }

            var prevColor = GUI.color;
            var color = ResolveGUIColor(m);
            if (color != null) GUI.color = color.Value;

            var hideLabel = GetAttr<HideLabelAttribute>(m) != null;
            var label = hideLabel ? "" : GetLabel(m);
            if (string.IsNullOrEmpty(label))
            {
                GUILayout.Label(text, style);
            }
            else
            {
                GUILayout.Label($"{label}:\n{text}", style);
            }

            GUI.color = prevColor;
        }

        //-------------------------------------- Required 校验 --------------------------------------

        /// <summary>值缺失(空/默认值)时按 [Required] 的提示等级给出提示。</summary>
        private static void DrawRequired(object target, CachedMember m)
        {
            var attr = GetAttr<RequiredAttribute>(m);
            if (attr == null) return;
            if (RequiredIsValid(GetSafe(target, m), m.ValueType)) return;

            var message = string.IsNullOrEmpty(attr.ErrorMessage)
                ? $"{GetLabel(m)} 是必填项"
                : attr.ErrorMessage;
            EditorGUILayout.HelpBox(message, ToMessageType(attr.MessageType));
        }

        /// <summary>null、空字符串、空 Unity 对象、空集合、值类型的默认值都视为"未填写"。</summary>
        private static bool RequiredIsValid(object value, Type type)
        {
            if (value == null) return false;
            if (value is string s) return string.IsNullOrWhiteSpace(s) == false;
            if (value is UnityEngine.Object uo) return uo != null;
            if (value is ICollection c) return c.Count > 0;

            if (type != null && type.IsValueType)
            {
                try
                {
                    var def = Activator.CreateInstance(type);
                    if (Equals(value, def)) return false;
                }
                catch
                {
                    // 无法构造的类型不做默认值判断
                }
            }

            return true;
        }

        //-------------------------------------- 内联检视器 (InlineEditor) --------------------------------------

        /// <summary>InlineEditor 只对"能展开成子成员"的类型有意义(Unity 对象 / 用户类)。</summary>
        private static bool IsInlineEditorTarget(Type type)
        {
            if (type == null) return false;
            if (type == typeof(string) || type.IsPrimitive || type.IsEnum || type.IsArray) return false;
            return typeof(UnityEngine.Object).IsAssignableFrom(type) || IsContainerType(type);
        }

        /// <summary>
        /// InlineEditor: 不做引用选择器, 直接把值的完整检视界面就地展开。
        /// Unity 对象(MonoBehaviour/ScriptableObject)默认不会被内窥, 这里显式放开。
        /// </summary>
        private static void DrawInlineEditor(object target, CachedMember m, EditorWindow host, int depth)
        {
            var attr = GetAttr<InlineEditorAttribute>(m);
            var value = GetSafe(target, m);
            if (value == null)
            {
                DrawScalarLabelNull(m);
                return;
            }

            // [HideLabel] 时不画表头, 完全内联(DrawHeader = false)
            if (GetAttr<HideLabelAttribute>(m) == null)
            {
                var expanded = attr.Expanded || GetExpanded(target, m);
                expanded = GUILayout.Toggle(expanded, GetLabel(m), EditorStyles.foldout);
                SetExpanded(target, m, expanded);
                if (expanded == false) return;
            }

            if (value is UnityEngine.Object unityObject)
            {
                if (unityObject == null) return; // 已销毁的对象
                DrawRecursive(unityObject, host, depth + 1, target, true);
                return;
            }

            DrawRecursive(value, host, depth + 1, target);
        }

        private static bool IsContainerType(Type type)
        {
            if (type == null) return false;
            if (typeof(IList).IsAssignableFrom(type)) return true;
            if (typeof(IDictionary).IsAssignableFrom(type)) return true;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>)) return true;

            // 曲线/渐变不是"嵌套对象": 它们的数据在原生对象里, 展开出来只是一堆无意义的句柄成员,
            // 交给 DrawScalarControl 用 CurveField/GradientField 画。
            if (InspectorSerialization.IsUnityPropertyBagType(type)) return false;

            // 普通用户类/结构体按嵌套对象展开
            if (type.IsClass == false) return false;
            if (type == typeof(string)) return false;
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return false;
            return true;
        }

        //-------------------------------------- 标量控件 --------------------------------------

        private static object DrawScalarControl(object target, CachedMember m, Type type, object value, string label)
        {
            var dropdown = GetAttr<ValueDropdownAttribute>(m);
            if (dropdown != null)
            {
                return DrawValueDropdown(target, m, type, value, label, dropdown);
            }

            if (type.IsEnum)
            {
                return DrawEnum(target, m, type, value, label);
            }

            if (type == typeof(bool))
            {
                return EditorGUILayout.Toggle(label, Convert.ToBoolean(value));
            }

            if (type == typeof(string))
            {
                return DrawString(target, m, value as string, label);
            }

            // 曲线/渐变有专用控件; 值为 null 时交给 Unity 自己处理(它会代以一个空曲线/空渐变)
            if (type == typeof(AnimationCurve))
            {
                return EditorGUILayout.CurveField(label, value as AnimationCurve);
            }

            if (type == typeof(Gradient))
            {
                return EditorGUILayout.GradientField(label, value as Gradient);
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                {
                    var min = GetAttr<MinValueAttribute>(m);
                    var max = GetAttr<MaxValueAttribute>(m);
                    if (min != null && max != null && min.MemberName == null && max.MemberName == null)
                    {
                        var cur = Convert.ToSingle(value);
                        var res = EditorGUILayout.Slider(label, cur, (float)min.MinValue, (float)max.MaxValue);
                        return Convert.ChangeType(res, type);
                    }

                    var curL = Convert.ToInt64(value ?? 0);
                    var resL = EditorGUILayout.LongField(label, curL);
                    return Convert.ChangeType(resL, type);
                }

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                {
                    var min = GetAttr<MinValueAttribute>(m);
                    var max = GetAttr<MaxValueAttribute>(m);
                    if (min != null && max != null && min.MemberName == null && max.MemberName == null)
                    {
                        var cur = Convert.ToSingle(value);
                        var res = EditorGUILayout.Slider(label, cur, (float)min.MinValue, (float)max.MaxValue);
                        return Convert.ChangeType(res, type);
                    }

                    var curF = Convert.ToSingle(value ?? 0);
                    var resF = EditorGUILayout.FloatField(label, curF);
                    return Convert.ChangeType(resF, type);
                }
            }

            if (type == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(label, (Vector2)(value ?? Vector2.zero));
            }

            if (type == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(label, (Vector3)(value ?? Vector3.zero));
            }

            if (type == typeof(Vector4))
            {
                return EditorGUILayout.Vector4Field(label, (Vector4)(value ?? Vector4.zero));
            }

            if (type == typeof(Vector2Int))
            {
                return EditorGUILayout.Vector2IntField(label, (Vector2Int)(value ?? Vector2Int.zero));
            }

            if (type == typeof(Vector3Int))
            {
                return EditorGUILayout.Vector3IntField(label, (Vector3Int)(value ?? Vector3Int.zero));
            }

            // Quaternion 没有专用控件: 按欧拉角编辑, 与 Unity 默认 Inspector 的表现一致
            if (type == typeof(Quaternion))
            {
                var rotation = value is Quaternion quaternion ? quaternion : Quaternion.identity;
                return Quaternion.Euler(EditorGUILayout.Vector3Field(label, rotation.eulerAngles));
            }

            if (type == typeof(Rect))
            {
                return EditorGUILayout.RectField(label, value is Rect rect ? rect : default(Rect));
            }

            if (type == typeof(RectInt))
            {
                return EditorGUILayout.RectIntField(label, value is RectInt rectInt ? rectInt : default(RectInt));
            }

            if (type == typeof(Bounds))
            {
                return EditorGUILayout.BoundsField(label, value is Bounds bounds ? bounds : default(Bounds));
            }

            if (type == typeof(BoundsInt))
            {
                return EditorGUILayout.BoundsIntField(label,
                                                      value is BoundsInt boundsInt ? boundsInt : default(BoundsInt));
            }

            if (type == typeof(LayerMask))
            {
                var mask = value is LayerMask layerMask ? layerMask.value : 0;
                return (LayerMask)EditorGUILayout.LayerField(label, mask);
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                var obj = value as UnityEngine.Object;
                return EditorGUILayout.ObjectField(label, obj, type, true);
            }

            // 兜底: 剩下的类型(char 就是靠它 + Convert.ChangeType 编辑的)走文本框。
            // 注意 Vector4/Quaternion/Rect/Bounds/LayerMask 这类曾经也落到这里, 结果一编辑就在
            // SetValue 处抛 ArgumentException 刷一条"赋值失败"、而且改动被丢掉, 所以上面逐个补了控件。
            return EditorGUILayout.TextField(label, value?.ToString() ?? "");
        }

        private static object DrawString(object target, CachedMember m, string value, string label)
        {
            var file = GetAttr<FilePathAttribute>(m);
            var folder = GetAttr<FolderPathAttribute>(m);

            var result = EditorGUILayout.TextField(label, value ?? "");

            if (file != null || folder != null)
            {
                if (GUILayout.Button("…", GUILayout.Width(26), GUILayout.ExpandWidth(false)))
                {
                    var start = folder != null ? folder.Parent : file.ParentFolder;
                    if (string.IsNullOrEmpty(start)) start = Application.dataPath;

                    if (folder != null)
                    {
                        var dir = EditorUtility.OpenFolderPanel("选择文件夹", start, "");
                        if (string.IsNullOrEmpty(dir) == false) result = ToAssetPath(dir);
                    }
                    else
                    {
                        var ext = file.Extensions ?? "";
                        ext = ext.TrimStart('.').Replace("|", ";").Replace(",", ";");
                        if (string.IsNullOrEmpty(ext)) ext = "*";
                        var sel = EditorUtility.OpenFilePanel("选择文件", start, ext);
                        if (string.IsNullOrEmpty(sel) == false) result = ToAssetPath(sel);
                    }
                }
            }

            return result;
        }

        private static string ToAssetPath(string fullPath)
        {
            var full = fullPath.Replace('\\', '/');
            var data = Application.dataPath.Replace('\\', '/');
            if (full.StartsWith(data, StringComparison.OrdinalIgnoreCase))
            {
                return "Assets" + full.Substring(data.Length);
            }

            return full;
        }

        private static object DrawEnum(object target, CachedMember m, Type type, object value, string label)
        {
            var toggle = GetAttr<EnumToggleButtonsAttribute>(m) != null;
            var values = Enum.GetValues(type);

            if (toggle)
            {
                var contents = new List<GUIContent>();
                foreach (var v in values)
                {
                    contents.Add(new GUIContent(GetEnumLabel(type, v)));
                }

                var cur = Convert.ToInt32(value);
                var newIdx = GUILayout.Toolbar(cur, contents.ToArray());
                if (newIdx == cur) return value;
                if (newIdx < 0 || newIdx >= values.Length) return value;
                return values.GetValue(newIdx);
            }

            var options = new List<string>();
            foreach (var v in values)
            {
                options.Add(GetEnumLabel(type, v));
            }

            var currentIdx = Array.IndexOf(values, value);
            if (currentIdx < 0) currentIdx = 0;
            var selected = EditorGUILayout.Popup(label, currentIdx, options.ToArray());
            return values.GetValue(selected);
        }

        private static string GetEnumLabel(Type enumType, object value)
        {
            try
            {
                var field = enumType.GetField(value.ToString());
                if (field != null)
                {
                    foreach (var attr in field.GetCustomAttributes(false))
                    {
                        if (attr is LabelTextAttribute label && string.IsNullOrEmpty(label.Text) == false)
                        {
                            return label.Text;
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }

            return FriendlyName(value.ToString());
        }

        //-------------------------------------- 下拉框 --------------------------------------

        private static object DrawValueDropdown(object target, CachedMember m, Type type, object value,
                                                string label, ValueDropdownAttribute dropdown)
        {
            var items = ResolveDropdownItems(target, dropdown.ValuesGetter);
            if (items == null || items.Count == 0)
            {
                return EditorGUILayout.TextField(label, value?.ToString() ?? "");
            }

            var texts = new List<string>();
            var selected = -1;
            for (var i = 0; i < items.Count; i++)
            {
                texts.Add(items[i].Text);
                if (selected < 0 && ValuesEqual(items[i].Value, value))
                {
                    selected = i;
                }
            }

            var content = string.IsNullOrEmpty(label) ? GUIContent.none : new GUIContent(label);
            var idx = EditorGUILayout.Popup(content, selected, texts.ToArray());
            if (idx >= 0 && idx < items.Count && selected != idx)
            {
                return items[idx].Value;
            }

            return value;
        }

        private static List<DropdownItem> ResolveDropdownItems(object target, string getter)
        {
            if (target == null || string.IsNullOrEmpty(getter)) return null;

            object result;
            try
            {
                result = GetMemberValue(target, getter);
            }
            catch
            {
                return null;
            }

            if (result == null) return null;

            var items = new List<DropdownItem>();
            if (result is IEnumerable enumerable == false)
            {
                items.Add(new DropdownItem { Text = result.ToString(), Value = result });
                return items;
            }

            foreach (var element in enumerable)
            {
                if (element == null)
                {
                    items.Add(new DropdownItem { Text = "(null)", Value = null });
                    continue;
                }

                var type = element.GetType();
                var textProp = type.GetProperty("Text");
                var valueProp = type.GetProperty("Value");

                if (textProp != null && valueProp != null)
                {
                    items.Add(new DropdownItem
                    {
                        Text = textProp.GetValue(element, null)?.ToString() ?? "",
                        Value = valueProp.GetValue(element, null),
                    });
                }
                else
                {
                    items.Add(new DropdownItem { Text = element.ToString(), Value = element });
                }
            }

            return items;
        }

        private class DropdownItem
        {
            public string Text;
            public object Value;
        }

        //-------------------------------------- 容器: 列表 / 字典 / 嵌套 --------------------------------------

        private static void DrawContainerMember(object target, CachedMember m, EditorWindow host, int depth)
        {
            var value = GetSafe(target, m);
            if (value == null)
            {
                DrawScalarLabelNull(m);
                return;
            }

            var valueType = value.GetType();

            if (value is IDictionary)
            {
                DrawDictionary(target, m, value as IDictionary, host);
                return;
            }

            if (value is IList list)
            {
                DrawList(target, m, list, host, depth);
                return;
            }

            if (IsContainerType(valueType) && value is IEnumerable)
            {
                DrawStringSet(target, m, (IEnumerable)value);
                return;
            }

            // 嵌套用户对象
            var hideLabel = GetAttr<HideLabelAttribute>(m) != null;
            if (hideLabel == false)
            {
                GUILayout.Label(GetLabel(m), EditorStyles.boldLabel);
            }

            DrawRecursive(value, host, depth + 1, target);
        }

        private static void DrawScalarLabelNull(CachedMember m)
        {
            EditorGUILayout.LabelField(GetLabel(m), "(null)");
        }

        private static void DrawStringSet(object target, CachedMember m, IEnumerable collection)
        {
            var count = 0;
            foreach (var _ in collection) count++;

            var box = GUI.skin.GetStyle("box");
            GUILayout.BeginVertical(box);
            GUILayout.Label($"{GetLabel(m)} ({count})", EditorStyles.boldLabel);
            foreach (var item in collection)
            {
                EditorGUILayout.LabelField(item?.ToString() ?? "null");
            }

            GUILayout.EndVertical();
        }

        private static void DrawDictionary(object target, CachedMember m, IDictionary dict, EditorWindow host)
        {
            var box = GUI.skin.GetStyle("box");
            var readOnly = GetAttr<ReadOnlyAttribute>(m) != null;
            var settings = GetAttr<DictionaryDrawerSettingsAttribute>(m);
            if (settings != null && settings.IsReadOnly) readOnly = true;

            var expanded = GetExpanded(target, m);

            GUILayout.BeginVertical(box);
            expanded = GUILayout.Toggle(expanded, $"{GetLabel(m)} ({dict.Count})", EditorStyles.foldout);
            SetExpanded(target, m, expanded);

            if (expanded)
            {
                var keys = new List<object>();
                foreach (var k in dict.Keys) keys.Add(k);

                foreach (var k in keys)
                {
                    var v = dict[k];
                    EditorGUILayout.BeginHorizontal();

                    if (readOnly)
                    {
                        if (v != null && IsScalarLike(v.GetType()))
                        {
                            EditorGUILayout.TextField(k?.ToString() ?? "", v.ToString());
                        }
                        else
                        {
                            EditorGUILayout.LabelField(k?.ToString() ?? "", v?.ToString() ?? "null");
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("-", GUILayout.Width(22), GUILayout.ExpandWidth(false)))
                        {
                            Mutate(target, () => dict.Remove(k));
                            host?.Repaint();
                            EditorGUILayout.EndHorizontal();
                            break;
                        }

                        if (v != null && IsScalarLike(v.GetType()))
                        {
                            var newText = EditorGUILayout.TextField(k?.ToString() ?? "", v.ToString());
                            var parsed = ParseSimple(v.GetType(), newText);
                            if (parsed != null && Equals(parsed, v) == false)
                            {
                                Mutate(target, () => dict[k] = parsed);
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField(k?.ToString() ?? "", v?.ToString() ?? "null");
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
        }

        /// <summary>列表绘制入口: 带 [TableList] 且元素类型有可表格化成员的走表格, 否则走普通列表。</summary>
        private static void DrawList(object target, CachedMember m, IList list, EditorWindow host, int depth)
        {
            var table = GetAttr<TableListAttribute>(m);
            if (table != null)
            {
                var elementType = GetListElementType(m.ValueType);
                var tableColumns = elementType != null ? GetTableColumns(elementType) : null;
                if (tableColumns != null && tableColumns.Count > 0)
                {
                    DrawTableList(target, m, list, host, depth, table, tableColumns);
                    return;
                }
            }

            DrawPlainList(target, m, list, host, depth);
        }

        private static void DrawPlainList(object target, CachedMember m, IList list, EditorWindow host, int depth)
        {
            var box = GUI.skin.GetStyle("box");

            // Unity 对象列表: 只读列表展示
            if (list.Count > 0 && list[0] is UnityEngine.Object)
            {
                GUILayout.BeginVertical(box);
                GUILayout.Label($"{GetLabel(m)} ({list.Count})", EditorStyles.boldLabel);
                foreach (var item in list)
                {
                    if (item is UnityEngine.Object uo)
                    {
                        EditorGUILayout.ObjectField(uo.name, uo, uo.GetType(), true);
                    }
                    else
                    {
                        EditorGUILayout.LabelField(item?.ToString() ?? "null");
                    }
                }

                GUILayout.EndVertical();
                return;
            }

            var listSettings = GetAttr<ListDrawerSettingsAttribute>(m);
            var readOnly = GetAttr<ReadOnlyAttribute>(m) != null;
            if (listSettings != null && listSettings.IsReadOnly) readOnly = true;

            var expanded = GetExpanded(target, m);

            GUILayout.BeginVertical(box);

            EditorGUILayout.BeginHorizontal();
            expanded = GUILayout.Toggle(expanded, $"{GetLabel(m)} ({list.Count})", EditorStyles.foldout);
            SetExpanded(target, m, expanded);
            GUILayout.FlexibleSpace();

            if (readOnly == false && GUILayout.Button("+", GUILayout.Width(24)))
            {
                AddListElement(target, m, list, listSettings);
                host?.Repaint();
            }

            EditorGUILayout.EndHorizontal();

            if (expanded)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var element = list[i];
                    EditorGUILayout.BeginVertical();

                    if (element == null)
                    {
                        EditorGUILayout.LabelField($"#{i}", "null");
                    }
                    else if (IsScalarLike(element.GetType()))
                    {
                        EditorGUILayout.BeginHorizontal();
                        var newText = EditorGUILayout.TextField($"#{i}", element.ToString());
                        if (readOnly == false && GUILayout.Button("-", GUILayout.Width(22)))
                        {
                            Mutate(target, () => list.RemoveAt(i));
                            host?.Repaint();
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            break;
                        }

                        EditorGUILayout.EndHorizontal();

                        if (readOnly == false)
                        {
                            var parsed = ParseSimple(element.GetType(), newText);
                            if (parsed != null && Equals(parsed, element) == false)
                            {
                                Mutate(target, () => list[i] = parsed);
                            }
                        }
                    }
                    else
                    {
                        if (readOnly == false && GUILayout.Button("-", GUILayout.Width(22), GUILayout.Height(16)))
                        {
                            Mutate(target, () => list.RemoveAt(i));
                            host?.Repaint();
                            EditorGUILayout.EndVertical();
                            break;
                        }

                        DrawRecursive(element, host, depth + 1, target);
                    }

                    EditorGUILayout.EndVertical();
                    GUILayout.Space(2);
                }
            }

            GUILayout.EndVertical();
        }

        //-------------------------------------- 表格 (TableList) --------------------------------------

        private const int DefaultTableColumnWidth = 100;
        private const int TableIndexColumnWidth = 34;
        private const int TableRemoveColumnWidth = 24;

        /// <summary>表格的一列, 来自元素类型的一个成员。</summary>
        private sealed class TableColumn
        {
            public CachedMember Member;
            public string GroupKey = "";   // 顶层分组 ID(用于合并表头)
            public string GroupName = "";
            public string Label = "";
            public int Width = DefaultTableColumnWidth;
        }

        /// <summary>
        /// 表格绘制。列来自元素类型自身的成员(已按 PropertyOrder 排序), 列宽取成员的 [TableColumnWidth];
        /// 表头按顶层分组属性合并成一条带(分组表头), 下面再画一行列名。
        /// </summary>
        private static void DrawTableList(object target, CachedMember m, IList list, EditorWindow host, int depth,
                                          TableListAttribute table, List<TableColumn> columns)
        {
            var readOnly = table.IsReadOnly || GetAttr<ReadOnlyAttribute>(m) != null;
            var showIndex = table.ShowIndexLabels;
            var showRemove = readOnly == false && table.HideToolbar == false;
            var expanded = table.AlwaysExpanded || GetExpanded(target, m);
            var label = GetLabel(m);

            GUILayout.BeginVertical(GUI.skin.GetStyle("box"));

            if (table.AlwaysExpanded)
            {
                if (GetAttr<HideLabelAttribute>(m) == null && string.IsNullOrEmpty(label) == false)
                {
                    GUILayout.Label($"{label} ({list.Count})", EditorStyles.boldLabel);
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                expanded = GUILayout.Toggle(expanded, $"{label} ({list.Count})", EditorStyles.foldout);
                SetExpanded(target, m, expanded);
                GUILayout.FlexibleSpace();
                if (showRemove && GUILayout.Button("+", GUILayout.Width(TableRemoveColumnWidth)))
                {
                    AddListElement(target, m, list, GetAttr<ListDrawerSettingsAttribute>(m));
                    host?.Repaint();
                }

                EditorGUILayout.EndHorizontal();
            }

            if (expanded)
            {
                var scroll = Session.GetScroll(target, m.RawName);

                if (table.DrawScrollView)
                {
                    // 高度有界, 避免与外层窗口滚动视图互相抢空间
                    var rows = Mathf.Clamp(list.Count, 1, Mathf.Max(1, table.NumberOfItemsPerPage));
                    var maxHeight = EditorGUIUtility.singleLineHeight * (rows + 2) + 14f;
                    scroll = GUILayout.BeginScrollView(scroll,
                        GUILayout.MaxHeight(maxHeight), GUILayout.ExpandHeight(false));
                    Session.SetScroll(target, m.RawName, scroll);
                }

                DrawTableHeader(columns, showIndex, showRemove);

                EditorGUI.BeginDisabledGroup(readOnly);
                try
                {
                    DrawTableRows(target, m, list, columns, host, depth, showIndex, showRemove);
                }
                finally
                {
                    EditorGUI.EndDisabledGroup();
                }

                if (table.DrawScrollView)
                {
                    GUILayout.EndScrollView();
                }
            }

            GUILayout.EndVertical();
        }

        private static void DrawTableHeader(List<TableColumn> columns, bool showIndex, bool showRemove)
        {
            var grouped = false;
            foreach (var column in columns)
            {
                if (string.IsNullOrEmpty(column.GroupKey) == false)
                {
                    grouped = true;
                    break;
                }
            }

            if (grouped)
            {
                EditorGUILayout.BeginHorizontal();
                if (showIndex) GUILayout.Space(TableIndexColumnWidth);

                var i = 0;
                while (i < columns.Count)
                {
                    var key = columns[i].GroupKey;
                    var width = 0;
                    var j = i;
                    while (j < columns.Count && columns[j].GroupKey == key)
                    {
                        width += columns[j].Width;
                        j++;
                    }

                    if (string.IsNullOrEmpty(key))
                    {
                        GUILayout.Space(width);
                    }
                    else
                    {
                        GUILayout.Label(columns[i].GroupName, CreateLabelStyle(true), GUILayout.Width(width));
                    }

                    i = j;
                }

                if (showRemove) GUILayout.Space(TableRemoveColumnWidth);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            if (showIndex) GUILayout.Label("#", EditorStyles.miniBoldLabel, GUILayout.Width(TableIndexColumnWidth));
            foreach (var column in columns)
            {
                GUILayout.Label(column.Label, EditorStyles.miniBoldLabel, GUILayout.Width(column.Width));
            }

            if (showRemove) GUILayout.Space(TableRemoveColumnWidth);
            EditorGUILayout.EndHorizontal();

            DrawHorizontalLine();
        }

        private static void DrawTableRows(object target, CachedMember m, IList list, List<TableColumn> columns,
                                          EditorWindow host, int depth, bool showIndex, bool showRemove)
        {
            if (list.Count == 0)
            {
                EditorGUILayout.LabelField("(空)", EditorStyles.miniLabel);
                return;
            }

            for (var i = 0; i < list.Count; i++)
            {
                var element = list[i];

                EditorGUILayout.BeginHorizontal();
                if (showIndex)
                {
                    GUILayout.Label($"#{i}", EditorStyles.miniLabel, GUILayout.Width(TableIndexColumnWidth));
                }

                if (element == null)
                {
                    GUILayout.Label("null", EditorStyles.miniLabel);
                }
                else
                {
                    foreach (var column in columns)
                    {
                        DrawTableCell(element, column, host, depth);
                    }
                }

                var removed = false;
                if (showRemove &&
                    GUILayout.Button("-", GUILayout.Width(TableRemoveColumnWidth), GUILayout.ExpandWidth(false)))
                {
                    Mutate(target, () => list.RemoveAt(i));
                    host?.Repaint();
                    removed = true;
                }

                EditorGUILayout.EndHorizontal();
                DrawHorizontalLine();

                if (removed) break;
            }
        }

        private static void DrawTableCell(object element, TableColumn column, EditorWindow host, int depth)
        {
            GUILayout.BeginVertical(GUILayout.Width(column.Width));

            // 条件不满足的成员保留列位, 单元格留空
            if (CheckVisibility(column.Member, element))
            {
                if (column.Member.Kind == MemberKind.MethodButton)
                {
                    DrawMember(element, column.Member, host, depth);
                }
                else
                {
                    DrawValueMember(element, column.Member, host, depth, true);
                }
            }
            else
            {
                GUILayout.Label(GUIContent.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }

            GUILayout.EndVertical();
        }

        private static void DrawHorizontalLine()
        {
            var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 1f);
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 0.35f));
            }
        }

        private static List<TableColumn> GetTableColumns(Type elementType)
        {
            if (s_TableColumnCache.TryGetValue(elementType, out var cached))
            {
                return cached;
            }

            var columns = new List<TableColumn>();
            foreach (var member in GetMembers(elementType))
            {
                var width = GetAttr<TableColumnWidthAttribute>(member);
                var group = GetOuterGroup(member);

                columns.Add(new TableColumn
                {
                    Member    = member,
                    GroupKey  = group,
                    GroupName = group,
                    Label     = GetLabel(member),
                    Width     = width != null && width.Width > 0 ? width.Width : DefaultTableColumnWidth,
                });
            }

            s_TableColumnCache[elementType] = columns;
            return columns;
        }

        /// <summary>取最外层的分组 ID(路径深度最小), 作为表头合并带的依据。</summary>
        private static string GetOuterGroup(CachedMember m)
        {
            var best = "";
            var bestDepth = int.MaxValue;
            foreach (var attr in m.Attrs)
            {
                if (attr is PropertyGroupAttribute group == false) continue;

                var id = string.IsNullOrEmpty(group.GroupID) ? "" : group.GroupID.Trim('/');
                if (string.IsNullOrEmpty(id)) continue;

                var depth = PathDepth(id);
                if (depth < bestDepth)
                {
                    bestDepth = depth;
                    var index = id.IndexOf('/');
                    best = index < 0 ? id : id.Substring(0, index);
                }
            }

            return best;
        }

        private static bool IsScalarLike(Type type)
        {
            if (type == null) return false;
            if (type.IsEnum || type.IsPrimitive) return true;
            if (type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime)) return true;
            return type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Color) ||
                   typeof(UnityEngine.Object).IsAssignableFrom(type);
        }

        private static void AddListElement(object target, CachedMember m, IList list, ListDrawerSettingsAttribute settings)
        {
            if (settings != null && string.IsNullOrEmpty(settings.CustomAddFunction) == false)
            {
                var method = FindMethod(target, settings.CustomAddFunction);
                if (method != null)
                {
                    InvokeSafe(target, method);
                    return;
                }
            }

            var elemType = GetListElementType(m.ValueType);
            if (elemType == null) return;
            try
            {
                var instance = Activator.CreateInstance(elemType);
                Mutate(target, () => list.Add(instance));
            }
            catch (Exception e)
            {
                InspectorLog.Warn($"[Inspector] 无法添加列表元素 {elemType.Name}\n{e.Message}");
            }
        }

        private static Type GetListElementType(Type type)
        {
            if (type.IsArray) return type.GetElementType();
            if (type.IsGenericType) return type.GetGenericArguments()[0];
            return null;
        }

        private static object ParseSimple(Type type, string text)
        {
            try
            {
                if (type.IsEnum) return Enum.Parse(type, text, true);
                if (type == typeof(string)) return text;
                if (type == typeof(bool)) return bool.Parse(text);
                if (type == typeof(float)) return float.Parse(text, CultureInfo.InvariantCulture);
                if (type == typeof(double)) return double.Parse(text, CultureInfo.InvariantCulture);
                if (type == typeof(int)) return int.Parse(text);
                if (type == typeof(long)) return long.Parse(text);
                if (type == typeof(Vector2) || type == typeof(Vector3)) return null;
                return Convert.ChangeType(text, type);
            }
            catch
            {
                return null;
            }
        }

        //-------------------------------------- 标题 / 提示 --------------------------------------

        private static void DrawTitlePrelude(object target, CachedMember m)
        {
            foreach (var attr in m.Attrs)
            {
                switch (attr)
                {
                    case TitleAttribute title:
                        if (string.IsNullOrEmpty(title.Title) == false)
                        {
                            GUILayout.Label(title.Title, title.Bold ? EditorStyles.boldLabel : EditorStyles.label);
                            if (string.IsNullOrEmpty(title.Subtitle) == false)
                            {
                                GUILayout.Label(title.Subtitle, EditorStyles.miniLabel);
                            }

                            if (title.HorizontalLine)
                            {
                                var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 2f);
                                EditorGUI.DrawRect(rect, new Color(0.4f, 0.4f, 0.4f, 0.3f));
                            }

                            GUILayout.Space(4);
                        }

                        break;

                    case InfoBoxAttribute box:
                        if (IsAnnotationVisible(box.VisibleIfMemberName, target) == false) break;
                        EditorGUILayout.HelpBox(box.Message, ToMessageType(box.InfoMessageType));
                        break;

                    case DetailedInfoBoxAttribute detail:
                        if (IsAnnotationVisible(detail.VisibleIfMemberName, target) == false) break;
                        EditorGUILayout.HelpBox(detail.Message + "\n" + detail.Details, ToMessageType(detail.InfoMessageType));
                        break;
                }
            }
        }

        /// <summary>
        /// 标题/提示类批注的条件显隐。
        /// visibleIfMemberName 为空 = 无条件显示; 支持 @ 表达式与裸成员名(同 [ShowIf])。
        /// 与 CheckVisibility 一样是纯规则, 可脱离 Unity 运行环境被测试。
        /// </summary>
        internal static bool IsAnnotationVisible(string visibleIfMemberName, object target)
        {
            if (string.IsNullOrEmpty(visibleIfMemberName)) return true;
            return Truthy(ConditionValue(target, visibleIfMemberName));
        }

        private static MessageType ToMessageType(InfoMessageType type)
        {
            switch (type)
            {
                case InfoMessageType.Info:
                    return MessageType.Info;
                case InfoMessageType.Warning:
                    return MessageType.Warning;
                case InfoMessageType.Error:
                    return MessageType.Error;
                default:
                    return MessageType.None;
            }
        }

        //-------------------------------------- 可见性 / 禁用 --------------------------------------

        private static bool CheckVisibility(CachedMember m, object target)
        {
            return CheckVisibility(m, target, Application.isPlaying);
        }

        /// <summary>
        /// 可见性判定。isPlaying 由调用方传入而非直接读 Application.isPlaying,
        /// 是为了让这组纯规则能脱离 Unity 运行环境被测试。
        /// </summary>
        internal static bool CheckVisibility(CachedMember m, object target, bool isPlaying)
        {
            // HideInPlayMode / HideInEditorMode 按当前是否运行中隐藏
            if (isPlaying)
            {
                if (GetAttr<HideInPlayModeAttribute>(m) != null) return false;
            }
            else if (GetAttr<HideInEditorModeAttribute>(m) != null)
            {
                return false;
            }

            foreach (var attr in GetAttrs<ShowIfAttribute>(m))
            {
                var expect = attr.Value;
                var actual = ConditionValue(target, attr.MemberName);
                if (expect != null)
                {
                    if (ValuesEqual(actual, expect) == false) return false;
                }
                else if (Truthy(actual) == false)
                {
                    return false;
                }
            }

            foreach (var attr in GetAttrs<HideIfAttribute>(m))
            {
                var expect = attr.Value;
                var actual = ConditionValue(target, attr.MemberName);
                if (expect != null)
                {
                    if (ValuesEqual(actual, expect)) return false;
                }
                else if (Truthy(actual))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool CheckDisabled(CachedMember m, object target)
        {
            foreach (var attr in GetAttrs<DisableIfAttribute>(m))
            {
                var actual = ConditionValue(target, attr.MemberName);
                var disabled = attr.Value != null ? ValuesEqual(actual, attr.Value) : Truthy(actual);
                if (disabled) return true;
            }

            var enableIf = GetAttrs<EnableIfAttribute>(m);
            if (enableIf.Length > 0)
            {
                foreach (var attr in enableIf)
                {
                    var actual = ConditionValue(target, attr.MemberName);
                    var ok = attr.Value != null ? ValuesEqual(actual, attr.Value) : Truthy(actual);
                    if (ok == false) return true;
                }
            }

            return false;
        }

        // 真值/相等语义与表达式求值器共用一套实现(InspectorValue), 否则同一个值在
        // [ShowIf("X", v)] 与 @X == v 两处会得到不同结论。
        private static bool Truthy(object value)
        {
            return InspectorValue.Truthy(value);
        }

        private static bool ValuesEqual(object a, object b)
        {
            return InspectorValue.Equal(a, b);
        }

        /// <summary>
        /// 按名字取成员值(条件显隐里不写 @ 的裸成员名走这条)。
        /// 顺序: 无参方法 > 字段/属性(取最派生的那个); 名字解析不到时按"视为可见"返回 true。
        /// 解析结果是有缓存的 —— 条件每帧都要重算, 但"这个名字对应哪个成员"是恒定的。
        /// </summary>
        private static object GetMemberValue(object target, string memberName)
        {
            if (target == null || string.IsNullOrEmpty(memberName)) return null;

            var method = FindMethod(target, memberName);
            if (method != null)
            {
                try
                {
                    return method.IsStatic ? method.Invoke(null, null) : method.Invoke(target, null);
                }
                catch
                {
                    return false;
                }
            }

            var member = ResolveFieldOrProperty(target.GetType(), memberName, false);
            if (member == null)
            {
                // 名字解析不到的既有语义是"视为可见": 宁可不隐藏, 也不要因为一个拼错的名字把成员整片吞掉。
                // 但"宁可不隐藏"不该连线索都不留: 拼错的名字(条件里的成员名、@ 表达式里的裸名字)报一次,
                // 否则成员一直显示, 谁也想不到是名字写错了。
                InspectorLog.WarnOnce($"member-unresolved:{target.GetType().FullName}.{memberName}",
                                      $"[Inspector] 条件成员名解析不到: {target.GetType().Name}.{memberName}, " +
                                      "已按“视为可见”处理 —— 请检查成员名是否写错。");
                return true;
            }

            try
            {
                return ReadMemberValue(member, target);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>按成员元数据读值; 静态成员忽略 target, 只写属性(无 getter)返回 null。</summary>
        internal static object ReadMemberValue(MemberInfo member, object target)
        {
            switch (member)
            {
                case FieldInfo field:
                    return field.IsStatic ? field.GetValue(null) : field.GetValue(target);
                case PropertyInfo prop:
                    if (prop.GetMethod == null) return null;
                    return prop.GetMethod.IsStatic ? prop.GetValue(null, null) : prop.GetValue(target, null);
                default:
                    return null;
            }
        }

        /// <summary>
        /// 按名字解析字段或属性(取最派生的那个), 结果按 (类型, 名字, 是否只认静态) 缓存。
        /// 逐层向基类走而不是一次 GetMember/GetProperty: 一次查在"同名方法重载"时会混进方法节点,
        /// 在"派生类用 new 隐藏基类同名成员"时返回的是成员数组(顺序未定义);
        /// 逐层走把"只认字段/属性, 且取最派生的那个"变成确定规则。
        /// internal: 表达式求值器共用同一套解析规则与缓存 —— 它每帧都要重走 @ 表达式里的成员链。
        /// </summary>
        internal static MemberInfo ResolveFieldOrProperty(Type type, string name, bool staticOnly)
        {
            if (type == null || string.IsNullOrEmpty(name)) return null;

            var key = (type, name, staticOnly);
            if (s_FieldPropertyCache.TryGetValue(key, out var cached)) return cached;

            var flags = MemberLookupFlags |
                        (staticOnly ? BindingFlags.Static : BindingFlags.Instance | BindingFlags.Static);

            MemberInfo member = null;
            for (var current = type; current != null && member == null; current = current.BaseType)
            {
                member = (MemberInfo)current.GetField(name, flags) ?? current.GetProperty(name, flags);
            }

            s_FieldPropertyCache[key] = member;
            return member;
        }

        //-------------------------------------- 读写辅助 --------------------------------------

        private static object GetSafe(object target, CachedMember m)
        {
            try
            {
                if (m.Kind == MemberKind.Field)
                {
                    return ((FieldInfo)m.Info).GetValue(m.IsStatic ? null : target);
                }

                var prop = (PropertyInfo)m.Info;
                return prop.GetValue(prop.GetMethod.IsStatic ? null : target, null);
            }
            catch (Exception e)
            {
                InspectorLog.Warn($"[Inspector] 读取失败 {m.RawName}\n{e.Message}");
                return null;
            }
        }

        private static void SetValue(object target, CachedMember m, object value)
        {
            if (m.Kind == MemberKind.Field)
            {
                var field = (FieldInfo)m.Info;
                field.SetValue(m.IsStatic ? null : target, ConvertValue(value, field.FieldType));
            }
            else
            {
                var prop = (PropertyInfo)m.Info;
                if (prop.SetMethod == null) return;
                prop.SetValue(prop.SetMethod.IsStatic ? null : target, ConvertValue(value, prop.PropertyType), null);
            }
        }

        private static object ConvertValue(object value, Type targetType)
        {
            if (value == null) return null;
            if (targetType.IsInstanceOfType(value)) return value;

            if (targetType.IsEnum)
            {
                try
                {
                    return Enum.ToObject(targetType, value);
                }
                catch
                {
                    return value;
                }
            }

            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return value;
            }
        }

        /// <summary>
        /// 按名字与形参个数找方法, 结果按 (类型, 名字, 形参个数, 是否只认静态) 缓存。
        /// 不能用 Type.GetMethod(name, flags): 同名重载会抛 AmbiguousMatchException,
        /// 而按钮 / 回调 / 表达式调用本来就要求形参个数确定, 显式只认这一个版本反而更确定。
        /// 逐层向声明它的类型找, 顺带省掉 GetMethods 一次扫全表(它会分配整个方法数组)。
        /// internal: 表达式求值器共用同一套解析规则与缓存。
        /// </summary>
        internal static MethodInfo FindMethod(Type type, string name, int argCount, bool staticOnly)
        {
            if (type == null || string.IsNullOrEmpty(name)) return null;

            var key = (type, name, argCount, staticOnly);
            if (s_MethodCache.TryGetValue(key, out var cached)) return cached;

            var flags = MemberLookupFlags |
                        (staticOnly ? BindingFlags.Static : BindingFlags.Instance | BindingFlags.Static);

            MethodInfo method = null;
            for (var current = type; current != null && method == null; current = current.BaseType)
            {
                foreach (var candidate in current.GetMethods(flags))
                {
                    if (candidate.Name != name || candidate.IsSpecialName) continue;
                    if (candidate.GetParameters().Length != argCount) continue;
                    method = candidate;
                    break;
                }
            }

            s_MethodCache[key] = method;
            return method;
        }

        /// <summary>引擎侧入口: 按钮 / 回调 / CustomAddFunction 一律按无参方法调用。</summary>
        internal static MethodInfo FindMethod(object target, string name)
        {
            return target == null ? null : FindMethod(target.GetType(), name, 0, false);
        }

        //-------------------------------------- 标签 / 状态 / 颜色 --------------------------------------

        private static string GetLabel(CachedMember m)
        {
            var label = GetAttr<LabelTextAttribute>(m);
            if (label != null && string.IsNullOrEmpty(label.Text) == false)
            {
                return label.Text;
            }

            return FriendlyName(m.RawName);
        }

        private static string FriendlyName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return Regex.Replace(name, "(?<!^)([A-Z][a-z0-9])", " $1").Trim();
        }

        private static Color? ResolveGUIColor(CachedMember m)
        {
            var attr = GetAttr<GUIColorAttribute>(m);
            if (attr == null) return null;

            if (string.IsNullOrEmpty(attr.ColorString) == false)
            {
                var parts = attr.ColorString.Split(',');
                if (parts.Length >= 3 &&
                    float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var r) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var g) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var b))
                {
                    var a = 1f;
                    if (parts.Length >= 4)
                    {
                        float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out a);
                    }

                    return new Color(r, g, b, a);
                }

                return null;
            }

            return new Color(attr.R, attr.G, attr.B, attr.A);
        }

        //-------------------------------------- 行内按钮 --------------------------------------

        private static void DrawInlineButtons(object target, CachedMember m)
        {
            var buttons = GetAttrs<InlineButtonAttribute>(m);
            if (buttons.Length == 0) return;

            foreach (var attr in buttons)
            {
                var text = attr.ButtonLabel;
                if (string.IsNullOrEmpty(text)) text = attr.MemberMethod;

                if (GUILayout.Button(text, GUILayout.ExpandWidth(false)))
                {
                    var method = FindMethod(target, attr.MemberMethod);
                    if (method != null)
                    {
                        InvokeSafe(target, method);
                    }
                }
            }
        }
    }
}
