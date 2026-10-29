using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RD.Draw;
using RD.Draw.Editor;
using RD.Draw.Editor.Metadata;
using RD.Draw.Editor.Rules;
using RD.Draw.Editor.Tables;
using NUnit.Framework;
using UnityEngine;

namespace RD.Draw.Tests.Editor
{
    /// <summary>
    /// Engine 中"画什么(可见性)、按什么顺序(排序)、归到哪一组(分组树)"的回归基线。
    ///
    /// 这三件事是纯反射逻辑, 不碰 , 因此可以脱离编辑器运行。它们是引擎的骨架,
    /// 也是后续重构(状态归属、抽 DrawContext)最先会动到的地方, 所以先钉住。
    ///
    /// 覆盖不到: CheckVisibility/CheckDisabled 依赖 Application.isPlaying(Unity ECall),
    /// 以及全部绘制流程与 Begin/End 配平, 仍只能靠 Unity Test Runner 或手工回归。
    /// </summary>
    public class EngineReflectionTests
    {

        /// <summary>
        /// 本测试类自带的引擎服务实例: 成员缓存、分组树、可见性判定与绘制路径同源,
        /// 但不共享引擎那份进程级实例, 因此用例之间不存在隐式的状态依赖。
        /// </summary>
        private static readonly EngineServices s_Services = new EngineServices();
        //-------------------------------------- 夹具 --------------------------------------

        private class VisibilitySample
        {
            public int PublicField;
            private int privatePlainField;

            [SerializeField] private int serializedPrivateField;
            [ShowInInspector] private int shownPrivateField;
            [Serialize] private int inspectorSerializedField;

            [HideInInspector] public int hiddenPublicField;
            [CompilerGenerated] private int compilerGeneratedField;

            public static int StaticField;
            [ShowInInspector] public static int ShownStaticField;

            public int PlainProperty { get; set; }
            [ShowInInspector] public int ShownProperty { get; set; }
            [ShowInInspector] private int ShownPrivateProperty { get; set; }
            [ShowInInspector] public int ShownGetOnlyProperty { get; }

            [Button] public void ButtonMethod() { }
            [OnInspectorGUI] public void OnGuiMethod() { }
            [Button] public void ButtonWithParam(int value) { }
            public void PlainMethod() { }
        }

        private class OrderSample
        {
            [PropertyOrder(10)] public int Ten;
            public int ZeroFirst;
            [PropertyOrder(-5)] public int Negative;
            public int ZeroSecond;
            [PropertyOrder(2)] public int Two;
        }

        private class GroupSample
        {
            public int Ungrouped;

            [VerticalGroup("信息")]
            public int VerticalMember;

            [ButtonGroup("信息/更新版本")]
            public void UpdateVersion() { }

            [BoxGroup("外观")]
            public int BoxMember;

            [VerticalGroup("信息")]
            public int AnotherVerticalMember;

            /// <summary>一个成员挂两个(带路径前缀的)分组属性 —— 每个属性都应从根走自己的路径。</summary>
            [VerticalGroup("父组")]
            [BoxGroup("父组/子盒")]
            public int MultiAttrMember;
        }

        private class VisibleGroupSample
        {
            public bool ShowAdvanced = true;

            [FoldoutGroup("Advanced", VisibleIf = "@ShowAdvanced")]
            public int Detail = 1;
        }

        /// <summary>Outer 组里只有 Inner 这一个子组, 用来验证"子组被隐藏后父组也不空画"。</summary>
        private class NestedVisibleGroupSample
        {
            public bool ShowDetail = true;

            [BoxGroup("Outer")]
            [FoldoutGroup("Outer/Inner", VisibleIf = "@ShowDetail")]
            public int Value = 1;
        }

        private class MethodLookupSample
        {
            [Button] public void Overloaded() { }
            [Button] public void Overloaded(int value) { }
            public void WithParamOnly(int value) { }
        }

        private class LookupBase
        {
            public void Ping() { }
        }

        private class LookupDerived : LookupBase
        {
            public new void Ping() { }
        }

        /// <summary>类型上带 [LabelText] 的枚举 —— 泛型基类里 N 个同名字段唯一的区分来源。</summary>
        [LabelText("带标签的枚举")]
        private enum TypeLabeledEnum
        {
            A,
            B,
        }

        private enum UnlabeledEnum
        {
            A,
        }

        /// <summary>类上也带 [LabelText], 但它是"类型的显示名", 不该被当作字段标签。</summary>
        [LabelText("类上的标签")]
        private class LabeledClass
        {
        }

