using NUnit.Framework;
using RD.Draw.Editor.Rendering;

namespace RD.Draw.Tests.Editor
{
    /// <summary>
    /// 分页区间的回归基线: 列表与表格现在共用 <see cref="PagingRange"/>。
    ///
    /// 钉住的边界:
    ///   - 未启用分页 = 整段 [0, count);
    ///   - 页索引越界夹回两端, 末页可能不满;
    ///   - **空数据 + 启用分页**必须给出空区间 —— 旧实现两处各写一遍除法,
    ///     空数据时页数算成 0、页索引经 Mathf.Clamp 得到 -1, 起始下标随之为负,
    ///     绘制循环会直接越界; 这条用例就是那次收敛的护栏。
    /// </summary>
    public class PagingRangeTests
    {
        [Test]
        public void Disabled_ReturnsWholeRange()
        {
            var page = PagingRange.Compute(25, 10, enabled: false, requestedPage: 2);

            Assert.IsFalse(page.Enabled);
            Assert.AreEqual(1, page.PageCount);
            Assert.AreEqual(0, page.PageIndex);
            Assert.AreEqual(0, page.Start);
            Assert.AreEqual(25, page.End);
        }

        [Test]
        public void Enabled_FirstPage()
        {
            var page = PagingRange.Compute(25, 10, enabled: true, requestedPage: 0);

            Assert.AreEqual(3, page.PageCount);
            Assert.AreEqual(0, page.PageIndex);
            Assert.AreEqual(0, page.Start);
            Assert.AreEqual(10, page.End);
        }

        [Test]
        public void Enabled_LastPageIsShort()
        {
            var page = PagingRange.Compute(25, 10, enabled: true, requestedPage: 2);

            Assert.AreEqual(2, page.PageIndex);
            Assert.AreEqual(20, page.Start);
            Assert.AreEqual(25, page.End);
        }

        [Test]
        public void Enabled_ClampsRequestedPage()
        {
            var above = PagingRange.Compute(25, 10, enabled: true, requestedPage: 99);
            Assert.AreEqual(2, above.PageIndex);
            Assert.AreEqual(20, above.Start);

            var below = PagingRange.Compute(25, 10, enabled: true, requestedPage: -3);
            Assert.AreEqual(0, below.PageIndex);
            Assert.AreEqual(0, below.Start);
        }

        [Test]
        public void Enabled_EmptyList_YieldsEmptyRange()
        {
            var page = PagingRange.Compute(0, 10, enabled: true, requestedPage: 5);

            // 至少一页: 页数 0 会让 "pageCount > 1" 之外的夹取算出负下标。
            Assert.AreEqual(1, page.PageCount);
            Assert.AreEqual(0, page.PageIndex);
            Assert.AreEqual(0, page.Start);
            Assert.AreEqual(0, page.End);
        }

        [Test]
        public void Enabled_NonPositivePageSize_IsTreatedAsOne()
        {
            var page = PagingRange.Compute(5, 0, enabled: true, requestedPage: 0);

            Assert.AreEqual(1, page.PageSize);
            Assert.AreEqual(5, page.PageCount);
            Assert.AreEqual(0, page.Start);
            Assert.AreEqual(1, page.End);
        }
    }
}
