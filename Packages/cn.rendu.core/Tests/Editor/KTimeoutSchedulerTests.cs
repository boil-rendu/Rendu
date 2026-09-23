using NUnit.Framework;
using RD.Core;

namespace RD.Core.Tests
{
    /// <summary>
    /// KTimeoutScheduler 纯逻辑测试（不依赖网络）。
    /// 守护 KService 超时调度抽取后的行为：立即项入 updateIds、
    /// 定时项按 timeId + minTime 在到期时移入 updateIds。
    /// </summary>
    [TestFixture]
    public class KTimeoutSchedulerTests
    {
        [Test]
        public void AddImmediate_Goes_To_UpdateIds()
        {
            var s = new KTimeoutScheduler();
            s.AddToUpdate(0, 100);
            Assert.AreEqual(1, s.PendingImmediateCount);
            Assert.AreEqual(0, s.PendingTimedCount);

            var due = s.TakeDueUpdateIds();
            Assert.AreEqual(1, due.Count);
            Assert.AreEqual(100, due[0]);
            Assert.AreEqual(0, s.PendingImmediateCount);
        }

        [Test]
        public void Timed_Not_Expired_Before_MinTime()
        {
            var s = new KTimeoutScheduler();
            s.AddToUpdate(500, 7);
            s.AddToUpdate(300, 9);
            // 早于最小定时(300)：直接返回，无任何到期
            s.TimerOut(100);
            Assert.AreEqual(0, s.TakeDueUpdateIds().Count);
            Assert.AreEqual(2, s.PendingTimedCount);
        }

        [Test]
        public void All_Timed_Expire_When_TimeNow_Exceeds_Max()
        {
            var s = new KTimeoutScheduler();
            s.AddToUpdate(500, 7);
            s.AddToUpdate(300, 9);
            s.TimerOut(600);
            var due = s.TakeDueUpdateIds();
            Assert.AreEqual(2, due.Count);
            Assert.IsTrue(due.Contains(7));
            Assert.IsTrue(due.Contains(9));
            Assert.AreEqual(0, s.PendingTimedCount);
        }

        [Test]
        public void Immediate_And_Timed_Merge_On_Take()
        {
            var s = new KTimeoutScheduler();
            s.AddToUpdate(0, 1);
            s.AddToUpdate(200, 2);
            s.TimerOut(200);
            var due = s.TakeDueUpdateIds();
            Assert.AreEqual(2, due.Count);
            Assert.IsTrue(due.Contains(1));
            Assert.IsTrue(due.Contains(2));
        }

        [Test]
        public void Single_Timed_Expires_At_Its_Time()
        {
            var s = new KTimeoutScheduler();
            s.AddToUpdate(300, 9);
            s.TimerOut(299);
            Assert.AreEqual(0, s.TakeDueUpdateIds().Count);
            s.TimerOut(300);
            var due = s.TakeDueUpdateIds();
            Assert.AreEqual(1, due.Count);
            Assert.AreEqual(9, due[0]);
        }
    }
}
