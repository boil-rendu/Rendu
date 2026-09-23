using System.Collections.Generic;
using RD.Inspector.Editor;
using NUnit.Framework;

namespace RD.Inspector.Tests.Editor
{
    /// <summary>
    /// InspectorExpressionEvaluator 的行为基线。
    /// 这里固化的多数是"当前实现的事实契约", 而非"理想语义"——
    /// 其中若干条(不短路求值、Truthy(0) 为真、字面量一律为 double、字符串拼接的触发条件)
    /// 属于容易踩坑的隐式规则。改动求值器前, 应先让这里变红。
    /// </summary>
    public class InspectorExpressionEvaluatorTests
    {
        private enum Tag
        {
            Red,
            Green,
            Blue,
        }

        private class Inner
        {
            public int Depth = 2;
            public string Label = "inner";
            public List<int> Items = new List<int> { 4, 5, 6 };
        }

        private class Widget
        {
            public int Count = 3;
            public string Name = "widget";
            public bool Active = true;
            public Tag Tag = Tag.Green;
            public readonly Inner Child = new Inner();

            public int Calls;

            public List<int> Items = new List<int> { 10, 20, 30 };
            public int[] Numbers = { 7, 8, 9 };
            public Dictionary<string, int> Map = new Dictionary<string, int> { { "a", 1 } };
            public string Text = "abc";

            public List<int> GetScaled(int factor)
            {
                return new List<int> { factor, factor * 2 };
            }

            public double Add(double a, double b)
            {
                return a + b;
            }

            public int LastA;
            public int LastB;
            public Tag LastTag;

            /// <summary>形参是 int —— 表达式字面量是 double, 必须收窄才能调用。</summary>
            public int AddInt(int a, int b)
            {
                this.LastA = a;
                this.LastB = b;
                return a + b;
            }

            public string Describe(Tag tag)
            {
                this.LastTag = tag;
                return tag.ToString();
            }

            public string Greet(string who)
            {
                return "hi " + who;
            }

            public bool Mark()
            {
                this.Calls++;
                return true;
            }
        }

        /// <summary>同名重载: 表达式调用必须按形参个数挑, 而不是抛 AmbiguousMatchException。</summary>
        private class Overloaded
        {
            public int Pick()
            {
                return 1;
            }

            public int Pick(int a)
            {
                return a;
            }

            public int Pick(int a, int b)
            {
                return a + b;
            }
        }

        private class HiddenNameBase
        {
            public string Name
            {
                get { return "base"; }
            }
        }

        /// <summary>用 new 隐藏基类同名属性: 一次查回来的是成员数组(顺序未定义), 必须取最派生的那个。</summary>
        private class HiddenNameDerived : HiddenNameBase
        {
            public new string Name
            {
                get { return "derived"; }
            }
        }

        /// <summary>自定义索引器, 用来覆盖"索引器元数据缓存"这条路径。</summary>
        private class Indexed
        {
            public int Offset;

            public int this[int index]
            {
                get { return index + Offset; }
            }
        }

        private static object Eval(string expression)
        {
            return InspectorExpressionEvaluator.Evaluate(expression, null, null, null);
        }

        private static object EvalOn(string expression, object target)
        {
            return InspectorExpressionEvaluator.Evaluate(expression, target, null, null);
        }

        //-------------------------------------- 入口判定 --------------------------------------

        [Test]
        public void IsExpression_OnlyAcceptsAtPrefix()
        {
            Assert.IsTrue(InspectorExpressionEvaluator.IsExpression("@1"));
            Assert.IsFalse(InspectorExpressionEvaluator.IsExpression("1"));
            Assert.IsFalse(InspectorExpressionEvaluator.IsExpression(""));
            Assert.IsFalse(InspectorExpressionEvaluator.IsExpression(null));
        }