        /// <summary>部分值带 [LabelText] 的枚举 —— 验证枚举标签数组"声明序 + 值级标签"。</summary>
        private enum OptionLabeledEnum
        {
            [LabelText("甲")] A,
            B,
            [LabelText("丙")] C,
        }

        private class LabelSample
        {
            [LabelText("显式标签")]
            public int Explicit = 0;

            [LabelText("")]
            public int EmptyLabel = 0;

            public TypeLabeledEnum FromEnumType = TypeLabeledEnum.A;

            public UnlabeledEnum FromUnlabeledEnumType = UnlabeledEnum.A;

            public LabeledClass FromClassType = new LabeledClass();

            public int PlainField = 0;
        }

        /// <summary>
        /// 容器头部(列表 / 字典 / 表格)的标签判定。三者共用同一套判定, 且只看成员元数据、不看值,
        /// 因此能脱离渲染验证 —— "画什么文字 / 画不画这一行"是纯函数, 剩下的只有布局调用本身。
        /// </summary>
        private class ContainerSample
        {
            public List<int> NamedList = new();

            [HideLabel]
            public List<int> HiddenList = new();

            [HideLabel]
            public Dictionary<string, int> HiddenDict = new();
        }

        /// <summary>表格列结构推导: 验证 [HideInTables] 把成员从表格列里剔除(但不影响普通面板绘制)。</summary>
        private class TableSample
        {
            public int Visible = 0;

            [HideInTables]
            public int HiddenInTable = 0;

            [VerticalGroup("信息")]
            public int GroupedVisible = 0;

            [VerticalGroup("信息")]
            [HideInTables]
            public int GroupedHiddenInTable = 0;
        }

        /// <summary>元素类型的所有成员都标 [HideInTables] 时, 列集合为空, 上层据此退回普通列表。</summary>
        private class AllHiddenTableSample
        {
            [HideInTables] public int A = 0;
            [HideInTables] public int B = 0;
        }

        //-------------------------------------- 辅助 --------------------------------------

        private static List<string> MemberNames(Type type)
        {
            var names = new List<string>();
            foreach (var m in s_Services.Members.Get(type))
            {
                names.Add(m.RawName);
            }

            return names;
        }

        private static CachedMember FindMember(Type type, string name)
        {
            foreach (var m in s_Services.Members.Get(type))
            {
                if (m.RawName == name) return m;
            }

            return null;
        }

        private static string LabelOf(string memberName)
        {
            return s_Services.Labels.GetLabel(FindMember(typeof(LabelSample), memberName));
        }

        private static CachedMember ContainerMemberOf(string memberName)
        {
            return FindMember(typeof(ContainerSample), memberName);
        }

        private static string ContainerHeaderOf(string memberName, int count)
        {
            return s_Services.Labels.CountedLabel(ContainerMemberOf(memberName), count);
        }

        private static string LabelOrEmptyOf(string memberName)
        {
            return s_Services.Labels.LabelOrEmpty(ContainerMemberOf(memberName));
        }

        private static bool ShouldDrawHeaderOf(string memberName)
        {
            return s_Services.Labels.ShouldDrawLabel(ContainerMemberOf(memberName));
        }

        private static GroupNode BuildGroupTree()
        {
            return BuildGroupTree(typeof(GroupSample));
        }

        private static GroupNode BuildGroupTree(Type type)
        {
            return s_Services.Groups.Get(type, s_Services.Members.Get(type));
        }

        private static GroupNode FindGroup(GroupNode parent, string groupId)
        {
            foreach (var child in parent.Children)
            {
                if (child.GroupId == groupId) return child;
            }

            return null;
        }

        //-------------------------------------- 可见性 --------------------------------------

        [Test]
        public void Members_PublicInstanceField_IsCollected()
        {
            CollectionAssert.Contains(MemberNames(typeof(VisibilitySample)), "PublicField");
        }

        [Test]
        public void Members_PrivateFieldWithoutAttribute_IsSkipped()
        {
            CollectionAssert.DoesNotContain(MemberNames(typeof(VisibilitySample)), "privatePlainField");
        }

        [TestCase("serializedPrivateField")]
        [TestCase("shownPrivateField")]
        [TestCase("inspectorSerializedField")]
        public void Members_PrivateFieldWithAnyShowingAttribute_IsCollected(string fieldName)
        {
            CollectionAssert.Contains(MemberNames(typeof(VisibilitySample)), fieldName);
        }

