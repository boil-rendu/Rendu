using System;
using System.Collections.Generic;
using RD.Draw;
using RD.Draw.Editor;
using NUnit.Framework;

namespace RD.Draw.Tests.Editor
{
    /// <summary>
    /// 引擎「折叠状态(成员与分组) + OnStateUpdate」这块的回归基线。
    ///
    /// 状态的存放走过三代, 本文件把每一代都可观察到的语义钉成契约:
    ///   1. 进程级静态字典, 键为 RuntimeHelpers.GetHashCode(target) + "_" + 成员名 —— 对象销毁后键不清理;
    ///   2. 会话对象 HostSession 内的同类键 —— 生命周期跟随宿主, 但键仍不回收、身份哈希仍可被复用;
    ///   3. 现在: 会话内按宿主对象弱引用分桶(ConditionalWeakTable), 桶内才按成员名索引,
    ///      判定走"引用相等" —— 对象被回收即整桶消失, 既不残留键, 也不会把状态错给后来的对象。
    /// 契约本身没变: 默认展开、按成员隔离、按实例隔离、按会话隔离;
    /// 分组展开(桶内键是 GroupID 路径)与成员折叠走同一套分桶, 因此同样逐实例独立。
    ///
    /// 未覆盖: 所有绘制路径对 GetExpanded/SetExpanded 的调用(如 foldout、Dictionary/List/Table 的展开)。
    /// </summary>
    public class EngineStateTests
    {
        //-------------------------------------- 夹具 --------------------------------------

        private class StateSample
        {
            public int Plain = 1;

            [OnStateUpdate("@$property.State.Expanded = false")]
            public int AutoCollapse = 2;

            [OnStateUpdate("@$property.State.Expanded = true")]
            public int AutoExpand = 3;

            [OnStateUpdate("MarkCalled")]
            public int ViaMethod = 4;

            public int MethodCalls;

            public void MarkCalled()
            {
                this.MethodCalls++;
            }
        }

        /// <summary>
        /// 状态键曾经取自对象身份哈希, 夹具实例若在测试之间被回收, 新实例有可能复用到同一个哈希,
        /// 从而读到上一个测试残留的折叠状态, 让断言随机失败 —— 这是保活实例的由来。
        /// 现在状态按对象弱引用分桶(引用相等), 已不存在哈希复用问题; 保活仅作为习惯保留,
        /// 免得用例对 GC 时机产生隐式依赖, 也避免后台回收干扰其它断言。
        /// </summary>
        private static readonly List<object> s_KeepAlive = new();

        private static StateSample NewSample()
        {
            var sample = new StateSample();
            s_KeepAlive.Add(sample);
            return sample;
        }

        private static Engine.CachedMember Member(string name)
        {
            foreach (var m in Engine.GetMembers(typeof(StateSample)))
            {
                if (m.RawName == name) return m;
            }

            Assert.Fail($"夹具 StateSample 上找不到成员 {name}");
            return null;
        }

        //-------------------------------------- 折叠状态 --------------------------------------

        [Test]
        public void FoldedState_DefaultsToExpanded()
        {
            var sample = NewSample();
            Assert.IsTrue(Engine.GetExpanded(sample, Member("Plain")), "没有记录时默认展开");
        }

        [Test]
        public void FoldedState_RoundTripsThroughSetExpanded()
        {
            var sample = NewSample();
            var member = Member("Plain");

            Engine.SetExpanded(sample, member, false);
            Assert.IsFalse(Engine.GetExpanded(sample, member));

            Engine.SetExpanded(sample, member, true);
            Assert.IsTrue(Engine.GetExpanded(sample, member));
        }

        [Test]
        public void FoldedState_IsolatedPerMember()
        {
            var sample = NewSample();
            var plain = Member("Plain");
            var autoExpand = Member("AutoExpand");

            Engine.SetExpanded(sample, plain, false);

            Assert.IsFalse(Engine.GetExpanded(sample, plain));
            Assert.IsTrue(Engine.GetExpanded(sample, autoExpand), "同实例的不同成员互不影响");
        }

        [Test]
        public void FoldedState_IsolatedPerInstance()
        {
            var a = NewSample();
            var b = NewSample();
            var member = Member("Plain");

            Engine.SetExpanded(a, member, false);

            Assert.IsFalse(Engine.GetExpanded(a, member));
            Assert.IsTrue(Engine.GetExpanded(b, member), "折叠状态按实例隔离");
        }

        [Test]
        public void FoldedState_OwnerStateIsReleasedWithTheOwner()
        {
            var member = Member("Plain");
            var session = new HostSession();

            var weak = CollapseOnThrowawayInstance(session, member);
            ForceCollection();

            Assert.IsFalse(weak.IsAlive, "会话按弱引用分桶, 不应让宿主对象无法回收");
            Assert.IsTrue(Engine.GetExpanded(NewSample(), member, session),
                "宿主对象回收后, 不会把折叠状态留给后来的对象");
        }

        /// <summary>在独立栈帧里给临时对象设状态, 保证返回后它只被弱引用指向。</summary>
        private static WeakReference CollapseOnThrowawayInstance(HostSession session, Engine.CachedMember member)
        {
            var sample = new StateSample();     // 刻意不进 s_KeepAlive, 允许回收
            Engine.SetExpanded(sample, member, false, session);
            return new WeakReference(sample);
        }

