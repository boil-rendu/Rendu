using NUnit.Framework;
using UnityEngine;
using RD.Draw.Editor;

namespace RD.Draw.Tests.Editor
{
    /// <summary>
    /// 对象池(设计决策 4)的回归基线。
    ///
    /// 池化的风险全在"复用把上一帧的状态带出来了", 所以这里钉的不是"能复用"本身, 而是:
    ///   - 借出的节点每个字段都是默认值, 包括解释器回填的输出字段(拖拽结果 / 列宽增量 / 点中的下标);
    ///   - 归还时子树被清空、钩子闭包被断开 —— 池里的节点不持有宿主对象与业务字符串;
    ///   - 同一棵树归还两次是无操作(否则两次借出会指向同一个节点)。
    /// </summary>
    public class WidgetPoolTests
    {
        //-------------------------------------- 字段清空 --------------------------------------

        [Test]
        public void Release_ScrollWidget_ClearsStateAndHooks()
        {
            var root   = WidgetPool.Rent<ColumnWidget>();
            var scroll = WidgetPool.Rent<ScrollWidget>();
            scroll.Scroll      = new Vector2(3f, 4f);
            scroll.MaxHeight   = 120f;
            scroll.BeforeBegin = _ => { };
            scroll.AfterBegin  = _ => { };
            scroll.End         = _ => { };
            root.Add(scroll);

            WidgetPool.Release(root);

            var rented = WidgetPool.Rent<ScrollWidget>();
            Assert.AreSame(scroll, rented, "同类型栈应当复用同一实例");
            Assert.AreEqual(default(Vector2), rented.Scroll);
            Assert.AreEqual(0f, rented.MaxHeight);
            Assert.IsNull(rented.BeforeBegin, "钩子闭包会捕获宿主对象, 必须断开");
            Assert.IsNull(rented.AfterBegin);
            Assert.IsNull(rented.End);
        }

        [Test]
        public void Release_StyledScope_ClearsAllThreeOptionals()
        {
            // 三个字段互斥(每次只设一个), 复用时不把另外两个清掉就会把颜色/禁用态漏给下一个成员
            var root  = WidgetPool.Rent<ColumnWidget>();
            var scope = WidgetPool.Rent<StyledScopeWidget>();
            scope.Color      = Color.red;
            scope.Enabled    = false;
            scope.LabelWidth = 42f;
            root.Add(scope);

            WidgetPool.Release(root);

            var rented = WidgetPool.Rent<StyledScopeWidget>();
            Assert.AreSame(scope, rented);
            Assert.IsNull(rented.Color);
            Assert.IsNull(rented.Enabled);
            Assert.IsNull(rented.LabelWidth);
        }

        [Test]
        public void Release_ToolbarMulti_ResetsClickedIndex()
        {
            var root = WidgetPool.Rent<ColumnWidget>();
            var node = WidgetPool.Rent<ToolbarMultiWidget>();
            node.States       = new[] { true, false };
            node.Options      = new[] { "A", "B" };
            node.ClickedIndex = 1;
            node.Width        = 80f;
            root.Add(node);

            WidgetPool.Release(root);

            var rented = WidgetPool.Rent<ToolbarMultiWidget>();
            Assert.AreSame(node, rented);
            Assert.AreEqual(-1, rented.ClickedIndex, "复用时必须回到「未点击」");
            Assert.IsNull(rented.States);
            Assert.IsNull(rented.Options);
            Assert.AreEqual(0f, rented.Width);
        }

        [Test]
        public void Release_ReorderWidget_ResetsResultToNone()
        {
            // default(ReorderDrag).Source 是 0 而不是 -1 —— 忘了重置会被当成"正在拖第 0 条"
            var root = WidgetPool.Rent<ColumnWidget>();
            var node = WidgetPool.Rent<ReorderWidget>();
            node.ListId = 9;
            node.Result = new ReorderDrag(2, 5, true);
            root.Add(node);

            WidgetPool.Release(root);

            var rented = WidgetPool.Rent<ReorderWidget>();
            Assert.AreSame(node, rented);
            Assert.AreEqual(0, rented.ListId);
            Assert.IsFalse(rented.Result.Active);
            Assert.IsFalse(rented.Result.Dropped);
            Assert.AreEqual(-1, rented.Result.Source);
            Assert.AreEqual(-1, rented.Result.Target);
        }

        [Test]
        public void Release_ColumnResizer_ResetsDelta()
        {
            var root = WidgetPool.Rent<ColumnWidget>();
            var node = WidgetPool.Rent<ColumnResizerWidget>();
            node.Height = 20f;
            node.Delta  = 7f;
            root.Add(node);

            WidgetPool.Release(root);

            var rented = WidgetPool.Rent<ColumnResizerWidget>();
            Assert.AreSame(node, rented);
            Assert.AreEqual(0f, rented.Height);
            Assert.AreEqual(0f, rented.Delta, "上一帧的拖拽增量不能留到下一帧");
        }

        //-------------------------------------- 子树与重复归还 --------------------------------------

        [Test]
        public void Release_ClearsChildren()
        {
            var root = WidgetPool.Rent<ColumnWidget>();
            root.Add(WidgetPool.Rent<LabelWidget>());
            root.Add(WidgetPool.Rent<LabelWidget>());

            WidgetPool.Release(root);

            var rented = WidgetPool.Rent<ColumnWidget>();
            Assert.AreSame(root, rented);
            Assert.AreEqual(0, rented.ChildrenOrNull?.Count ?? 0);
        }

        [Test]
        public void Release_LongChildList_DropsCapacity()
        {
            // 一张几百行的列表画完后, 容器不该带着几百槽的数组回池(512 个容器就是几十万个引用)
            var root = WidgetPool.Rent<ColumnWidget>();
            for (var i = 0; i < 100; i++)
            {
                var label = WidgetPool.Rent<LabelWidget>();
                label.Text = "row " + i;
                root.Add(label);
            }

            WidgetPool.Release(root);

            var rented = WidgetPool.Rent<ColumnWidget>();
            Assert.LessOrEqual(rented.ChildrenOrNull.Capacity, 64);
        }

        [Test]
        public void Release_Twice_DoesNotAliasNodes()
        {
            var root = WidgetPool.Rent<ColumnWidget>();
            root.Add(WidgetPool.Rent<LabelWidget>());

            WidgetPool.Release(root);
            WidgetPool.Release(root);   // 第二次必须是无操作

            var first  = WidgetPool.Rent<LabelWidget>();
            var second = WidgetPool.Rent<LabelWidget>();

            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.AreNotSame(first, second, "同一实例被塞进池两次时, 两次借出会拿到同一个节点");

            WidgetPool.Release(first);
            WidgetPool.Release(second);
        }
    }
}
