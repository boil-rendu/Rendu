using RD.EditorKit;
using RD.EditorKit.Editor;
using NUnit.Framework;

namespace RD.EditorKit.Tests.Editor
{
    /// <summary>
    /// 条件显隐 / 禁用 的回归基线（[ShowIf] / [HideIf] / [EnableIf] / [DisableIf] /
    /// [HideInPlayMode] / [HideInEditorMode]）。
    ///
    /// 这组规则决定"什么能看见、什么可编辑"，但此前完全无法自动化验证：
    /// CheckVisibility 直接读 Application.isPlaying（Unity ECall），脱离编辑器就跑不了。
    /// 现在运行模式由调用方传入，两种模式都能被钉住。
    ///
    /// 固化的语义要点：
    /// - 不带 value 的条件按真值判断（Truthy），带 value 的按相等判断（ValuesEqual）；
    /// - 数值相等走相对容差（InspectorValue.NumericTolerance），float 字段与 double 字面量也能对上；
    /// - ShowIf / EnableIf 要求条件成立，HideIf / DisableIf 要求条件不成立；
    /// - 多个同名条件之间是"与"——任一不满足即拒绝（EnableIf / DisableIf 支持 AllowMultiple）；
    /// - 条件既能写成员名，也能写 @ 表达式。
    ///
    /// 未覆盖：Truthy 对 UnityEngine.Object 的重载判空（那需要 Unity 运行时）。
    /// </summary>
    public class InspectorEngineConditionTests
    {
        //-------------------------------------- 夹具 --------------------------------------

        private class ConditionSample
        {
            public bool Flag = true;
            public int Count = 3;

            [ShowIf("Flag")] public int ShowByFlag;
            [ShowIf("Count", 3)] public int ShowWhenCountIs3;
            [ShowIf("@Count > 2")] public int ShowWhenCountGt2;

            [HideIf("Flag")] public int HideByFlag;
            [HideIf("Count", 3)] public int HideWhenCountIs3;
            [HideIf("@Count > 2")] public int HideWhenCountGt2;

            [EnableIf("Flag")] public int EnableByFlag;
            [DisableIf("@Count > 2")] public int DisableWhenCountGt2;

            [EnableIf("Flag")]
            [EnableIf("@Count > 2")]
            public int EnableByBoth;

            [HideInPlayMode] public int HiddenInPlay;
            [HideInEditorMode] public int HiddenInEdit;

            public int Unconditional;
        }

        /// <summary>
        /// 数值比较夹具: 字段是 float, 而特性参数是 C# 字面量(double)。
        /// </summary>
        private class NumericConditionSample
        {
            public float Speed = 0.1f;
            public double Ratio = 0.3;

            [ShowIf("Speed", 0.1)] public int ShowWhenSpeedIsPointOne;
            [HideIf("Speed", 0.1)] public int HideWhenSpeedIsPointOne;
            [ShowIf("Ratio", 0.1 + 0.2)] public int ShowWhenRatioIsPointThree;
            [ShowIf("Speed", 0.2)] public int ShowWhenSpeedIsPointTwo;
        }

        private class HiddenConditionBase
        {
            public bool Flag
            {
                get { return false; }
            }
        }

        /// <summary>
        /// 用 new 隐藏基类同名属性。
        /// 条件里写成员名(或 @ 表达式里的裸成员名)时, 解析必须取最派生的那个:
        /// 一次 GetMember/GetProperty 查到的是成员数组(顺序未定义), 取 [0] 是在赌运气。
        /// 这里钉住的是"取最派生"这条确定规则。
        /// </summary>
        private class HiddenConditionSample : HiddenConditionBase
        {
            public new bool Flag
            {
                get { return true; }
            }

            [ShowIf("Flag")] public int ShownWhenDerivedFlagIsTrue;
            [ShowIf("@Flag == false")] public int ShownWhenFlagIsFalse;
        }

        //-------------------------------------- 辅助 --------------------------------------

        private static InspectorEngine.CachedMember Member(object target, string name)
        {
            foreach (var m in InspectorEngine.GetMembers(target.GetType()))
            {
                if (m.RawName == name) return m;
            }

            Assert.Fail($"夹具 {target.GetType().Name} 上找不到成员 {name}");
            return null;
        }