        [Test]
        public void EmptyOrBareAt_ReturnsNull()
        {
            Assert.IsNull(Eval(null));
            Assert.IsNull(Eval(""));
            Assert.IsNull(Eval("@"));
        }

        //-------------------------------------- 字面量 --------------------------------------

        [Test]
        public void NumberLiteral_AlwaysEvaluatesToDouble()
        {
            var value = Eval("@1");
            Assert.IsInstanceOf<double>(value);
            Assert.AreEqual(1d, value);
            Assert.AreEqual(1.5d, Eval("@1.5"));
            Assert.AreEqual(1000d, Eval("@1e3"));
        }

        [Test]
        public void StringLiteral_SupportsBothQuoteStyles()
        {
            Assert.AreEqual("x", Eval("@\"x\""));
            Assert.AreEqual("y", Eval("@'y'"));
        }

        [Test]
        public void StringLiteral_TranslatesEscapes()
        {
            Assert.AreEqual("a\nb", Eval("@'a\\nb'"));
            Assert.AreEqual("a\tb", Eval("@'a\\tb'"));
            Assert.AreEqual("a\rb", Eval("@'a\\rb'"));
        }

        [Test]
        public void BoolAndNullLiterals()
        {
            Assert.AreEqual(true, Eval("@true"));
            Assert.AreEqual(false, Eval("@false"));
            Assert.IsNull(Eval("@null"));
        }

        //-------------------------------------- 算术 --------------------------------------

        [Test]
        public void Arithmetic_HonoursPrecedence()
        {
            Assert.AreEqual(7d, Eval("@1 + 2 * 3"));
            Assert.AreEqual(9d, Eval("@(1 + 2) * 3"));
            Assert.AreEqual(-4d, Eval("@-(1 + 3)"));
        }

        [Test]
        public void Arithmetic_DivisionIsFloatingPoint()
        {
            Assert.AreEqual(1.5d, Eval("@3 / 2"));
        }

        [Test]
        public void Arithmetic_ModuloByZero_ReturnsZeroInsteadOfThrowing()
        {
            Assert.AreEqual(0d, Eval("@5 % 0"));
        }

        [Test]
        public void Arithmetic_NonNumericOperands_ReturnNull()
        {
            Assert.IsNull(Eval("@'a' * 2"));
            Assert.IsNull(Eval("@true - 1"));
        }

        //-------------------------------------- 比较与逻辑 --------------------------------------

        [Test]
        public void Comparison_IsOrdinalForStrings()
        {
            Assert.AreEqual(true, Eval("@'a' < 'b'"));
            Assert.AreEqual(true, Eval("@'B' < 'a'"), "字符串比较是 Ordinal, 区分大小写");
            Assert.AreEqual(true, Eval("@2 <= 2"));
            Assert.AreEqual(false, Eval("@2 < 2"));
        }

        [Test]
        public void Equality_UsesNumericTolerance()
        {
            Assert.AreEqual(true, Eval("@0.1 + 0.2 == 0.3"), "浮点比较带随量级放大的相对容差");
            Assert.AreEqual(true, Eval("@1 != 2"));
            Assert.AreEqual(true, Eval("@null == null"), "null 与 null 相等, 结果是 bool 而非 null");
        }

        [Test]
        public void Equality_ComparesEnumMemberWithString()
        {
            var widget = new Widget();
            Assert.AreEqual(true, EvalOn("@Tag == 'Green'", widget));
            Assert.AreEqual(false, EvalOn("@Tag == 'Red'", widget));
        }

        [Test]
        public void Logic_OperatorsProduceBooleans()
        {
            Assert.AreEqual(true, Eval("@true && true"));
            Assert.AreEqual(true, Eval("@false || true"));
            Assert.AreEqual(true, Eval("@!false"));
            Assert.AreEqual(false, Eval("@!true"));
        }

