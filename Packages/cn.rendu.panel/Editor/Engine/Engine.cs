using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace RD.Panel.Editor
{
    /// <summary>
    /// Inspector 属性渲染引擎
    /// 依据反射 + Runtime 层特性即时绘制任意 C# 对象
    /// </summary>
    public static class Engine
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
        private static readonly HostSession s_DefaultSession = new();

        /// <summary>本次绘制期间生效的会话, 由 DrawObject 在进入/退出时挂载与还原。</summary>
        private static HostSession s_Session;

        /// <summary>当前生效的会话: 折叠状态与滚动位置都存在这里, 不再使用进程级静态字典。</summary>
        internal static HostSession Session => s_Session ?? s_DefaultSession;

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
            DrawObject(target, host, onBeforeChange, onChanged, (HostState)null);
        }

        /// <summary>
        /// 绘制任意对象, 并显式指定宿主状态句柄(由 <see cref="CreateHostState"/> 创建)。
        /// 每个宿主各持一份句柄, 折叠 / 滚动状态互不串台; 传 null 时走默认会话。
        /// 宿主可以位于其它程序集(例如独立的窗口包) —— HostState 不透明, 内部实现不外泄。
        /// </summary>
        public static void DrawObject(object target, EditorWindow host,
                                      Action<object> onBeforeChange,
                                      Action<object> onChanged,
                                      HostState state)
        {
            DrawObject(target, host, onBeforeChange, onChanged, state?.Session);
        }

        /// <summary>
        /// 绘制任意对象, 并显式指定本次绘制使用的后端(<see cref="ImGuiDriver"/> / <see cref="UitkDriver"/> /
        /// 自己的实现)。不传时按 <see cref="Drawing.DefaultFactory"/> 的默认后端 —— 想让某一个窗口单独改用
        /// UI Toolkit, 就在自己的 OnGUI 里传一个新的 <see cref="UitkDriver"/>。
        /// </summary>
        public static void DrawObject(object target, EditorWindow host,
                                      Action<object> onBeforeChange,
                                      Action<object> onChanged,
                                      IPanelDriver driver)
        {
            DrawObject(target, host, onBeforeChange, onChanged, (HostState)null, driver);
        }

        /// <summary>绘制任意对象: 显式指定状态句柄 + 绘制后端。</summary>
        public static void DrawObject(object target, EditorWindow host,
                                      Action<object> onBeforeChange,
                                      Action<object> onChanged,
                                      HostState state,
                                      IPanelDriver driver)
        {
            DrawObject(target, host, onBeforeChange, onChanged, state?.Session, driver);
        }

        /// <summary>创建一份宿主状态句柄; 由宿主自己持有, 生命周期与宿主一致。</summary>
        public static HostState CreateHostState()
        {
            return new HostState(new HostSession());
        }

        /// <summary>
        /// 是否 Unity"属性包"类型(AnimationCurve / Gradient): 数据在原生对象里,
        /// 序列化按整体读写, 绘制走 CurveField/GradientField 而不是展开成员。
        /// 引擎的容器判定与宿主(cn.rendu.inspector)的序列化规则共用这一条判定。
        /// </summary>
        public static bool IsUnityPropertyBagType(Type type)
        {
            return type == typeof(AnimationCurve) || type == typeof(Gradient);
        }

        /// <summary>
        /// 绘制任意对象, 并显式指定本次绘制使用的状态会话。
        /// 内部实现重载: HostSession 是内部类型, 程序集外的宿主请走 HostState 句柄。
        /// </summary>
        internal static void DrawObject(object target, EditorWindow host,
                                        Action<object> onBeforeChange,
                                        Action<object> onChanged,
                                        HostSession session)
        {
            DrawObject(target, host, onBeforeChange, onChanged, session, null);
        }

        /// <summary>
        /// 绘制任意对象: 显式指定状态会话 + 绘制后端。driver 为空时按 <see cref="Drawing"/> 的默认后端创建。
        /// </summary>
        internal static void DrawObject(object target, EditorWindow host,
                                        Action<object> onBeforeChange,
                                        Action<object> onChanged,
                                        HostSession session,
                                        IPanelDriver driver)
        {
            if (target == null) return;
            // 已销毁的 Unity 对象(fake null)不再绘制
            if (target is UnityEngine.Object unityTarget && unityTarget == null) return;

            var previousBefore  = s_BeforeChange;
            var previousChanged = s_Changed;
            var previousSession = s_Session;
            s_BeforeChange = onBeforeChange;
            s_Changed      = onChanged;
            s_Session      = session ?? s_DefaultSession;

            var active         = driver ?? Drawing.CreateDriver(host);
            var previousDriver = Drawing.Attach(active);
            try
            {
                active.BeginPanel();
                // 根对象始终允许绘制: EditorWindow 自绘, 或 Editor 检视组件
                DrawRecursive(target, host, 0, null, true);
            }
            catch (Exception e)
            {
                Log.Error($"[Inspector] 绘制失败 {target.GetType().Name}\n{e}");
            }
            finally
            {
                active.EndPanel();
                Drawing.Attach(previousDriver);
                s_BeforeChange = previousBefore;
                s_Changed      = previousChanged;
                s_Session      = previousSession;
            }
        }

        /// <summary>本次绘制生效的后端。所有画的地方都经由它, 不直接碰任何本地 UI API。</summary>
        private static IPanelDriver Drv => Drawing.Current;

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

                // [Button("名字", 排序号)]: 按钮走同一条排序通道, 没写号时按声明顺序
                if (attr is ButtonAttribute button)
                {
                    return button.Order;
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
                Drv.BeginIndent();
            }

            // 根节点代表"未分组"区域, 永远可见, 因此不存在"未分组成员被跳过"的情况。
            var root = GetGroupTree(target.GetType(), members);
            RenderGroupNode(root, target, host, depth, true);

            if (indent)
            {
                Drv.EndIndent();
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
                        Log.Error($"[Inspector] 绘制成员失败 {m.RawName}\n{e}");
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
            return IsGroupVisible(node, target, IsPlaying);
        }

        /// <summary>
        /// 运行态来源。离线校验(纯 .NET)里 Unity 原生调用会抛 SecurityException(方法 JIT 期就炸),
        /// 所以原生调用隔离在独立方法里: 测试/录制驱动通过 s_IsPlayingOverride 注入, 编辑器内不受影响(恒为 null)。
        /// </summary>
        internal static bool? s_IsPlayingOverride;

        private static bool IsPlaying => s_IsPlayingOverride ?? UnityIsPlaying();

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static bool UnityIsPlaying() => Application.isPlaying;

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
                    Drv.BeginVertical(true);
                    if (node.ShowLabel && string.IsNullOrEmpty(name) == false)
                    {
                        var align = node.CenterLabel ? PanelAlign.Center : PanelAlign.Left;
                        Drv.Label(name, PanelTextSpec.Of(align, bold: true));
                    }

                    return true;

                case GroupKind.Vertical:
                    Drv.BeginVertical(false);
                    return true;

                case GroupKind.Horizontal:
                case GroupKind.ButtonGroup:
                    Drv.BeginHorizontal();
                    return true;

                case GroupKind.Title:
                    Drv.BeginVertical(true);
                    if (string.IsNullOrEmpty(name) == false)
                    {
                        // 分组没有副标题, Split 退化为 Left(TitleAnchor 对 Split 也返回 Left)
                        Drv.Label(name, PanelTextSpec.Of(TitleAnchor(node.Alignment), node.BoldTitle));
                    }

                    if (node.HorizontalLine)
                    {
                        Drv.Separator(TitleGroupLineThickness, TitleGroupLineColor);
                    }

                    Drv.Space(4);
                    return true;

                case GroupKind.Foldout:
                {
                    Drv.BeginVertical(true);
                    var expanded = GetGroupExpanded(target, node);
                    expanded = Drv.Foldout(expanded, string.IsNullOrEmpty(name) ? node.GroupId : name);
                    // 保留模式(UI Toolkit)不会自动重画这一帧: 折叠状态一变就得主动请求, 否则子树不增不减
                    if (expanded != GetGroupExpanded(target, node)) Drv.RequestRepaint();
                    SetGroupExpanded(target, node, expanded);
                    if (expanded == false)
                    {
                        Drv.EndVertical();
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
                    Drv.EndHorizontal();
                    break;
                default:
                    Drv.EndVertical();
                    break;
            }
        }

        /// <summary>分组头 ([TitleGroup]) 横线的粗细与颜色。</summary>
        private const float TitleGroupLineThickness = 2f;

        private static readonly Color TitleGroupLineColor = new(0.4f, 0.4f, 0.4f, 0.3f);

        /// <summary>
        /// [Title]/[TitleGroup] 的对齐枚举 → 绘制层的水平对齐。Split 的主标题仍靠左(副标题才是被"劈"到
        /// 右侧的那半), 无副标题时 Split 退化为 Left。
        /// </summary>
        internal static PanelAlign TitleAnchor(TitleAlignments alignment)
        {
            switch (alignment)
            {
                case TitleAlignments.Centered: return PanelAlign.Center;
                case TitleAlignments.Right:    return PanelAlign.Right;
                default:                       return PanelAlign.Left;
            }
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
        internal static void RunStateUpdates(object target, CachedMember m, HostSession session = null)
        {
            var updates = GetAttrs<OnStateUpdateAttribute>(m);
            if (updates.Length == 0) return;

            var state = new PropertyState
            {
                Expanded = GetExpanded(target, m, session),
            };
            var value = GetSafe(target, m);

            foreach (var attr in updates)
            {
                if (string.IsNullOrEmpty(attr.Action)) continue;
                if (ExpressionEvaluator.IsExpression(attr.Action))
                {
                    ExpressionEvaluator.Evaluate(attr.Action, target, value, state);
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

        internal static bool GetExpanded(object target, CachedMember m, HostSession session = null)
        {
            // 没有记录时为展开: 折叠状态只在用户显式收起后才存在
            return (session ?? Session).GetToggle(target, m.RawName, true);
        }

        internal static void SetExpanded(object target, CachedMember m, bool value, HostSession session = null)
        {
            (session ?? Session).SetToggle(target, m.RawName, value);
        }

        /// <summary>
        /// 读取分组展开状态; 没有记录时返回分组属性声明的默认值(见 FoldoutGroupAttribute.Expanded)。
        /// 这里必须带上宿主对象: 分组路径在同类型的所有实例上是同一个字符串,
        /// 若只按路径存取, 同窗口里的两个同类型对象会共用一次折叠。
        /// </summary>
        internal static bool GetGroupExpanded(object target, GroupNode node, HostSession session = null)
        {
            return (session ?? Session).GetGroupToggle(target, node.GroupId, node.Expanded);
        }

        internal static void SetGroupExpanded(object target, GroupNode node, bool value, HostSession session = null)
        {
            (session ?? Session).SetGroupToggle(target, node.GroupId, value);
        }

        /// <summary>条件成员求值: 支持以 @ 开头的表达式与普通成员名。</summary>
        private static object ConditionValue(object target, string memberName)
        {
            if (ExpressionEvaluator.IsExpression(memberName))
            {
                return ExpressionEvaluator.Evaluate(memberName, target, null, null);
            }

            return GetMemberValue(target, memberName);
        }

        private static void DrawButton(object target, CachedMember m)
        {
            var attr = GetAttr<ButtonAttribute>(m);
            var label = attr?.ButtonName;
            if (string.IsNullOrEmpty(label)) label = FriendlyName(m.RawName);

            var height = attr != null && attr.ButtonHeight > 0 ? attr.ButtonHeight : DefaultButtonHeight;

            var spec = PanelButtonSpec.Of(height: height,
                                          tooltip: string.IsNullOrEmpty(attr?.Tooltip) ? null : attr.Tooltip,
                                          icon: attr?.Icon != SdfIconType.None ? Icons.FromSdf(attr.Icon) : null);

            // GUIColor 只作用于本次按钮: 由后端负责覆盖并在结束后还原(异常也不会污染整帧)。
            var color = ResolveGUIColor(m);
            if (color != null) spec = spec.WithBackground(color.Value);

            if (Drv.Button(label, spec))
            {
                InvokeSafe(target, m.Info);
            }
        }

        /// <summary>[Button] 不指定高度时的默认按钮高度。</summary>
        private const int DefaultButtonHeight = 24;

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
                    Log.Error($"[Inspector] 方法执行失败 {method.Name}\n{e}");
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
            if (inTableCell)
            {
                // 单元格里真正让标签消失的是 LabelForControl 返回的空标签; 这个只是给
                // 带自有标签的嵌套控件(向量分量之类)少留点位置。
                Drv.PushLabelWidth(0f);
            }
            else
            {
                // 没有 [LabelWidth] 时传 null: 保持外层宽度, 别把上层设好的值顶掉
                var labelWidth = GetAttr<LabelWidthAttribute>(m);
                Drv.PushLabelWidth(labelWidth != null ? labelWidth.Width : (float?)null);
            }

            try
            {
                DrawValueMemberBody(target, m, host, depth, inTableCell);
            }
            finally
            {
                Drv.PopLabelWidth();
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
                DrawDisplayString(target, m, inTableCell);
                return;
            }

            var readOnly = GetAttr<ReadOnlyAttribute>(m) != null;
            if (m.Kind == MemberKind.Property && (m.Info as PropertyInfo)?.SetMethod == null)
            {
                readOnly = true;
            }

            var disabled = readOnly || CheckDisabled(m, target);

            var value = GetSafe(target, m);
            var label = LabelForControl(m, inTableCell);

            object newValue = null;
            var changed = false;
            try
            {
                Drv.PushEnabled(disabled == false);

                var color    = ResolveGUIColor(m);
                var hasColor = color.HasValue;
                if (hasColor) Drv.PushColor(color.Value);

                try
                {
                    Drv.BeginHorizontal();
                    Drv.BeginCheck();

                    newValue = DrawScalarControl(target, m, type, value, label);

                    changed = Drv.EndCheck();
                    DrawInlineButtons(target, m);
                }
                finally
                {
                    Drv.EndHorizontal();
                    if (hasColor) Drv.PopColor();
                    Drv.PopEnabled();
                }
            }
            catch (Exception e)
            {
                Log.Error($"[Inspector] 绘制成员失败 {m.RawName}\n{e}");
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

                    Drv.RequestRepaint();
                }
                catch (Exception e)
                {
                    Log.Error($"[Inspector] 赋值失败 {m.RawName}\n{e}");
                }
            }
        }

        /// <summary>
        /// 以只读文本形式展示成员(DisplayAsString)。
        /// </summary>
        private static void DrawDisplayString(object target, CachedMember m, bool inTableCell = false)
        {
            var attr = GetAttr<DisplayAsStringAttribute>(m);
            if (attr == null) return;

            var value = GetSafe(target, m);
            var text = value?.ToString() ?? "";
            if (attr.MaxCharCount > 0 && text.Length > attr.MaxCharCount)
            {
                text = text.Substring(0, attr.MaxCharCount) + "...";
            }

            var spec = ToTextSpec(attr.Alignment, wrap: true, richText: true);

            var color    = ResolveGUIColor(m);
            var hasColor = color.HasValue;
            if (hasColor) Drv.PushColor(color.Value);

            var label = LabelForControl(m, inTableCell);
            Drv.Label(string.IsNullOrEmpty(label) ? text : $"{label}:\n{text}", spec);

            if (hasColor) Drv.PopColor();
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
            Drv.HelpBox(message, ToMessageKind(attr.MessageType));
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
                expanded = Drv.Foldout(expanded, GetLabel(m));
                if (expanded != GetExpanded(target, m)) Drv.RequestRepaint();
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
            if (IsUnityPropertyBagType(type)) return false;

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
                return Drv.ToggleField(label, Convert.ToBoolean(value));
            }

            if (type == typeof(string))
            {
                return DrawString(target, m, value as string, label);
            }

            // 曲线/渐变有专用控件; 值为 null 时交给后端自己处理(它会代以一个空曲线/空渐变)
            if (IsUnityPropertyBagType(type))
            {
                return Drv.StructField(label, value, type);
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
                        var res = Drv.Slider(label, cur, (float)min.MinValue, (float)max.MaxValue);
                        return Convert.ChangeType(res, type);
                    }

                    var curL = Convert.ToInt64(value ?? 0);
                    var resL = Drv.IntegerField(label, curL);
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
                        var res = Drv.Slider(label, cur, (float)min.MinValue, (float)max.MaxValue);
                        return Convert.ChangeType(res, type);
                    }

                    var curF = Convert.ToSingle(value ?? 0);
                    var resF = Drv.FloatField(label, curF);
                    return Convert.ChangeType(resF, type);
                }
            }

            // Unity 对象引用有专门的选择器: 它不是值类型, 别塞进 StructField
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return Drv.ObjectField(label, value as UnityEngine.Object, type);
            }

            // Vector* / Rect / Bounds / LayerMask 都由后端按类型挑控件
            if (IsStructFieldType(type))
            {
                return Drv.StructField(label, value, type);
            }

            // 兜底: 剩下的类型(char 就是靠它 + Convert.ChangeType 编辑的)走文本框。
            // 注意 Vector4/Quaternion/Rect/Bounds/LayerMask 这类曾经也落到这里, 结果一编辑就在
            // SetValue 处抛 ArgumentException 刷一条"赋值失败"、而且改动被丢掉, 所以上面逐个补了控件。
            return Drv.TextField(label, value?.ToString() ?? "");
        }

        /// <summary>有专用复合控件的结构类型: 交给后端挑具体控件(见 <see cref="IPanelDriver.StructField"/>)。</summary>
        private static bool IsStructFieldType(Type type)
        {
            return type == typeof(Vector2)     || type == typeof(Vector3)    || type == typeof(Vector4) ||
                   type == typeof(Vector2Int)  || type == typeof(Vector3Int) || type == typeof(Quaternion) ||
                   type == typeof(Rect)        || type == typeof(RectInt)    || type == typeof(Bounds) ||
                   type == typeof(BoundsInt)   || type == typeof(LayerMask);
        }

        private static object DrawString(object target, CachedMember m, string value, string label)
        {
            var file = GetAttr<FilePathAttribute>(m);
            var folder = GetAttr<FolderPathAttribute>(m);

            var result = Drv.TextField(label, value ?? "");

            if (file != null || folder != null)
            {
                if (Drv.Button("…", PanelButtonSpec.Of(width: PathPickerButtonWidth, expandWidth: false)))
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
                var contents = GetEnumLabels(type);

                var cur = Convert.ToInt32(value);
                var newIdx = Drv.Toolbar(cur, contents);
                if (newIdx == cur) return value;
                if (newIdx < 0 || newIdx >= values.Length) return value;
                return values.GetValue(newIdx);
            }

            var options = GetEnumLabels(type);

            var currentIdx = Array.IndexOf(values, value);
            if (currentIdx < 0) currentIdx = 0;
            var selected = Drv.Popup(label, currentIdx, options);
            return values.GetValue(selected);
        }

        /// <summary>枚举值显示文本缓存: 枚举类型 → 与 Enum.GetValues 同序的标签数组。标签来自静态特性, 按类型缓存安全。</summary>
        private static readonly Dictionary<Type, string[]> s_EnumLabelCache = new();

        /// <summary>[FilePath]/[FolderPath] 打开选择面板的那个"…"按钮的宽度。</summary>
        private const float PathPickerButtonWidth = 26f;
        /// <summary>枚举全部值的显示文本(与 <see cref="Enum.GetValues"/> 同序), 按类型缓存一次。</summary>
        internal static string[] GetEnumLabels(Type enumType)
        {
            if (s_EnumLabelCache.TryGetValue(enumType, out var cached)) return cached;

            var values = Enum.GetValues(enumType);
            var labels = new string[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                labels[i] = GetEnumLabel(enumType, values.GetValue(i));
            }

            s_EnumLabelCache[enumType] = labels;
            return labels;
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
                return Drv.TextField(label, value?.ToString() ?? "");
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

            var idx = Drv.Popup(label, selected, texts.ToArray());
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
            if (ShouldDrawLabel(m))
            {
                Drv.Label(GetLabel(m), CountedHeaderSpec);
            }

            DrawRecursive(value, host, depth + 1, target);
        }

        private static void DrawScalarLabelNull(CachedMember m)
        {
            Drv.KeyValueLabel(LabelOrEmpty(m), "(null)");
        }

        private static void DrawStringSet(object target, CachedMember m, IEnumerable collection)
        {
            var count = 0;
            foreach (var _ in collection) count++;

            Drv.BeginVertical(true);
            if (ShouldDrawLabel(m))
            {
                Drv.Label(CountedLabel(m, count), CountedHeaderSpec);
            }

            foreach (var item in collection)
            {
                Drv.KeyValueLabel("", item?.ToString() ?? "null");
            }

            Drv.EndVertical();
        }

        /// <summary>集合/列表的计数头: 一行加粗文字。</summary>
        private static PanelTextSpec CountedHeaderSpec => PanelTextSpec.Of(bold: true);

        private static void DrawDictionary(object target, CachedMember m, IDictionary dict, EditorWindow host)
        {
            var readOnly = GetAttr<ReadOnlyAttribute>(m) != null;
            var settings = GetAttr<DictionaryDrawerSettingsAttribute>(m);
            if (settings != null && settings.IsReadOnly) readOnly = true;

            var expanded = GetExpanded(target, m);

            Drv.BeginVertical(true);
            expanded = Drv.Foldout(expanded, CountedLabel(m, dict.Count));
            if (expanded != GetExpanded(target, m)) Drv.RequestRepaint();
            SetExpanded(target, m, expanded);

            if (expanded)
            {
                var keys = new List<object>();
                foreach (var k in dict.Keys) keys.Add(k);

                foreach (var k in keys)
                {
                    var v = dict[k];
                    Drv.BeginHorizontal();
                    var keyText = k?.ToString() ?? "";

                    if (readOnly)
                    {
                        // 只读分支画的是"能编辑但不会写回"的输入框: 与改造前一致(返回值本来就被丢掉了)
                        if (v != null && IsScalarLike(v.GetType()))
                        {
                            Drv.TextField(keyText, v.ToString());
                        }
                        else
                        {
                            Drv.KeyValueLabel(keyText, v?.ToString() ?? "null");
                        }
                    }
                    else
                    {
                        if (Drv.Button("-", RemoveButtonSpec))
                        {
                            Mutate(target, () => dict.Remove(k));
                            Drv.RequestRepaint();
                            Drv.EndHorizontal();
                            break;
                        }

                        if (v != null && IsScalarLike(v.GetType()))
                        {
                            var newText = Drv.TextField(keyText, v.ToString());
                            var parsed = ParseSimple(v.GetType(), newText);
                            if (parsed != null && Equals(parsed, v) == false)
                            {
                                Mutate(target, () => dict[k] = parsed);
                            }
                        }
                        else
                        {
                            Drv.KeyValueLabel(keyText, v?.ToString() ?? "null");
                        }
                    }

                    Drv.EndHorizontal();
                }
            }

            Drv.EndVertical();
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
            // Unity 对象列表: 只读列表展示
            if (list.Count > 0 && list[0] is UnityEngine.Object)
            {
                Drv.BeginVertical(true);
                if (ShouldDrawLabel(m))
                {
                    Drv.Label(CountedLabel(m, list.Count), CountedHeaderSpec);
                }
                foreach (var item in list)
                {
                    if (item is UnityEngine.Object uo)
                    {
                        Drv.ObjectField(uo.name, uo, uo.GetType());
                    }
                    else
                    {
                        Drv.KeyValueLabel("", item?.ToString() ?? "null");
                    }
                }

                Drv.EndVertical();
                return;
            }

            var listSettings = GetAttr<ListDrawerSettingsAttribute>(m);
            var readOnly = GetAttr<ReadOnlyAttribute>(m) != null;
            if (listSettings != null && listSettings.IsReadOnly) readOnly = true;

            var expanded = GetExpanded(target, m);

            Drv.BeginVertical(true);

            Drv.BeginHorizontal();
            expanded = Drv.Foldout(expanded, CountedLabel(m, list.Count));
            if (expanded != GetExpanded(target, m)) Drv.RequestRepaint();
            SetExpanded(target, m, expanded);
            Drv.FlexibleSpace();

            if (readOnly == false && Drv.Button("+", AddButtonSpec))
            {
                AddListElement(target, m, list, listSettings);
                Drv.RequestRepaint();
            }

            Drv.EndHorizontal();

            if (expanded)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var element = list[i];
                    Drv.BeginVertical(false);

                    if (element == null)
                    {
                        Drv.KeyValueLabel($"#{i}", "null");
                    }
                    else if (IsScalarLike(element.GetType()))
                    {
                        Drv.BeginHorizontal();
                        var newText = Drv.TextField($"#{i}", element.ToString());
                        if (readOnly == false && Drv.Button("-", RemoveRowButtonSpec))
                        {
                            Mutate(target, () => list.RemoveAt(i));
                            Drv.RequestRepaint();
                            Drv.EndHorizontal();
                            Drv.EndVertical();
                            break;
                        }

                        Drv.EndHorizontal();

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
                        if (readOnly == false &&
                            Drv.Button("-", RemoveRowButtonSpec.WithHeight(InlineRemoveButtonHeight)))
                        {
                            Mutate(target, () => list.RemoveAt(i));
                            Drv.RequestRepaint();
                            Drv.EndVertical();
                            break;
                        }

                        DrawRecursive(element, host, depth + 1, target);
                    }

                    Drv.EndVertical();
                    Drv.Space(2);
                }
            }

            Drv.EndVertical();
        }

        //-------------------------------------- 表格 (TableList) --------------------------------------

        private const int DefaultTableColumnWidth = 100;
        private const int TableIndexColumnWidth = 34;
        private const int TableRemoveColumnWidth = 24;

        //-------------------------------------- 集合增删按钮的规格 --------------------------------------
        // 各处宽度是历史约定, 迁移时逐个照搬(列表 22 / 表格按删除列宽 / 字典 22 且不撑开)

        /// <summary>列表元素行内的删除按钮: 定宽 22。</summary>
        private static readonly PanelButtonSpec RemoveRowButtonSpec = PanelButtonSpec.Of(width: 22);

        /// <summary>字典键值对的删除按钮: 定宽 22, 不参与水平撑开。</summary>
        private static readonly PanelButtonSpec RemoveButtonSpec =
            PanelButtonSpec.Of(width: 22, expandWidth: false);

        /// <summary>表格行尾的删除按钮: 宽度对齐删除列。</summary>
        private static readonly PanelButtonSpec TableRemoveButtonSpec =
            PanelButtonSpec.Of(width: TableRemoveColumnWidth, expandWidth: false);

        /// <summary>列表 / 表格头部的添加按钮: 定宽 24。</summary>
        private static readonly PanelButtonSpec AddButtonSpec = PanelButtonSpec.Of(width: 24);

        /// <summary>[InlineButton]: 按文字宽度收缩, 不吃整行。</summary>
        private static readonly PanelButtonSpec InlineButtonSpec = PanelButtonSpec.Of(expandWidth: false);

        /// <summary>列表里"嵌套对象元素"的删除按钮高度(比标量行矮一点)。</summary>
        private const float InlineRemoveButtonHeight = 16f;

        /// <summary>表头下那条分隔线的高度与颜色。</summary>
        private static readonly Color TableHeaderLineColor = new(0.3f, 0.3f, 0.3f, 0.35f);

        /// <summary>
        /// 表格至少留几行数据位。空表只留 1 行时, 表头 + 横向滚动条一占就只剩一条缝, 什么也看不清。
        /// </summary>
        private const int MinTableVisibleRows = 3;

        /// <summary>表头下那条分隔线的高度(见 <see cref="DrawHorizontalLine"/>)。</summary>
        private const float TableHeaderLineHeight = 1f;

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

            Drv.BeginVertical(true);

            if (table.AlwaysExpanded)
            {
                if (ShouldDrawLabel(m))
                {
                    Drv.Label(CountedLabel(m, list.Count), CountedHeaderSpec);
                }
            }
            else
            {
                Drv.BeginHorizontal();
                expanded = Drv.Foldout(expanded, CountedLabel(m, list.Count));
                if (expanded != GetExpanded(target, m)) Drv.RequestRepaint();
                SetExpanded(target, m, expanded);
                Drv.FlexibleSpace();
                if (showRemove && Drv.Button("+", AddButtonSpec))
                {
                    AddListElement(target, m, list, GetAttr<ListDrawerSettingsAttribute>(m));
                    Drv.RequestRepaint();
                }

                Drv.EndHorizontal();
            }

            if (expanded)
            {
                var scroll = Session.GetScroll(target, m.RawName);

                if (table.DrawScrollView)
                {
                    // 高度有界, 避免与外层窗口滚动视图互相抢空间
                    var headerRows = HasGroupedColumns(columns) ? 2 : 1;
                    var maxHeight = TableScrollHeight(list.Count, headerRows, table.NumberOfItemsPerPage,
                        Drv.LineHeight, Drv.ScrollbarHeight);
                    Drv.BeginScroll(ref scroll, maxHeight);
                    Session.SetScroll(target, m.RawName, scroll);
                }

                DrawTableHeader(columns, showIndex, showRemove);

                Drv.PushEnabled(readOnly == false);
                try
                {
                    DrawTableRows(target, m, list, columns, host, depth, showIndex, showRemove);
                }
                finally
                {
                    Drv.PopEnabled();
                }

                if (table.DrawScrollView)
                {
                    Drv.EndScroll();
                }
            }

            Drv.EndVertical();
        }

        /// <summary>表格里可见的数据行数: 空表也留 <see cref="MinTableVisibleRows"/> 行, 并封顶到 NumberOfItemsPerPage。</summary>
        internal static int TableVisibleRows(int itemCount, int itemsPerPage)
        {
            return Mathf.Clamp(itemCount, MinTableVisibleRows, Mathf.Max(MinTableVisibleRows, itemsPerPage));
        }

        /// <summary>
        /// 表格开 DrawScrollView 时的滚动区高度上限, 逐项算:
        /// (数据行 + 表头行) × 行高 + 横向滚动条 + 表头分隔线。
        /// 每一行都按 singleLineHeight 计, 而表头/单元格实际用的是更小的 mini 样式 —— 属于上界,
        /// 宁可多留几像素: 原来是"数据行 + 2"再硬编码 14px, 恰好比实际内容少几像素,
        /// 于是横向滚动条压住最后一行、分组表头那条带被裁掉一半。
        /// </summary>
        internal static float TableScrollHeight(int itemCount, int headerRows, int itemsPerPage,
                                                float lineHeight, float scrollbarHeight)
        {
            return lineHeight * (TableVisibleRows(itemCount, itemsPerPage) + headerRows)
                   + scrollbarHeight + TableHeaderLineHeight;
        }

        /// <summary>列里有没有顶层分组(有则表头是"分组带 + 列名"两行, 高度要多算一行)。</summary>
        private static bool HasGroupedColumns(List<TableColumn> columns)
        {
            foreach (var column in columns)
            {
                if (string.IsNullOrEmpty(column.GroupKey) == false) return true;
            }

            return false;
        }

        private static void DrawTableHeader(List<TableColumn> columns, bool showIndex, bool showRemove)
        {
            if (HasGroupedColumns(columns))
            {
                Drv.BeginHorizontal();
                if (showIndex) Drv.Space(TableIndexColumnWidth);

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
                        Drv.Space(width);
                    }
                    else
                    {
                        Drv.Label(columns[i].GroupName, PanelTextSpec.Of(PanelAlign.Center, true, false, false, false, width));
                    }

                    i = j;
                }

                if (showRemove) Drv.Space(TableRemoveColumnWidth);
                Drv.EndHorizontal();
            }

            Drv.BeginHorizontal();
            if (showIndex) Drv.Label("#", PanelTextSpec.Of(mini: true, bold: true, width: TableIndexColumnWidth));
            foreach (var column in columns)
            {
                Drv.Label(column.Label, PanelTextSpec.Of(mini: true, bold: true, width: column.Width));
            }

            if (showRemove) Drv.Space(TableRemoveColumnWidth);
            Drv.EndHorizontal();

            DrawHorizontalLine();
        }

        private static void DrawTableRows(object target, CachedMember m, IList list, List<TableColumn> columns,
                                          EditorWindow host, int depth, bool showIndex, bool showRemove)
        {
            if (list.Count == 0)
            {
                Drv.Label("(空)", PanelTextSpec.Of(mini: true));
                return;
            }

            for (var i = 0; i < list.Count; i++)
            {
                var element = list[i];

                Drv.BeginHorizontal();
                if (showIndex)
                {
                    Drv.Label($"#{i}", PanelTextSpec.Of(mini: true, width: TableIndexColumnWidth));
                }

                if (element == null)
                {
                    Drv.Label("null", PanelTextSpec.Of(mini: true));
                }
                else
                {
                    foreach (var column in columns)
                    {
                        DrawTableCell(element, column, host, depth);
                    }
                }

                var removed = false;
                if (showRemove && Drv.Button("-", TableRemoveButtonSpec))
                {
                    Mutate(target, () => list.RemoveAt(i));
                    Drv.RequestRepaint();
                    removed = true;
                }

                Drv.EndHorizontal();
                DrawHorizontalLine();

                if (removed) break;
            }
        }

        private static void DrawTableCell(object element, TableColumn column, EditorWindow host, int depth)
        {
            Drv.BeginVertical(false, column.Width);

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
                Drv.Label("", PanelTextSpec.Of(height: Drv.LineHeight));
            }

            Drv.EndVertical();
        }

        private static void DrawHorizontalLine()
        {
            Drv.Separator(TableHeaderLineHeight, TableHeaderLineColor);
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
                Log.Warn($"[Inspector] 无法添加列表元素 {elemType.Name}\n{e.Message}");
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
                            // Split: 主标题靠左、副标题同一行靠右(没有副标题时退化为 Left)
                            if (title.Alignment == TitleAlignments.Split &&
                                string.IsNullOrEmpty(title.Subtitle) == false)
                            {
                                Drv.BeginHorizontal();
                                Drv.Label(title.Title, PanelTextSpec.Of(PanelAlign.Left, title.Bold));
                                Drv.FlexibleSpace();
                                Drv.Label(title.Subtitle, PanelTextSpec.Of(PanelAlign.Right, mini: true));
                                Drv.EndHorizontal();
                            }
                            else
                            {
                                var anchor = TitleAnchor(title.Alignment);
                                Drv.Label(title.Title, PanelTextSpec.Of(anchor, title.Bold));
                                if (string.IsNullOrEmpty(title.Subtitle) == false)
                                {
                                    Drv.Label(title.Subtitle, PanelTextSpec.Of(anchor, mini: true));
                                }
                            }

                            if (title.HorizontalLine)
                            {
                                Drv.Separator(TitleGroupLineThickness, TitleGroupLineColor);
                            }

                            Drv.Space(4);
                        }

                        break;

                    case InfoBoxAttribute box:
                        if (IsAnnotationVisible(box.VisibleIfMemberName, target) == false) break;
                        Drv.HelpBox(box.Message, ToMessageKind(box.InfoMessageType));
                        break;

                    case DetailedInfoBoxAttribute detail:
                        if (IsAnnotationVisible(detail.VisibleIfMemberName, target) == false) break;
                        Drv.HelpBox(detail.Message + "\n" + detail.Details, ToMessageKind(detail.InfoMessageType));
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

        private static PanelMessageKind ToMessageKind(InfoMessageType type)
        {
            switch (type)
            {
                case InfoMessageType.Info:    return PanelMessageKind.Info;
                case InfoMessageType.Warning: return PanelMessageKind.Warning;
                case InfoMessageType.Error:   return PanelMessageKind.Error;
                default:                      return PanelMessageKind.None;
            }
        }

        /// <summary>[DisplayAsString] 的对齐枚举 → 绘制层的文本规格(只读文本: 自动换行 + 富文本)。</summary>
        private static PanelTextSpec ToTextSpec(TextAlign alignment, bool wrap = false, bool richText = false,
                                                bool bold = false, bool mini = false)
        {
            PanelAlign panelAlign;
            switch (alignment)
            {
                case TextAlign.Center: panelAlign = PanelAlign.Center; break;
                case TextAlign.Right:  panelAlign = PanelAlign.Right; break;
                default:               panelAlign = PanelAlign.Left; break;
            }

            return PanelTextSpec.Of(panelAlign, bold, mini, wrap, richText);
        }

        //-------------------------------------- 可见性 / 禁用 --------------------------------------

        private static bool CheckVisibility(CachedMember m, object target)
        {
            return CheckVisibility(m, target, IsPlaying);
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

        // 真值/相等语义与表达式求值器共用一套实现(Value), 否则同一个值在
        // [ShowIf("X", v)] 与 @X == v 两处会得到不同结论。
        private static bool Truthy(object value)
        {
            return Value.Truthy(value);
        }

        private static bool ValuesEqual(object a, object b)
        {
            return Value.Equal(a, b);
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
                Log.WarnOnce($"member-unresolved:{target.GetType().FullName}.{memberName}",
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
                Log.Warn($"[Inspector] 读取失败 {m.RawName}\n{e.Message}");
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

        /// <summary>
        /// 字段/属性的显示名, 三级回退:
        ///   1. 成员自身的 [LabelText];
        ///   2. 成员"类型"上的 [LabelText] —— 仅枚举, 见 <see cref="CachedMember.TypeLabel"/>;
        ///   3. FriendlyName(成员名)。
        /// 第 2 级是给"一个泛型基类承载 N 个枚举"用的: 那 N 个字段声明的其实是同一个成员,
        /// 名字只可能来自各自的枚举类型; 枚举自己又写不了成员名, 类型名是它唯一自然的标签。
        /// </summary>
        internal static string GetLabel(CachedMember m)
        {
            var label = GetAttr<LabelTextAttribute>(m);
            if (label != null && string.IsNullOrEmpty(label.Text) == false)
            {
                return label.Text;
            }

            var typeLabel = m.TypeLabel;
            if (string.IsNullOrEmpty(typeLabel) == false)
            {
                return typeLabel;
            }

            return FriendlyName(m.RawName);
        }

        /// <summary>
        /// 成员标签, 但 [HideLabel] 时为空串 —— 值控件(标量/枚举/字符串/对象预览)统一走这里:
        /// 控件照画, 只是没有名字。
        /// </summary>
        internal static string LabelOrEmpty(CachedMember m)
        {
            return GetAttr<HideLabelAttribute>(m) != null ? "" : GetLabel(m);
        }

        /// <summary>
        /// 值控件实际要用的标签: 表格单元格里**永远为空**。
        /// 单元格的字段名已经由列名指明了, 再画一遍标签不仅重复, 而且标签会吃掉约一列的宽度 ——
        /// 控件被挤到自己列之外, 看起来就像"值没刷出来"(截图里列名跑进单元格、CodeType 的下拉
        /// 挪到"层级"列, 都是这么来的)。
        /// </summary>
        internal static string LabelForControl(CachedMember m, bool inTableCell)
        {
            return inTableCell ? "" : LabelOrEmpty(m);
        }

        /// <summary>
        /// 容器头部(列表/字典/集合/表格)的"名字 + 计数"文案。
        /// [HideLabel] 时**只留计数**: 折叠头("▼")必须有一段可点的文字, 整行不画就没法展开了,
        /// 所以这里不能像纯文本头那样直接省略。
        /// </summary>
        internal static string CountedLabel(CachedMember m, int count)
        {
            var label = LabelOrEmpty(m);
            return string.IsNullOrEmpty(label) ? $"({count})" : $"{label} ({count})";
        }

        /// <summary>
        /// 非折叠的纯文本头行(只有一行 bold 文字, 没有箭头)该不该画。
        /// 没有名字就整行不画 —— 否则会留下一条什么都不说明的空白带。
        /// </summary>
        internal static bool ShouldDrawLabel(CachedMember m)
        {
            return string.IsNullOrEmpty(LabelOrEmpty(m)) == false;
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

                if (Drv.Button(text, InlineButtonSpec))
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