        [Test]
        public void Members_HideInInspector_Wins()
        {
            CollectionAssert.DoesNotContain(MemberNames(typeof(VisibilitySample)), "hiddenPublicField");
        }

        [Test]
        public void Members_CompilerGeneratedField_IsSkipped()
        {
            CollectionAssert.DoesNotContain(MemberNames(typeof(VisibilitySample)), "compilerGeneratedField");
        }

        [Test]
        public void Members_StaticField_IsSkippedUnlessExplicitlyShown()
        {
            var names = MemberNames(typeof(VisibilitySample));
            CollectionAssert.DoesNotContain(names, nameof(VisibilitySample.StaticField));
            CollectionAssert.Contains(names, nameof(VisibilitySample.ShownStaticField));
        }

        [Test]
        public void Members_PropertyNeedsExplicitAttribute()
        {
            var names = MemberNames(typeof(VisibilitySample));
            CollectionAssert.DoesNotContain(names, "PlainProperty");
            CollectionAssert.Contains(names, "ShownProperty");
            CollectionAssert.Contains(names, "ShownPrivateProperty");
            CollectionAssert.Contains(names, "ShownGetOnlyProperty");
        }

        [Test]
        public void Members_ButtonsAndOnInspectorGUI_AreCollectedWithCorrectKind()
        {
            Assert.AreEqual(MemberKind.MethodButton,
                FindMember(typeof(VisibilitySample), "ButtonMethod").Kind);
            Assert.AreEqual(MemberKind.MethodOnInspectorGUI,
                FindMember(typeof(VisibilitySample), "OnGuiMethod").Kind);
        }

        [Test]
        public void Members_MethodWithoutAttributeOrWithParameters_IsSkipped()
        {
            var names = MemberNames(typeof(VisibilitySample));
            CollectionAssert.DoesNotContain(names, "PlainMethod");
            CollectionAssert.DoesNotContain(names, "ButtonWithParam");
        }

        //-------------------------------------- 排序与缓存 --------------------------------------

        [Test]
        public void Members_AreSortedByPropertyOrderThenDeclaration()
        {
            CollectionAssert.AreEqual(
                new[] { "Negative", "ZeroFirst", "ZeroSecond", "Two", "Ten" },
                MemberNames(typeof(OrderSample)));
        }

        [Test]
        public void Members_AreCachedPerType()
        {
            Assert.AreSame(s_Services.Members.Get(typeof(OrderSample)),
                           s_Services.Members.Get(typeof(OrderSample)));
        }

        //-------------------------------------- 方法解析 --------------------------------------

        [Test]
        public void FindMethod_Overloads_ResolveToTheParameterlessOne()
        {
            var method = s_Services.Values.FindMethod(new MethodLookupSample(), nameof(MethodLookupSample.Overloaded));

            Assert.IsNotNull(method, "同名重载不能再让 Type.GetMethod 抛 AmbiguousMatchException");
            Assert.AreEqual(0, method.GetParameters().Length);
        }

        [Test]
        public void FindMethod_MethodWithParametersOnly_ReturnsNull()
        {
            Assert.IsNull(
                s_Services.Values.FindMethod(new MethodLookupSample(), nameof(MethodLookupSample.WithParamOnly)),
                "按钮/回调用 null 参数调用, 带参方法根本无法执行, 应直接判为'没有'");
        }

        [Test]
        public void FindMethod_PrefersMostDerivedDeclaration()
        {
            var method = s_Services.Values.FindMethod(new LookupDerived(), nameof(LookupDerived.Ping));

            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(LookupDerived), method.DeclaringType, "被 new 隐藏的成员应取最派生的那个");
        }

        //-------------------------------------- 分组树 --------------------------------------

        [Test]
        public void GroupTree_UngroupedMembersSitOnRoot()
        {
            var root = BuildGroupTree();

            Assert.AreEqual(GroupKind.None, root.Kind);
            Assert.AreEqual(1, root.Members.Count);
            Assert.AreEqual("Ungrouped", root.Members[0].RawName);
            Assert.AreEqual(3, root.Children.Count);   // 信息 / 外观 / 父组(多属性夹具成员)
        }

