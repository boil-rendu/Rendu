using NUnit.Framework;
using RD.Draw.Editor;

namespace RD.Draw.Tests.Editor
{
    /// <summary>
    /// <see cref="LayoutPolicy"/> 的回归基线: 布局规则现在只有一份纯函数。
    ///
    /// 这些用例钉住的是"规则"本身(定宽 / 撑满 / 上限宽 / 允许收缩 / 边距), 而不是某个后端
    /// 的翻译结果 —— 翻译层( style.*)不该重写这些决策。
    /// </summary>
    public class LayoutPolicyTests
    {
        //-------------------------------------- 字段 --------------------------------------

        [Test]
        public void ForField_PositiveWidth_IsFixedAndLocked()
        {
            var hint = LayoutPolicy.ForField(120f);

            Assert.AreEqual(120f, hint.Width);
            Assert.AreEqual(120f, hint.MinWidth);   // 定宽锁死: 不许收缩
            Assert.IsFalse(hint.FillWidth);
        }

        [Test]
        public void ForField_NonPositiveWidth_FillsAndShrinks()
        {
            var hint = LayoutPolicy.ForField(0f);

            Assert.IsTrue(hint.FillWidth);          // 撑满行内剩余宽
            Assert.AreEqual(0f, hint.MinWidth);     // 允许收缩, 防长值顶宽列
            Assert.AreEqual(0f, hint.Width);
        }

        //-------------------------------------- 文本 --------------------------------------

        [Test]
        public void ForLabel_MaxWidth_IsUpperBoundNotFixed()
        {
            var hint = LayoutPolicy.ForLabel(DrawTextSpec.Of(maxWidth: 200f));

            Assert.AreEqual(200f, hint.MaxWidth);   // 上限宽
            Assert.AreEqual(0f, hint.MinWidth);     // min=0 允许收缩
            Assert.AreEqual(0f, hint.Width);        // 非定宽
            Assert.IsTrue(hint.FillWidth);          // 未定宽时占满行
        }

        [Test]
        public void ForLabel_PositiveWidth_IsFixed()
        {
            var hint = LayoutPolicy.ForLabel(DrawTextSpec.Of(width: 80f));

            Assert.AreEqual(80f, hint.Width);
            Assert.IsFalse(hint.FillWidth);         // 定宽不占满行
        }

        [Test]
        public void ForLabel_NoConstraint_FillsRow()
        {
            var hint = LayoutPolicy.ForLabel(DrawTextSpec.Of());

            Assert.IsTrue(hint.FillWidth);
            Assert.AreEqual(0f, hint.Width);
            Assert.AreEqual(0f, hint.MaxWidth);
        }

        //-------------------------------------- 按钮 --------------------------------------

        [Test]
        public void ForButton_Frameless_HasZeroMargin()
        {
            var hint = LayoutPolicy.ForButton(DrawButtonSpec.Of(width: 14).WithFrameless());

            Assert.AreEqual(0f, hint.MarginLeft);
            Assert.AreEqual(0f, hint.MarginRight);
            Assert.AreEqual(0f, hint.MarginTop);
            Assert.AreEqual(0f, hint.MarginBottom);
        }

        [Test]
        public void ForButton_FixedWidth_HasZeroMarginAndCompactPadding()
        {
            var hint = LayoutPolicy.ForButton(DrawButtonSpec.Of(width: 24));

            Assert.AreEqual(0f, hint.MarginLeft);
            Assert.AreEqual(0f, hint.MarginRight);

            // 紧凑键内边距 (2,2,1,1)
            Assert.AreEqual(2f, hint.PaddingLeft);
            Assert.AreEqual(2f, hint.PaddingRight);
            Assert.AreEqual(1f, hint.PaddingTop);
            Assert.AreEqual(1f, hint.PaddingBottom);
        }

        [Test]
        public void ForButton_Default_UsesButtonMargin()
        {
            var hint = LayoutPolicy.ForButton(DrawButtonSpec.Of(expandWidth: true));

            // 普通按钮外边距 = ButtonMargin(唯一来源)
            Assert.AreEqual(RDTheme.ButtonMargin, hint.MarginLeft);
            Assert.AreEqual(RDTheme.ButtonMargin, hint.MarginRight);
        }

        [Test]
        public void ForButton_ExpandWidth_Fills()
        {
            var hint = LayoutPolicy.ForButton(DrawButtonSpec.Of(expandWidth: true));

            Assert.IsTrue(hint.FillWidth);
        }

        [Test]
        public void ForButton_ExplicitHeight_IsKept()
        {
            var hint = LayoutPolicy.ForButton(DrawButtonSpec.Of(width: 16, height: 18));

            Assert.AreEqual(18f, hint.Height);
        }

        //-------------------------------------- 分组盒 --------------------------------------

        [Test]
        public void ForBox_Width_HasWidthAndMaxWidth()
        {
            var hint = LayoutPolicy.ForBox(boxed: false, width: 300f, expandHeight: false, expandWidth: false);

            Assert.AreEqual(300f, hint.Width);
            Assert.AreEqual(300f, hint.MaxWidth);   // 双锁: 防子控件最小宽撑大
        }

        [Test]
        public void ForBox_Boxed_SetsMarginAndPadding()
        {
            var hint = LayoutPolicy.ForBox(boxed: true, width: 0f, expandHeight: false, expandWidth: false);

            Assert.AreEqual(RDTheme.BoxMargin, hint.MarginTop);
            Assert.AreEqual(RDTheme.BoxMargin, hint.MarginBottom);
            Assert.AreEqual(RDTheme.BoxSpacing, hint.PaddingLeft);
            Assert.AreEqual(RDTheme.BoxSpacing, hint.PaddingRight);
        }

        [Test]
        public void ForBox_ExpandFlags_AreKept()
        {
            var hint = LayoutPolicy.ForBox(boxed: false, width: 0f, expandHeight: true, expandWidth: true);

            Assert.IsTrue(hint.FillHeight);
            Assert.IsTrue(hint.FillWidth);
        }

        //-------------------------------------- 工具栏 --------------------------------------

        [Test]
        public void ForToolbarItem_FillsRow()
        {
            var hint = LayoutPolicy.ForToolbarItem();

            Assert.IsTrue(hint.FillWidth);          // 平分整行
        }
    }
}