        [Test]
        public void Logic_IsNotShortCircuiting()
        {
            var widget = new Widget();
            EvalOn("@false && Mark()", widget);
            Assert.AreEqual(1, widget.Calls,
                "当前实现无条件求值右操作数(不短路)。若改为短路求值, 此断言必须同步更改。");
        }

        //-------------------------------------- 三元 --------------------------------------

        [Test]
        public void Ternary_SelectsBranch()
        {
            Assert.AreEqual(2d, Eval("@true ? 2 : 3"));
            Assert.AreEqual(3d, Eval("@false ? 2 : 3"));
            Assert.AreEqual(2d, Eval("@1 < 2 ? 2 : 3"));
        }

        //-------------------------------------- 拼接 --------------------------------------

        [Test]
        public void Concat_TriggersOnlyWhenAnOperandIsString()
        {
            Assert.AreEqual("a1", Eval("@'a' + 1"));
            Assert.AreEqual("1a", Eval("@1 + 'a'"));
            Assert.AreEqual(3d, Eval("@1 + 2"), "两侧都为数值时走数值加法");
        }

        //-------------------------------------- 根与链 --------------------------------------

        [Test]
        public void Root_ThisAndDollarRootResolveToTarget()
        {
            var widget = new Widget();
            Assert.AreSame(widget, EvalOn("@this", widget));
            Assert.AreSame(widget, EvalOn("@$root", widget));
        }

        [Test]
        public void Root_DollarValueResolvesToSuppliedValue()
        {
            Assert.AreEqual(42, InspectorExpressionEvaluator.Evaluate("@$value", null, 42, null));
            Assert.AreEqual(43d, InspectorExpressionEvaluator.Evaluate("@$value + 1", null, 42, null));
        }

        [Test]
        public void Chain_ReadsFieldsAndProperties()
        {
            var widget = new Widget();
            Assert.AreEqual(3, EvalOn("@Count", widget));
            Assert.AreEqual(3, EvalOn("@this.Count", widget));
            Assert.AreEqual("widget", EvalOn("@this.Name", widget));
            Assert.AreEqual(true, EvalOn("@this.Active", widget));
        }

        [Test]
        public void Chain_DescendsIntoNestedObject()
        {
            var widget = new Widget();
            Assert.AreEqual(2, EvalOn("@this.Child.Depth", widget));
            Assert.AreEqual("inner", EvalOn("@Child.Label", widget));
        }

        // 注意: 表达式里的数字字面量一律是 double, 而反射 binder 默认只做拓宽转换, 于是
        // @Foo(1, 2) 打到 Foo(int, int) 在 Mono/IL2CPP 与 CoreCLR 上的结果并不一致。
        // InvokeRaw 之前用 CoerceArgs 显式收窄(见文件末尾的"实参收窄"一节), 这里固化能精确匹配的情形。
        [Test]
        public void Chain_InvokesMethodWithArguments()
        {
            var widget = new Widget();
            Assert.AreEqual(3d, EvalOn("@this.Add(1, 2)", widget));
            Assert.AreEqual("hi bob", EvalOn("@this.Greet('bob')", widget));
        }

        [Test]
        public void Call_InvokesUnboundInstanceMethod()
        {
            var widget = new Widget();
            Assert.AreEqual(5d, EvalOn("@Add(2, 3)", widget));
        }

        [Test]
        public void Chain_OnMissingMember_ReturnsNull()
        {
            Assert.IsNull(EvalOn("@this.Missing.Whatever", new Widget()));
        }

        //-------------------------------------- $property --------------------------------------

        [Test]
        public void Property_StateIsReadable()
        {
            var state = new InspectorPropertyState { Expanded = false, Visible = true, Enabled = false };
            Assert.AreEqual(false, InspectorExpressionEvaluator.Evaluate("@$property.State.Expanded", null, null, state));
            Assert.AreEqual(true, InspectorExpressionEvaluator.Evaluate("@$property.State.Visible", null, null, state));
            Assert.AreEqual(false, InspectorExpressionEvaluator.Evaluate("@$property.State.Enabled", null, null, state));
        }