        [Test]
        public void GroupTree_GroupIdPathBuildsNestedNodes()
        {
            var root = BuildGroupTree();

            var info = FindGroup(root, "信息");
            Assert.IsNotNull(info);
            Assert.AreEqual("信息", info.GroupName);
            Assert.AreEqual(2, info.Members.Count, "两个 [VerticalGroup(\"信息\")] 成员应落在同一个组");
            Assert.AreEqual(1, info.Children.Count);

            var update = info.Children[0];
            Assert.AreEqual("信息/更新版本", update.GroupId, "GroupId 记录的是完整路径");
            Assert.AreEqual("更新版本", update.GroupName, "GroupName 只取路径末段");
            Assert.AreEqual(1, update.Members.Count);
            Assert.AreEqual("UpdateVersion", update.Members[0].RawName);
        }

        [Test]
        public void GroupTree_GroupKindComesFromAttribute()
        {
            var root = BuildGroupTree();

            var info = FindGroup(root, "信息");
            Assert.AreEqual(GroupKind.Vertical, info.Kind);
            Assert.AreEqual(GroupKind.ButtonGroup, FindGroup(info, "信息/更新版本").Kind);
            Assert.AreEqual(GroupKind.Box, FindGroup(root, "外观").Kind);
        }

        [Test]
        public void GroupTree_SiblingsFollowDeclarationOrder()
        {
            var root = BuildGroupTree();

            Assert.AreEqual("信息", root.Children[0].GroupId);
            Assert.AreEqual("外观", root.Children[1].GroupId);
            Assert.AreEqual("父组", root.Children[2].GroupId);
        }

        [Test]
        public void GroupTree_MultiAttrMember_WalksEachPathFromRoot()
        {
            var root = BuildGroupTree();

            var parent = FindGroup(root, "父组");
            Assert.IsNotNull(parent, "夹具上应存在分组 父组");

            // 每个分组属性都从根沿自己的路径走: 子盒应是父组的直接子节点, 且父组下不会再出现同名中间节点
            var box = FindGroup(parent, "父组/子盒");
            Assert.IsNotNull(box, "父组/子盒 应是 父组 的直接子节点(第二个属性不能接着第一个的终点走)");
            Assert.AreEqual(GroupKind.Box, box.Kind);
            Assert.IsNull(FindGroup(parent, "父组"), "父组 下不应再出现同名中间节点(重复路径行走会造副本)");
        }

        [Test]
        public void GroupTree_IsCachedPerType()
        {
            Assert.AreSame(BuildGroupTree(), BuildGroupTree());
        }

        //-------------------------------------- 组级可见性 (VisibleIf) --------------------------------------

        [Test]
        public void GroupVisibleIf_True_KeepsGroup()
        {
            var node = FindGroup(BuildGroupTree(typeof(VisibleGroupSample)), "Advanced");
            Assert.IsNotNull(node, "夹具上应存在分组 Advanced");

            Assert.IsTrue(s_Services.Visibility.IsGroupVisible(node, new VisibleGroupSample { ShowAdvanced = true }, false));
        }

        [Test]
        public void GroupVisibleIf_False_HidesWholeGroup()
        {
            var node = FindGroup(BuildGroupTree(typeof(VisibleGroupSample)), "Advanced");
            Assert.IsNotNull(node, "夹具上应存在分组 Advanced");

            Assert.IsFalse(s_Services.Visibility.IsGroupVisible(node, new VisibleGroupSample { ShowAdvanced = false }, false),
                "VisibleIf 为假时整组不画, 而不是留下一个空框");
        }

        [Test]
        public void GroupVisibleIf_False_MakesParentHaveNoVisibleContent()
        {
            var outer = FindGroup(BuildGroupTree(typeof(NestedVisibleGroupSample)), "Outer");
            Assert.IsNotNull(outer, "夹具上应存在分组 Outer");

            Assert.IsTrue(s_Services.Visibility.IsGroupVisible(outer, new NestedVisibleGroupSample { ShowDetail = true }, false));
            Assert.IsFalse(s_Services.Visibility.IsGroupVisible(outer, new NestedVisibleGroupSample { ShowDetail = false }, false),
                "唯一的子组被 VisibleIf 隐藏后, 父组不应再被当成'还有内容'而画出空框");
        }

        //-------------------------------------- 标签回退 --------------------------------------

        [Test]
        public void Label_MemberOwnLabelTextWins()
        {
            Assert.AreEqual("显式标签", LabelOf(nameof(LabelSample.Explicit)));
        }

        [Test]
        public void Label_EmptyMemberLabelTextIsTreatedAsAbsent()
        {
            Assert.AreEqual("Empty Label", LabelOf(nameof(LabelSample.EmptyLabel)),
                "[LabelText(\"\")] 应等同于没写, 否则标签会变成空白");
        }