        private static bool Visible(object target, string name, bool isPlaying = false)
        {
            return InspectorEngine.CheckVisibility(Member(target, name), target, isPlaying);
        }

        private static bool Disabled(object target, string name)
        {
            return InspectorEngine.CheckDisabled(Member(target, name), target);
        }

        //-------------------------------------- ShowIf --------------------------------------

        [Test]
        public void ShowIf_WithoutValue_UsesTruthiness()
        {
            var sample = new ConditionSample { Flag = true };
            Assert.IsTrue(Visible(sample, "ShowByFlag"));

            sample.Flag = false;
            Assert.IsFalse(Visible(sample, "ShowByFlag"));
        }

        [Test]
        public void ShowIf_WithValue_ComparesEquality()
        {
            var sample = new ConditionSample { Count = 3 };
            Assert.IsTrue(Visible(sample, "ShowWhenCountIs3"));

            sample.Count = 4;
            Assert.IsFalse(Visible(sample, "ShowWhenCountIs3"));
        }

        [Test]
        public void ShowIf_AcceptsExpression()
        {
            var sample = new ConditionSample { Count = 3 };
            Assert.IsTrue(Visible(sample, "ShowWhenCountGt2"));

            sample.Count = 1;
            Assert.IsFalse(Visible(sample, "ShowWhenCountGt2"));
        }

        //-------------------------------------- 数值比较容差 --------------------------------------

        [Test]
        public void ShowIf_FloatFieldComparedWithDoubleLiteral_Matches()
        {
            // 字段是 float、字面量是 double: 0.1f 转 double 是 0.100000001490116…, 与 0.1 差约 1.5e-9。
            // 这正好躲过"绝对容差 1e-9"与 double.Epsilon(≈0), 是 [ShowIf("Speed", 0.1)] 在 float 字段上
            // 会诡异地永远不成立的根因 —— 必须靠随量级放大的相对容差兜住。
            var sample = new NumericConditionSample();

            Assert.IsTrue(Visible(sample, "ShowWhenSpeedIsPointOne"));
            Assert.IsFalse(Visible(sample, "HideWhenSpeedIsPointOne"), "相等即命中 HideIf");
        }

        [Test]
        public void ShowIf_NumericDifferenceBeyondTolerance_IsNotEqual()
        {
            var sample = new NumericConditionSample { Speed = 0.2f };

            Assert.IsFalse(Visible(sample, "ShowWhenSpeedIsPointOne"), "0.2f 与 0.1 相差 0.1, 远超容差");
            Assert.IsTrue(Visible(sample, "ShowWhenSpeedIsPointTwo"));
        }

        [Test]
        public void ShowIf_DoubleAccumulationError_StillMatches()
        {
            var sample = new NumericConditionSample();

            Assert.IsTrue(Visible(sample, "ShowWhenRatioIsPointThree"), "0.3 与 0.1+0.2 只差 5.5e-17");
        }

        //-------------------------------------- 成员名解析规则 --------------------------------------

        [Test]
        public void ShowIf_HiddenPropertyWithNewKeyword_ReadsMostDerived()
        {
            var sample = new HiddenConditionSample();

            Assert.IsTrue(Visible(sample, "ShownWhenDerivedFlagIsTrue"),
                "被 new 隐藏的成员必须取最派生的那个, 否则会读到一个不可预期的基类值");
            Assert.IsFalse(Visible(sample, "ShownWhenFlagIsFalse"), "裸成员名与 @ 表达式应解析到同一个成员");
        }

        //-------------------------------------- HideIf --------------------------------------

        [Test]
        public void HideIf_InvertsTruthiness()
        {
            var sample = new ConditionSample { Flag = true };
            Assert.IsFalse(Visible(sample, "HideByFlag"), "条件成立时应隐藏");

            sample.Flag = false;
            Assert.IsTrue(Visible(sample, "HideByFlag"));
        }

