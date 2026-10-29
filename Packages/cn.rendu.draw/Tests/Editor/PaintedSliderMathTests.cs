using NUnit.Framework;
using RD.Draw.Editor;

namespace RD.Draw.Tests.Editor
{
    /// <summary>
    /// 自绘滑杆的几何换算(Phase 4)。拖动时最容易错的就是这几行, 所以单独测:
    /// 端点贴边、越界夹取、退化区间(max &lt;= min)、零行程(还没布局)都不能把值算飞。
    /// </summary>
    public class PaintedSliderMathTests
    {
        private const float Epsilon = 1e-4f;

        [Test]
        public void Ratio_MapsRangeToUnit()
        {
            Assert.AreEqual(0f, PaintedSliderMath.Ratio(0f, 0f, 10f), Epsilon);
            Assert.AreEqual(0.5f, PaintedSliderMath.Ratio(5f, 0f, 10f), Epsilon);
            Assert.AreEqual(1f, PaintedSliderMath.Ratio(10f, 0f, 10f), Epsilon);
        }

        [Test]
        public void Ratio_ClampsOutOfRangeValues()
        {
            Assert.AreEqual(0f, PaintedSliderMath.Ratio(-5f, 0f, 10f), Epsilon);
            Assert.AreEqual(1f, PaintedSliderMath.Ratio(99f, 0f, 10f), Epsilon);
        }

        [Test]
        public void Ratio_DegenerateRange_IsZero()
        {
            // max == min 或 max < min: 滑杆没有可走的区间, 停在最左(取 min)才是可预期的
            Assert.AreEqual(0f, PaintedSliderMath.Ratio(5f, 5f, 5f), Epsilon);
            Assert.AreEqual(0f, PaintedSliderMath.Ratio(5f, 10f, 0f), Epsilon);
        }

        [Test]
        public void ValueFromX_MapsTrackEndsToRangeEnds()
        {
            Assert.AreEqual(0f, PaintedSliderMath.ValueFromX(0f, 100f, 0f, 10f), Epsilon);
            Assert.AreEqual(5f, PaintedSliderMath.ValueFromX(50f, 100f, 0f, 10f), Epsilon);
            Assert.AreEqual(10f, PaintedSliderMath.ValueFromX(100f, 100f, 0f, 10f), Epsilon);
        }

        [Test]
        public void ValueFromX_ClampsBeyondTrack()
        {
            Assert.AreEqual(0f, PaintedSliderMath.ValueFromX(-30f, 100f, 0f, 10f), Epsilon);
            Assert.AreEqual(10f, PaintedSliderMath.ValueFromX(130f, 100f, 0f, 10f), Epsilon);
        }

        [Test]
        public void ValueFromX_WithoutTravel_ReturnsMin()
        {
            // 布局还没结算时 contentRect.width 是 0, 行程为负 —— 不能除出 NaN/Infinity
            Assert.AreEqual(2f, PaintedSliderMath.ValueFromX(10f, 0f, 2f, 8f), Epsilon);
            Assert.AreEqual(2f, PaintedSliderMath.ValueFromX(10f, -12f, 2f, 8f), Epsilon);
        }

        [Test]
        public void ThumbOffset_IsRatioTimesTravel()
        {
            Assert.AreEqual(0f, PaintedSliderMath.ThumbOffset(0f, 0f, 10f, 50f), Epsilon);
            Assert.AreEqual(25f, PaintedSliderMath.ThumbOffset(5f, 0f, 10f, 50f), Epsilon);
            Assert.AreEqual(50f, PaintedSliderMath.ThumbOffset(10f, 0f, 10f, 50f), Epsilon);
        }

        [Test]
        public void ThumbOffset_WithoutTravel_IsZero()
        {
            Assert.AreEqual(0f, PaintedSliderMath.ThumbOffset(5f, 0f, 10f, 0f), Epsilon);
            Assert.AreEqual(0f, PaintedSliderMath.ThumbOffset(5f, 0f, 10f, -4f), Epsilon);
        }
    }
}
