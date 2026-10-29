using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using RD.Draw.Editor;
using RD.Draw.Editor.Rendering;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Tests.Editor
{
    /// <summary>
    /// "产树 → 解释 → 变更回放"这条链的回归基线。
    ///
    /// 用一个只记录调用、不落屏的假后端跑完整流程(而不是绕开渲染器直接断言树结构):
    /// 遍历顺序、控件翻译、交互结果回填、写回时机这几件事才测在同一处。
    /// </summary>
    public class WidgetRendererTests
    {
        //-------------------------------------- 遍历与翻译 --------------------------------------

        [Test]
        public void Render_WalksTreeInOrder()
        {
            var p = new Pipeline();
            p.Builder.BeginVertical();
            p.Builder.Label("标题", DrawTextSpec.Of());
            p.Builder.BeginHorizontal();
            p.Builder.Label("值", DrawTextSpec.Of());
            p.Builder.EndHorizontal();
            p.Builder.EndVertical();

            p.Run();

            CollectionAssert.AreEqual(new[]
            {
                "begin-v:False/0",
                "label:标题",
                "begin-h:False",
                "label:值",
                "end-h",
                "end-v",
            }, p.Driver.Calls);
        }

        [Test]
        public void Render_ScrollWidget_RunsHooksAroundBeginScroll()
        {
            // 表格的量宽/量位置就挂在这三个钩子上: 顺序错了量的就是别的容器
            var p = new Pipeline();
            var scrollPosition = Vector2.zero;
            var node = p.Builder.BeginScroll(ref scrollPosition, 100f);

            var beforeAt = -1;
            var afterAt  = -1;
            node.BeforeBegin = _ => beforeAt = p.Driver.Calls.Count;
            node.AfterBegin  = _ => afterAt  = p.Driver.Calls.Count;
            node.End         = _ => { };

            p.Builder.EndScroll();
            p.Run();

            Assert.AreEqual(0, beforeAt, "BeforeBegin 必须在 BeginScroll 之前");
            Assert.AreEqual(1, afterAt, "AfterBegin 必须在 BeginScroll 之后、画子节点之前");
            CollectionAssert.AreEqual(new[] { "begin-scroll:100", "end-scroll" }, p.Driver.Calls);
        }

        [Test]
        public void Mark_Restore_PopsContainersLeftOpenByAFailingMember()
        {
            // 建树抛异常时若不回滚, 后面的成员会挂进那个半开的容器里(整块缩进错位)
            var p = new Pipeline();
            p.Builder.BeginVertical();

            var mark = p.Builder.Mark();
            p.Builder.BeginHorizontal();
            p.Builder.BeginVertical();
            p.Builder.Restore(mark);          // 模拟"这两层容器开了一半就抛"

            p.Builder.Label("恢复后", DrawTextSpec.Of());
            p.Builder.EndVertical();

            p.Run();

            CollectionAssert.AreEqual(new[]
            {
                "begin-v:False/0",   // 外层列
                "begin-h:False",     // 半开的两层仍留在树里(空容器), 但后续节点不再挂进去
                "begin-v:False/0",
                "end-v",
                "end-h",
                "label:恢复后",      // 关键: 回到外层列的直系子节点, 而不是缩进在 row 里
                "end-v",
            }, p.Driver.Calls);
        }

        //-------------------------------------- 变更回放 --------------------------------------

        [Test]
        public void Apply_TextFieldChange_InvokesOnValue()
        {
            var p = new Pipeline();
            string received = null;
            p.Builder.TextField("名称", "旧值", 0f, value => received = value);
            p.Driver.TextFieldResult = "新值";

            p.Run();

            Assert.AreEqual("新值", received);
        }

        [Test]
        public void Apply_UnchangedTextField_DoesNotWriteOrRepaint()
        {
            var p = new Pipeline();
            string received = null;
            p.Builder.TextField("名称", "同值", 0f, value => received = value);

            p.Run();

            Assert.IsNull(received, "值没变就不该回放");
            Assert.AreEqual(0, p.Driver.RepaintCount);
        }

        [Test]
        public void Apply_ButtonClick_InvokesAction()
        {
            var p = new Pipeline();
            var clicks = 0;
            p.Builder.Button("运行", DrawButtonSpec.Of(), () => clicks++);
            p.Driver.ButtonClicked = true;

            p.Run();

            Assert.AreEqual(1, clicks);
        }

        [Test]
        public void Apply_ButtonWithoutClick_DoesNotInvokeAction()
        {
            var p = new Pipeline();
            var clicks = 0;
            p.Builder.Button("运行", DrawButtonSpec.Of(), () => clicks++);

            p.Run();

            Assert.AreEqual(0, clicks);
        }

        [Test]
        public void Apply_AppliesValuesBeforeButtons()
        {
            // 按钮可能删元素(列表/表格的 ×), 先写值才不会被删除导致的下标位移带偏
            var p = new Pipeline();
            var order = new List<string>();

            p.Builder.TextField("值", "旧值", 0f, _ => order.Add("value"));
            p.Builder.Button("删除", DrawButtonSpec.Of(), () => order.Add("button"));

            p.Driver.TextFieldResult = "新值";
            p.Driver.ButtonClicked   = true;
            p.Run();

            CollectionAssert.AreEqual(new[] { "value", "button" }, order);
        }

        [Test]
        public void Apply_FoldoutChange_InvokesCallback()
        {
            var p = new Pipeline();
            bool? received = null;
            p.Builder.Foldout(false, "分组", expanded => received = expanded);
            p.Driver.FoldoutResult = true;

            p.Run();

            Assert.AreEqual(true, received);
        }

        [Test]
        public void Apply_FoldoutUnchanged_DoesNotInvokeCallback()
        {
            var p = new Pipeline();
            bool? received = null;
            p.Builder.Foldout(true, "分组", expanded => received = expanded);

            p.Run();

            Assert.IsNull(received);
            Assert.AreEqual(0, p.Driver.RepaintCount);
        }

        [Test]
        public void Apply_IntegerFieldChange_InvokesOnValueWithLong()
        {
            var p = new Pipeline();
            long? received = null;
            p.Builder.IntegerField("页码", 1L, 0f, value => received = value);
            p.Driver.IntegerFieldResult = 7L;

            p.Run();

            Assert.AreEqual(7L, received);
        }

        //-------------------------------------- 自绘控件的边界(Phase 4) --------------------------------------

        [Test]
        public void ControlRenderer_ClaimsEveryKnownControlType()
        {
            // 认领范围必须与原生实现一致: 链上没人认领的控件, 解释器只会画它的子树 —— 等于控件消失
            var renderer = new ControlRenderer();
            var controls = new ControlWidget[]
            {
                new ButtonWidget(), new FoldoutWidget(), new TextFieldWidget(), new IntegerFieldWidget(),
                new FloatFieldWidget(), new SliderWidget(), new ToggleWidget(), new PopupWidget(),
                new EnumFlagsWidget(), new ToolbarWidget(), new ToolbarMultiWidget(),
                new ObjectFieldWidget(), new StructFieldWidget(),
            };

            foreach (var control in controls)
            {
                Assert.IsTrue(renderer.Supports(control), control.GetType().Name + " 没有被认领");
            }
        }

        [Test]
        public void ControlRenderer_WithoutDriver_FallsBackToNativePrimitives()
        {
            // 自绘关掉时必须原样退回驱动原语, 不能少画/漏画
            var p = new Pipeline();
            var renderer = new ControlRenderer();

            var toggle = WidgetPool.Rent<ToggleWidget>();
            toggle.Label = "开关";
            toggle.Value = true;

            var slider = WidgetPool.Rent<SliderWidget>();
            slider.Label = "范围";
            slider.Value = 3f;
            slider.Min   = 0f;
            slider.Max   = 10f;

            var toolbar = WidgetPool.Rent<ToolbarWidget>();
            toolbar.Options  = new[] { "A", "B", "C" };
            toolbar.Selected = 1;

            try
            {
                renderer.Render(toggle, p.Render);
                renderer.Render(slider, p.Render);
                renderer.Render(toolbar, p.Render);

                CollectionAssert.Contains(p.Driver.Calls, "toggle:开关");
                CollectionAssert.Contains(p.Driver.Calls, "slider:范围");
                CollectionAssert.Contains(p.Driver.Calls, "toolbar");
                Assert.AreEqual(true, toggle.Value);
                Assert.AreEqual(3f, slider.Value);
                Assert.AreEqual(1, toolbar.Selected);
            }
            finally
            {
                WidgetPool.Release(toggle);
                WidgetPool.Release(slider);
                WidgetPool.Release(toolbar);
            }
        }

        //-------------------------------------- 测试夹具 --------------------------------------

        /// <summary>
        /// 一次完整流程: 产树 → 选解释器落屏 → 回放变更 → 归还树。
        /// 归还放在 finally —— 与 <c>InspectorRenderer.Flush</c> 同构, 也顺便验证池的复用路径。
        /// </summary>
        private sealed class Pipeline
        {
            public readonly RecordingDriver Driver = new();
            public readonly Widget        Root     = new RootWidget();
            public readonly WidgetBuilder Builder;
            public readonly DrawContext   Ctx;
            public readonly RenderContext Render;

            public Pipeline()
            {
                Ctx     = DrawContext.Create(Driver, new HostSession(), null, null);
                Builder = new WidgetBuilder(Root);
                Render  = new RenderContext(Ctx);
            }

            public void Run()
            {
                try
                {
                    Ctx.Renderer.Render(Root, Render);
                    new ChangeApplier(Ctx, new ValueAccessor()).Apply(Builder, Render);
                }
                finally
                {
                    WidgetPool.Release(Root);
                }
            }
        }

        /// <summary>
        /// 只记录调用序列的后端: 控件把"假输入"原样返回(或按测试预设改写), 用来验证
        /// 回填 → 回放的链路; 它不做任何布局/主题的事。
        /// </summary>
        private sealed class RecordingDriver : Driver
        {
            public readonly List<string> Calls = new();
            public int RepaintCount;

            public bool   ButtonClicked;
            public bool?  FoldoutResult;
            public string TextFieldResult;
            public long?  IntegerFieldResult;

            public RecordingDriver() : base(new UnityEngine.UIElements.VisualElement()) { }

            //-------------------------------------- 作用域 --------------------------------------

            public override void BeginDraw() => Calls.Add("begin-draw");
            public override void EndDraw() => Calls.Add("end-draw");
            public override void RequestRepaint() => RepaintCount++;
            public override void BeginCheck() { }
            public override bool EndCheck() => false;

            //-------------------------------------- 容器 --------------------------------------

            public override void BeginVertical(bool boxed, float width, bool expandHeight, bool expandWidth) =>
                Calls.Add($"begin-v:{boxed}/{width}");

            public override void EndVertical() => Calls.Add("end-v");

            public override void BeginHorizontal(bool boxed) => Calls.Add($"begin-h:{boxed}");

            public override void EndHorizontal() => Calls.Add("end-h");

            public override void BeginIndent() => Calls.Add("begin-indent");
            public override void EndIndent() => Calls.Add("end-indent");

            public override void BeginScroll(ref Vector2 scroll, float maxHeight) =>
                Calls.Add($"begin-scroll:{maxHeight}");

            public override void EndScroll() => Calls.Add("end-scroll");

            public override void Space(float pixels) => Calls.Add($"space:{pixels}");
            public override void FlexibleSpace() => Calls.Add("flexible");

            public override void Separator(float thickness, Color color) => Calls.Add($"separator:{thickness}");

            //-------------------------------------- 文本 --------------------------------------

            public override void Label(string text, in DrawTextSpec spec) => Calls.Add($"label:{text}");
            public override void KeyValueLabel(string label, string value) => Calls.Add($"kv:{label}={value}");
            public override void HelpBox(string message, DrawMessageKind kind) => Calls.Add($"help:{message}");
            public override void ProgressBar(float value, string title, float height) => Calls.Add($"progress:{title}");

            //-------------------------------------- 控件 --------------------------------------

            public override bool Button(string text, in DrawButtonSpec spec)
            {
                Calls.Add($"button:{text}");
                return ButtonClicked;
            }

            public override bool Foldout(bool expanded, string label)
            {
                Calls.Add($"foldout:{label}");
                return FoldoutResult ?? expanded;
            }

            public override string TextField(string label, string value, float width)
            {
                Calls.Add($"text:{label}");
                return TextFieldResult ?? value;
            }

            public override long IntegerField(string label, long value, float width)
            {
                Calls.Add($"int:{label}");
                return IntegerFieldResult ?? value;
            }

            public override double FloatField(string label, double value, float width)
            {
                Calls.Add($"float:{label}");
                return value;
            }

            public override float Slider(string label, float value, float min, float max, float width)
            {
                Calls.Add($"slider:{label}");
                return value;
            }

            public override bool ToggleField(string label, bool value, float width)
            {
                Calls.Add($"toggle:{label}");
                return value;
            }

            public override int Popup(string label, int selected, string[] options, float width)
            {
                Calls.Add($"popup:{label}");
                return selected;
            }

            public override Enum EnumFlagsField(string label, Enum value, float width)
            {
                Calls.Add($"flags:{label}");
                return value;
            }

            public override int Toolbar(int selected, string[] options, float width)
            {
                Calls.Add("toolbar");
                return selected;
            }

            public override int ToolbarMulti(string[] options, bool[] states, float width)
            {
                Calls.Add("toolbar-multi");
                return -1;
            }

            public override UnityEngine.Object ObjectField(string label, UnityEngine.Object value, Type type, float width)
            {
                Calls.Add($"object:{label}");
                return value;
            }

            public override object StructField(string label, object value, Type type, float width)
            {
                Calls.Add($"struct:{label}");
                return value;
            }
        }
    }
}