        [Test]
        public void Property_StateIsWritable()
        {
            var state = new InspectorPropertyState { Expanded = true };
            InspectorExpressionEvaluator.Evaluate("@$property.State.Expanded = false", null, null, state);
            Assert.IsFalse(state.Expanded, "赋值表达式必须写回宿主传进来的同一个 State 实例");
        }

        [Test]
        public void Property_CanDriveConditionExpression()
        {
            var state = new InspectorPropertyState { Selected = false };
            Assert.AreEqual(false, InspectorExpressionEvaluator.EvaluateBool("@$property.State.Selected", null, null, state));
            state.Selected = true;
            Assert.AreEqual(true, InspectorExpressionEvaluator.EvaluateBool("@$property.State.Selected", null, null, state));
        }

        //-------------------------------------- 失败路径 --------------------------------------

        [Test]
        public void MalformedExpression_ReturnsNullInsteadOfThrowing()
        {
            Assert.IsNull(Eval("@1 +"));
            Assert.IsNull(Eval("@1 ;"));
            Assert.IsNull(Eval("@'unterminated"));
            Assert.IsNull(Eval("@[1]"));
        }

        //-------------------------------------- 解析缓存与解析规则 --------------------------------------
        // 表达式每帧都会重新判定, 所以词法/语法分析与"成员/方法解析"都被缓存。
        // 这一节钉住的是"缓存的是元数据, 不是取值结果", 以及解析规则本身(按形参个数、取最派生的成员)。

        [Test]
        public void Evaluate_RepeatedEvaluation_ReflectsCurrentValue()
        {
            var widget = new Widget();

            Assert.AreEqual(3, EvalOn("@Count", widget));
            widget.Count = 4;
            Assert.AreEqual(4, EvalOn("@Count", widget), "缓存的是 AST 与成员元数据, 每次仍要重新读值");

            Assert.AreEqual(true, EvalOn("@Active", widget));
            widget.Active = false;
            Assert.AreEqual(false, EvalOn("@Active", widget));
        }

        [Test]
        public void Assignment_SameExpressionTwice_WritesBothTimes()
        {
            var state = new InspectorPropertyState { Expanded = false };

            InspectorExpressionEvaluator.Evaluate("@$property.State.Expanded = true", null, null, state);
            Assert.IsTrue(state.Expanded);

            InspectorExpressionEvaluator.Evaluate("@$property.State.Expanded = false", null, null, state);
            Assert.IsFalse(state.Expanded, "第二次求值命中的是缓存的 AST 与成员元数据, 但必须照样写回");
        }

        [Test]
        public void Call_OverloadsResolveByArity()
        {
            var target = new Overloaded();

            Assert.AreEqual(1, EvalOn("@Pick()", target));
            Assert.AreEqual(7, EvalOn("@Pick(7)", target));
            Assert.AreEqual(3, EvalOn("@Pick(1, 2)", target), "同名重载必须按形参个数挑, 不能抛歧义异常");
        }

        [Test]
        public void Chain_HiddenMemberWithNewKeyword_ReadsMostDerived()
        {
            var target = new HiddenNameDerived();

            Assert.AreEqual("derived", EvalOn("@Name", target),
                "被 new 隐藏的同名成员必须取最派生的那个, 而不是数组里碰巧排在前面的那个");
            Assert.AreEqual("derived", EvalOn("@this.Name", target));
        }

        [Test]
        public void Index_CustomIndexer_RepeatedAccessReadsCurrentState()
        {
            var target = new Indexed { Offset = 1 };

            Assert.AreEqual(1, EvalOn("@this[0]", target));
            target.Offset = 5;
            Assert.AreEqual(5, EvalOn("@this[0]", target), "缓存的是索引器元数据, 不是取值结果");
        }