        [Test]
        public void Label_FallsBackToEnumTypeLabelText()
        {
            Assert.AreEqual("带标签的枚举", LabelOf(nameof(LabelSample.FromEnumType)),
                "枚举类型上的 [LabelText] 是泛型基类里 N 个同名字段唯一能区分的标签来源");
            Assert.AreEqual("From Unlabeled Enum Type", LabelOf(nameof(LabelSample.FromUnlabeledEnumType)),
                "枚举类型没写 [LabelText] 时不该猜, 退回 FriendlyName");
        }

        [Test]
        public void Label_ClassTypeLabelTextIsNotUsed()
        {
            Assert.AreEqual("From Class Type", LabelOf(nameof(LabelSample.FromClassType)),
                "类上的 [LabelText] 是类型自身的显示名(组件菜单等), 不参与字段标签");
        }

        [Test]
        public void Label_PlainMemberUsesFriendlyName()
        {
            Assert.AreEqual("Plain Field", LabelOf(nameof(LabelSample.PlainField)));
        }

        //-------------------------------------- 容器头 (列表 / 字典 / 表格) --------------------------------------

        [Test]
        public void ContainerHeader_NamedContainer_IsNamePlusCount()
        {
            Assert.AreEqual("Named List (3)", ContainerHeaderOf("NamedList", 3));
        }

        [Test]
        public void ContainerHeader_HideLabel_KeepsOnlyTheCount()
        {
            // 无名列表的头部只剩计数文案("N items"), 折叠箭头仍在, 所以还能展开 ——
            // 整行不画的话用户就没有可以点的入口了。
            Assert.AreEqual("3 items", ContainerHeaderOf("HiddenList", 3),
                "折叠头(\"▼\")必须留一段可点的文字, 整行不画就没法展开了");
            Assert.AreEqual("", LabelOrEmptyOf("HiddenDict"),
                "字典头与列表头走同一套判定");
        }

        [Test]
        public void PlainContainerHeader_HideLabel_IsNotDrawn()
        {
            Assert.IsTrue(ShouldDrawHeaderOf("NamedList"));
            Assert.IsFalse(ShouldDrawHeaderOf("HiddenList"),
                "没有箭头的纯文本头在 [HideLabel] 时应整行不画, 而不是留一条什么都不说明的空白带");
        }

        //-------------------------------------- 值控件的标签 (单元格里必须为空) --------------------------------------

        [Test]
        public void ControlLabel_InTableCell_IsAlwaysEmpty()
        {
            var member = FindMember(typeof(LabelSample), nameof(LabelSample.Explicit));
            Assert.AreEqual("显式标签", s_Services.Labels.LabelForControl(member, false));
            Assert.AreEqual("", s_Services.Labels.LabelForControl(member, true),
                "单元格里不画标签: 字段名已由列名给出, 再画一遍会吃掉约一列宽度, 控件被挤到隔壁列");
        }

        [Test]
        public void ControlLabel_HideLabelOutsideTable_IsAlsoEmpty()
        {
            var hidden = FindMember(typeof(ContainerSample), nameof(ContainerSample.HiddenList));
            Assert.AreEqual("", s_Services.Labels.LabelForControl(hidden, false), "表格外沿用 [HideLabel] 的语义");
        }

        //-------------------------------------- 标题对齐 --------------------------------------

        [Test]
        public void TitleAnchor_MapsAllFourAlignments()
        {
            Assert.AreEqual(DrawAlign.Left, s_Services.Labels.TitleAnchor(TitleAlignment.Left));
            Assert.AreEqual(DrawAlign.Center, s_Services.Labels.TitleAnchor(TitleAlignment.Centered));
            Assert.AreEqual(DrawAlign.Right, s_Services.Labels.TitleAnchor(TitleAlignment.Right));
            Assert.AreEqual(DrawAlign.Left, s_Services.Labels.TitleAnchor(TitleAlignment.Split),
                "Split 的主标题仍靠左 —— 被劈到右侧的是副标题, 没有副标题时 Split 退化为 Left");
        }

        //-------------------------------------- 枚举标签缓存 --------------------------------------

        [Test]
        public void EnumLabels_DeclarationOrderAndValueLabels()
        {
            Assert.AreEqual(new[] { "甲", "B", "丙" }, s_Services.Enums.GetLabels(typeof(OptionLabeledEnum)),
                "带 [LabelText] 的值用标签, 没带的用枚举名, 顺序与 Enum.GetValues 一致");
        }