        private static void ForceCollection()
        {
            for (var i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        //-------------------------------------- RunStateUpdates --------------------------------------

        [Test]
        public void RunStateUpdates_ExpressionCanCollapseAndExpand()
        {
            var sample = NewSample();

            var autoCollapse = Member("AutoCollapse");
            Assert.IsTrue(Engine.GetExpanded(sample, autoCollapse), "起点是默认展开");
            Engine.RunStateUpdates(sample, autoCollapse);
            Assert.IsFalse(Engine.GetExpanded(sample, autoCollapse),
                "@$property.State.Expanded = false 应写回折叠状态");

            var autoExpand = Member("AutoExpand");
            Engine.SetExpanded(sample, autoExpand, false);
            Engine.RunStateUpdates(sample, autoExpand);
            Assert.IsTrue(Engine.GetExpanded(sample, autoExpand),
                "@$property.State.Expanded = true 应写回展开状态");
        }

        [Test]
        public void RunStateUpdates_IgnoresMemberWithoutAttribute()
        {
            var sample = NewSample();
            var plain = Member("Plain");

            Engine.SetExpanded(sample, plain, false);
            Engine.RunStateUpdates(sample, plain);

            Assert.IsFalse(Engine.GetExpanded(sample, plain),
                "没有 [OnStateUpdate] 的成员不应被改动");
        }

        [Test]
        public void RunStateUpdates_NonExpressionActionInvokesMethodByName()
        {
            var sample = NewSample();
            var viaMethod = Member("ViaMethod");

            Assert.AreEqual(0, sample.MethodCalls);
            Engine.RunStateUpdates(sample, viaMethod);
            Assert.AreEqual(1, sample.MethodCalls, "非表达式 Action 按方法名直接调用");
        }

        //-------------------------------------- 会话隔离 --------------------------------------

        [Test]
        public void FoldedState_SeparateSessionsDoNotShareState()
        {
            var sample = NewSample();
            var member = Member("Plain");

            var windowA = new HostSession();
            var windowB = new HostSession();

            Engine.SetExpanded(sample, member, false, windowA);

            Assert.IsFalse(Engine.GetExpanded(sample, member, windowA));
            Assert.IsTrue(Engine.GetExpanded(sample, member, windowB),
                "不同会话(例如两个窗口)的折叠状态应彼此独立");
        }

        [Test]
        public void RunStateUpdates_TargetsGivenSession()
        {
            var sample = NewSample();
            var autoCollapse = Member("AutoCollapse");
            var session = new HostSession();

            Engine.RunStateUpdates(sample, autoCollapse, session);

            Assert.IsFalse(Engine.GetExpanded(sample, autoCollapse, session));
            Assert.IsTrue(Engine.GetExpanded(sample, autoCollapse), "默认会话不应被牵连改动");
        }

        //-------------------------------------- 分组展开状态 --------------------------------------

        private class GroupSample
        {
            // 刻意用非默认值(true), 这样 fallback 若被写死成 false 会被断言抓到
            [FoldoutGroup("Folded", true)]
            public int Inside = 1;
        }

        private static GroupSample NewGroupSample()
        {
            var sample = new GroupSample();
            s_KeepAlive.Add(sample);
            return sample;
        }

        /// <summary>分组树的根 GroupId 为空串, 因此按 GroupId 递归查找。</summary>
        private static Engine.GroupNode FindGroup(Engine.GroupNode node, string groupId)
        {
            if (node.GroupId == groupId) return node;
            foreach (var child in node.Children)
            {
                var found = FindGroup(child, groupId);
                if (found != null) return found;
            }

            return null;
        }

        private static Engine.GroupNode FoldedGroup()
        {
            var members = Engine.GetMembers(typeof(GroupSample));
            var tree = Engine.GetGroupTree(typeof(GroupSample), members);
            var node = FindGroup(tree, "Folded");
            Assert.IsNotNull(node, "夹具 GroupSample 上应存在分组 Folded");
            return node;
        }

        [Test]
        public void GroupExpanded_FallsBackToAttributeDefault()
        {
            var node = FoldedGroup();
            var session = new HostSession();

            Assert.AreEqual(node.Expanded, Engine.GetGroupExpanded(NewGroupSample(), node, session),
                "没有记录时用分组属性声明的默认值");
        }

        [Test]
        public void GroupExpanded_IsolatedPerInstance()
        {
            var node = FoldedGroup();
            var session = new HostSession();
            var a = NewGroupSample();
            var b = NewGroupSample();

            Engine.SetGroupExpanded(a, node, false, session);

            Assert.IsFalse(Engine.GetGroupExpanded(a, node, session));
            Assert.IsTrue(Engine.GetGroupExpanded(b, node, session),
                "同一分组路径在两个实例上必须各自独立, 否则折叠一个会连带折叠另一个");
        }

        //-------------------------------------- 宿主状态句柄 --------------------------------------

        [Test]
        public void CreateHostState_ReturnsFreshHandles()
        {
            var a = Engine.CreateHostState();
            var b = Engine.CreateHostState();

            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.AreNotSame(a, b, "每个宿主各持一份句柄, 两次创建不能返回同一个");
        }

        [Test]
        public void HostState_SessionsAreIsolated()
        {
            // 句柄的意义就在于此: 两个句柄背后是两份会话, 状态不会串台。
            // (GroupExpanded_IsolatedPerInstance 验证的是"同一会话内按实例分桶",
            //  这里补上"不同宿主各拿各的会话"这半边。)
            var node    = FoldedGroup();
            var hostA   = Engine.CreateHostState();
            var hostB   = Engine.CreateHostState();
            var sampleA = NewGroupSample();

            Engine.SetGroupExpanded(sampleA, node, false, hostA.Session);

            Assert.IsFalse(Engine.GetGroupExpanded(sampleA, node, hostA.Session));
            Assert.IsTrue(Engine.GetGroupExpanded(sampleA, node, hostB.Session),
                "宿主 A 折叠了分组, 不应该影响宿主 B 看到的展开状态");
        }
    }
}