        //-------------------------------------- 真值语义 --------------------------------------

        [Test]
        public void Truthy_Semantics()
        {
            Assert.IsFalse(InspectorExpressionEvaluator.Truthy(null));
            Assert.IsFalse(InspectorExpressionEvaluator.Truthy(false));
            Assert.IsTrue(InspectorExpressionEvaluator.Truthy(true));
            Assert.IsFalse(InspectorExpressionEvaluator.Truthy(""));
            Assert.IsTrue(InspectorExpressionEvaluator.Truthy("x"));
            Assert.IsFalse(InspectorExpressionEvaluator.Truthy(new List<int>()));
            Assert.IsTrue(InspectorExpressionEvaluator.Truthy(new List<int> { 1 }));
            Assert.IsTrue(InspectorExpressionEvaluator.Truthy(0), "数值 0 按当前实现为真");
        }

        //-------------------------------------- 实参收窄 --------------------------------------

        [Test]
        public void MethodCall_NarrowsDoubleLiteralToIntParameters()
        {
            var widget = new Widget();

            Assert.AreEqual(3, EvalOn("@AddInt(1, 2)", widget));
            Assert.AreEqual(1, widget.LastA, "字面量 1 应被收窄成 int 1 而不是丢失或报错");
            Assert.AreEqual(2, widget.LastB);
        }

        [Test]
        public void MethodCall_NarrowsDoubleLiteralToEnumParameter()
        {
            var widget = new Widget();

            Assert.AreEqual("Green", EvalOn("@Describe(1)", widget),
                "枚举形参应接受表达式里的整数字面量");
            Assert.AreEqual(Tag.Green, widget.LastTag);
        }

        [Test]
        public void MethodCall_DoubleParameterIsUnaffected()
        {
            var widget = new Widget();

            Assert.AreEqual(3d, EvalOn("@Add(1, 2)", widget), "double 形参本来就能直接接收字面量");
        }

        //-------------------------------------- 索引器 --------------------------------------

        [Test]
        public void Index_ReadsArrayListAndString()
        {
            var widget = new Widget();

            Assert.AreEqual(10, EvalOn("@Items[0]", widget));
            Assert.AreEqual(30, EvalOn("@Items[2]", widget));
            Assert.AreEqual(8, EvalOn("@Numbers[1]", widget));
            Assert.AreEqual('b', EvalOn("@Text[1]", widget));
        }

        [Test]
        public void Index_ReadsDictionaryByKey()
        {
            var widget = new Widget();

            Assert.AreEqual(1, EvalOn("@Map['a']", widget));
        }

        [Test]
        public void Index_OutOfRangeOrMissingKey_ReturnsNull()
        {
            var widget = new Widget();

            Assert.IsNull(EvalOn("@Items[99]", widget), "越界返回 null 而不是抛异常");
            Assert.IsNull(EvalOn("@Items[-1]", widget));
            Assert.IsNull(EvalOn("@Map['missing']", widget));
        }

        [Test]
        public void Index_FollowsMemberChain()
        {
            var widget = new Widget();

            Assert.AreEqual(5, EvalOn("@Child.Items[1]", widget));
        }

        [Test]
        public void Index_AppliesToCallResult()
        {
            var widget = new Widget();

            Assert.AreEqual(4, EvalOn("@GetScaled(2)[1]", widget),
                "调用结果后面也能接索引, 而不是被静默截断成 @GetScaled(2)");
        }

        [Test]
        public void Index_UnclosedBracket_IsRejected()
        {
            var widget = new Widget();

            Assert.IsNull(EvalOn("@Items[0", widget), "半个表达式应整体失败, 而不是退化成整个 @Items");
        }

        [Test]
        public void DanglingDot_IsRejected()
        {
            var widget = new Widget();

            Assert.IsNull(EvalOn("@Child.", widget), "悬空的点号同样不该退化成 @Child");
        }
    }
}
