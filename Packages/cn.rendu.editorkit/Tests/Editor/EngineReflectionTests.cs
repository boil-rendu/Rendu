using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RD.EditorKit;
using RD.EditorKit.Editor;
using NUnit.Framework;
using UnityEngine;

namespace RD.EditorKit.Tests.Editor
{
    /// <summary>
    /// Engine 中"画什么(可见性)、按什么顺序(排序)、归到哪一组(分组树)"的回归基线。
    ///
    /// 这三件事是纯反射逻辑, 不碰 IMGUI, 因此可以脱离编辑器运行。它们是引擎的骨架,
    /// 也是后续重构(状态归属、抽 DrawContext)最先会动到的地方, 所以先钉住。
    ///
    /// 覆盖不到: CheckVisibility/CheckDisabled 依赖 Application.isPlaying(Unity ECall),
    /// 以及全部绘制流程与 Begin/End 配平, 仍只能靠 Unity Test Runner 或手工回归。
    /// </summary>
    public class EngineReflectionTests
    {
        //-------------------------------------- 夹具 --------------------------------------

        private class VisibilitySample
        {
            public int PublicField;
            private int privatePlainField;

            [SerializeField] private int serializedPrivateField;
            [ShowInInspector] private int shownPrivateField;
            [InspectorSerialize] private int inspectorSerializedField;

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

        //-------------------------------------- 辅助 --------------------------------------

        private static List<string> MemberNames(Type type)
        {
            var names = new List<string>();
            foreach (var m in Engine.GetMembers(type))
            {
                names.Add(m.RawName);
            }

            return names;
        }

        private static Engine.CachedMember FindMember(Type type, string name)
        {
            foreach (var m in Engine.GetMembers(type))
            {
                if (m.RawName == name) return m;
            }

            return null;
        }

        private static Engine.GroupNode BuildGroupTree()
        {
            return BuildGroupTree(typeof(GroupSample));
        }

        private static Engine.GroupNode BuildGroupTree(Type type)
        {
            return Engine.GetGroupTree(type, Engine.GetMembers(type));
        }

        private static Engine.GroupNode FindGroup(Engine.GroupNode parent, string groupId)
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
            Assert.AreEqual(Engine.MemberKind.MethodButton,
                FindMember(typeof(VisibilitySample), "ButtonMethod").Kind);
            Assert.AreEqual(Engine.MemberKind.MethodOnInspectorGUI,
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
            Assert.AreSame(Engine.GetMembers(typeof(OrderSample)),
                           Engine.GetMembers(typeof(OrderSample)));
        }

        //-------------------------------------- 方法解析 --------------------------------------

        [Test]
        public void FindMethod_Overloads_ResolveToTheParameterlessOne()
        {
            var method = Engine.FindMethod(new MethodLookupSample(), nameof(MethodLookupSample.Overloaded));

            Assert.IsNotNull(method, "同名重载不能再让 Type.GetMethod 抛 AmbiguousMatchException");
            Assert.AreEqual(0, method.GetParameters().Length);
        }

        [Test]
        public void FindMethod_MethodWithParametersOnly_ReturnsNull()
        {
            Assert.IsNull(
                Engine.FindMethod(new MethodLookupSample(), nameof(MethodLookupSample.WithParamOnly)),
                "按钮/回调用 null 参数调用, 带参方法根本无法执行, 应直接判为'没有'");
        }

        [Test]
        public void FindMethod_PrefersMostDerivedDeclaration()
        {
            var method = Engine.FindMethod(new LookupDerived(), nameof(LookupDerived.Ping));

            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(LookupDerived), method.DeclaringType, "被 new 隐藏的成员应取最派生的那个");
        }

        //-------------------------------------- 分组树 --------------------------------------

        [Test]
        public void GroupTree_UngroupedMembersSitOnRoot()
        {
            var root = BuildGroupTree();

            Assert.AreEqual(Engine.GroupKind.None, root.Kind);
            Assert.AreEqual(1, root.Members.Count);
            Assert.AreEqual("Ungrouped", root.Members[0].RawName);
            Assert.AreEqual(2, root.Children.Count);
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
            Assert.AreEqual(Engine.GroupKind.Vertical, info.Kind);
            Assert.AreEqual(Engine.GroupKind.ButtonGroup, FindGroup(info, "信息/更新版本").Kind);
            Assert.AreEqual(Engine.GroupKind.Box, FindGroup(root, "外观").Kind);
        }

        [Test]
        public void GroupTree_SiblingsFollowDeclarationOrder()
        {
            var root = BuildGroupTree();

            Assert.AreEqual("信息", root.Children[0].GroupId);
            Assert.AreEqual("外观", root.Children[1].GroupId);
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

            Assert.IsTrue(Engine.IsGroupVisible(node, new VisibleGroupSample { ShowAdvanced = true }, false));
        }

        [Test]
        public void GroupVisibleIf_False_HidesWholeGroup()
        {
            var node = FindGroup(BuildGroupTree(typeof(VisibleGroupSample)), "Advanced");
            Assert.IsNotNull(node, "夹具上应存在分组 Advanced");

            Assert.IsFalse(Engine.IsGroupVisible(node, new VisibleGroupSample { ShowAdvanced = false }, false),
                "VisibleIf 为假时整组不画, 而不是留下一个空框");
        }

        [Test]
        public void GroupVisibleIf_False_MakesParentHaveNoVisibleContent()
        {
            var outer = FindGroup(BuildGroupTree(typeof(NestedVisibleGroupSample)), "Outer");
            Assert.IsNotNull(outer, "夹具上应存在分组 Outer");

            Assert.IsTrue(Engine.IsGroupVisible(outer, new NestedVisibleGroupSample { ShowDetail = true }, false));
            Assert.IsFalse(Engine.IsGroupVisible(outer, new NestedVisibleGroupSample { ShowDetail = false }, false),
                "唯一的子组被 VisibleIf 隐藏后, 父组不应再被当成'还有内容'而画出空框");
        }
    }
}