        [Test]
        public void HideIf_WithValue_ComparesEquality()
        {
            var sample = new ConditionSample { Count = 3 };
            Assert.IsFalse(Visible(sample, "HideWhenCountIs3"));

            sample.Count = 4;
            Assert.IsTrue(Visible(sample, "HideWhenCountIs3"));
        }

        [Test]
        public void HideIf_AcceptsExpression()
        {
            var sample = new ConditionSample { Count = 3 };
            Assert.IsFalse(Visible(sample, "HideWhenCountGt2"));

            sample.Count = 1;
            Assert.IsTrue(Visible(sample, "HideWhenCountGt2"));
        }

        //-------------------------------------- 运行模式 --------------------------------------

        [Test]
        public void HideInPlayMode_OnlyHidesWhilePlaying()
        {
            var sample = new ConditionSample();

            Assert.IsTrue(Visible(sample, "HiddenInPlay", isPlaying: false));
            Assert.IsFalse(Visible(sample, "HiddenInPlay", isPlaying: true));
        }

        [Test]
        public void HideInEditorMode_OnlyHidesOutsidePlayMode()
        {
            var sample = new ConditionSample();

            Assert.IsTrue(Visible(sample, "HiddenInEdit", isPlaying: true));
            Assert.IsFalse(Visible(sample, "HiddenInEdit", isPlaying: false));
        }

        [Test]
        public void MemberWithoutCondition_IsAlwaysVisible()
        {
            var sample = new ConditionSample();

            Assert.IsTrue(Visible(sample, "Unconditional", isPlaying: false));
            Assert.IsTrue(Visible(sample, "Unconditional", isPlaying: true));
        }

        //-------------------------------------- EnableIf / DisableIf --------------------------------------

        [Test]
        public void EnableIf_RequiresConditionToHold()
        {
            var sample = new ConditionSample { Flag = true };
            Assert.IsFalse(Disabled(sample, "EnableByFlag"));

            sample.Flag = false;
            Assert.IsTrue(Disabled(sample, "EnableByFlag"), "EnableIf 条件不成立即禁用");
        }

        [Test]
        public void DisableIf_DisablesWhenConditionHolds()
        {
            var sample = new ConditionSample { Count = 3 };
            Assert.IsTrue(Disabled(sample, "DisableWhenCountGt2"));

            sample.Count = 1;
            Assert.IsFalse(Disabled(sample, "DisableWhenCountGt2"));
        }

        [Test]
        public void EnableIf_AllConditionsMustHold()
        {
            var sample = new ConditionSample { Flag = true, Count = 3 };
            Assert.IsFalse(Disabled(sample, "EnableByBoth"));

            sample.Count = 1;
            Assert.IsTrue(Disabled(sample, "EnableByBoth"), "任一 EnableIf 不满足即禁用");
        }

        [Test]
        public void MemberWithoutCondition_IsEnabledByDefault()
        {
            var sample = new ConditionSample();
            Assert.IsFalse(Disabled(sample, "Unconditional"));
        }

        //-------------------------------------- 批注条件显隐 (VisibleIfMemberName) --------------------------------------

        [Test]
        public void AnnotationVisible_EmptyCondition_IsAlwaysVisible()
        {
            var sample = new ConditionSample();
            Assert.IsTrue(InspectorEngine.IsAnnotationVisible(null, sample), "没写条件 = 无条件显示");
            Assert.IsTrue(InspectorEngine.IsAnnotationVisible("", sample));
        }

        [Test]
        public void AnnotationVisible_AcceptsMemberName()
        {
            var sample = new ConditionSample { Flag = true };
            Assert.IsTrue(InspectorEngine.IsAnnotationVisible("Flag", sample));

            sample.Flag = false;
            Assert.IsFalse(InspectorEngine.IsAnnotationVisible("Flag", sample));
        }

        [Test]
        public void AnnotationVisible_AcceptsExpression()
        {
            var sample = new ConditionSample { Count = 3 };
            Assert.IsTrue(InspectorEngine.IsAnnotationVisible("@Count > 2", sample));

            sample.Count = 1;
            Assert.IsFalse(InspectorEngine.IsAnnotationVisible("@Count > 2", sample));
        }
    }
}