        [Test]
        public void EnumLabels_CachedPerType()
        {
            var first = s_Services.Enums.GetLabels(typeof(OptionLabeledEnum));
            var second = s_Services.Enums.GetLabels(typeof(OptionLabeledEnum));
            Assert.AreSame(first, second, "同一枚举类型应命中缓存复用同一数组, 否则枚举下拉每帧都在分配");
        }

        //-------------------------------------- 表格滚动区高度 --------------------------------------

        /// <summary>行高与横向滚动条高度的真实取值(测试里固定成常量, 免得依赖编辑器皮肤)。</summary>
        private const float TestLineHeight = 18f;

        private const float TestScrollbarHeight = 15f;

        [Test]
        public void TableVisibleRows_EmptyTable_KeepsAMinimum()
        {
            Assert.AreEqual(3, TableRenderer.TableVisibleRows(0, 15), "空表也要留几行数据位, 否则表体只剩一条缝");
            Assert.AreEqual(5, TableRenderer.TableVisibleRows(5, 15), "数据少于每页数时按实际行数留");
            Assert.AreEqual(15, TableRenderer.TableVisibleRows(100, 15), "封顶到 NumberOfItemsPerPage, 不跟外层滚动视图抢空间");
        }

        [Test]
        public void TableScrollHeight_AlwaysCoversHeaderAndScrollbar()
        {
            foreach (var itemCount in new[] { 0, 1, 5, 100 })
            {
                foreach (var headerRows in new[] { 1, 2 })
                {
                    // 内容实打实要占的: 表头 + 至少一行数据 + 分隔线 + 横向滚动条
                    var needed = TestLineHeight * (headerRows + 1) + TestScrollbarHeight + 1f;
                    var reserved = TableRenderer.TableScrollHeight(itemCount, headerRows, 15,
                        TestLineHeight, TestScrollbarHeight);

                    Assert.GreaterOrEqual(reserved, needed,
                        $"{itemCount} 行数据 / {headerRows} 行表头时预留高度小于内容, 最后一行会被裁");
                }
            }
        }

        [Test]
        public void TableScrollHeight_EmptyGroupedTable_CoversWhatTheOldFormulaMissed()
        {
            // 回归: 旧算法"行高 × (数据行 + 2) + 14"在"空表 + 分组表头"下算出的高度,
            // 比内容(两行表头 + 分隔线 + 一行 (空) + 横向滚动条)还少几像素 ——
            // 截图里分组表头被压扁、滚动条压住 (空) 行, 根因就是这个, 不是"主观觉得小"。
            var needed = TestLineHeight * 2 + 1f + TestLineHeight + TestScrollbarHeight;
            var oldFormula = TestLineHeight * (1 + 2) + 14f;
            var reserved = TableRenderer.TableScrollHeight(0, 2, 15, TestLineHeight, TestScrollbarHeight);

            Assert.Less(oldFormula, needed, "旧算法的预留高度确实小于内容");
            Assert.GreaterOrEqual(reserved, needed);
        }

        //-------------------------------------- 表格列结构 ([HideInTables]) --------------------------------------

        [Test]
        public void TableSchema_HideInTables_ExcludesMemberFromColumns()
        {
            var columns = s_Services.Tables.Get(typeof(TableSample));

            // 可见成员产生 2 列: 分组"信息"合并成一列, Visible 独立一列; 两个 [HideInTables] 成员不产列
            Assert.AreEqual(2, columns.Count, "隐藏成员既不单独成列, 也不让所属分组凭空多出一列");

            var names = new List<string>();
            foreach (var col in columns)
            {
                foreach (var m in col.Members) names.Add(m.RawName);
            }

            Assert.Contains("Visible", names);
            Assert.Contains("GroupedVisible", names);
            Assert.IsFalse(names.Contains("HiddenInTable"), "[HideInTables] 成员不得出现在任何列里");
            Assert.IsFalse(names.Contains("GroupedHiddenInTable"), "分组内的 [HideInTables] 成员同样剔除");
        }

        [Test]
        public void TableSchema_HideInTables_AllHidden_YieldsNoColumns()
        {
            var columns = s_Services.Tables.Get(typeof(AllHiddenTableSample));
            Assert.AreEqual(0, columns.Count, "元素类型无可表格化成员时列集合为空, 上层据此退回普通列表");
        }
    }
}
