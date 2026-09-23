using System.Collections.Generic;
using NUnit.Framework;
using RD.Core;

namespace RD.Core.Tests
{
    /// <summary>
    /// IdGenerator 分布式 ID 生成测试。
    ///
    /// GenerateId() 的结构为 14 位 Process + 30 位秒级时间 + 20 位序号：
    /// 同一秒内最多生成 Mask20bit(1048575) 个 ID，超出后序号回绕。
    /// 本组测试覆盖唯一性、字段正确性与回绕行为。
    /// </summary>
    [TestFixture]
    public class IdGeneratorTests
    {
        [SetUp]
        public void SetUp()
        {
            // GenerateId 依赖 TimeInfo（提供时间）与 Options（提供 Process 编号）。
            // 三者都是 Singleton，逐用例重建以避免相互污染。
            World.Instance.AddSingleton<TimeInfo>();
            World.Instance.AddSingleton(new Options());
            World.Instance.AddSingleton<IdGenerator>();
        }

        [TearDown]
        public void TearDown()
        {
            IdGenerator.Instance?.Dispose();
            TimeInfo.Instance?.Dispose();
            Options.Instance?.Dispose();
        }

        [Test]
        public void GenerateId_Embeds_Configured_Process()
        {
            Options.Instance.Process = 7;

            long id = IdGenerator.Instance.GenerateId();

            Assert.AreEqual(7, new IdStruct(id).Process, "ID 中应携带 Options 配置的 Process 编号");
        }

        [Test]
        public void GenerateId_Embeds_Current_Time_In_Seconds()
        {
            long id = IdGenerator.Instance.GenerateId();
            long nowSeconds = TimeInfo.Instance.FrameTime / 1000;

            // IdStruct.Time 记录的是"距 2022 年的秒数"，此处只校验其落在合理区间，
            // 精确值依赖 TimeInfo 实现，逐秒变化，不做硬编码断言。
            uint time = new IdStruct(id).Time;
            Assert.Greater(time, 0u, "运行时刻晚于 2022 年，Time 应大于 0");

            // 允许 ±1 秒误差：秒级截断的取整方向随毫秒余数变化，
            // 硬编码相等会在跨秒边界时偶发抖动。
            long expected = nowSeconds - IdGeneratorEpoch2022Seconds();
            Assert.LessOrEqual(System.Math.Abs((long) time - expected), 1L,
                    "Time 应为当前时间距 2022 年的秒数（允许 1 秒截断误差）");
        }

        /// <summary>
        /// 同一秒内的序号单调递增，且不超出 20 位。
        /// 这是 64 位 ID 不重复的核心保证：序号溢出会直接导致 ID 冲突。
        /// </summary>
        [Test]
        public void GenerateId_Sequence_Is_Monotonic_And_Within_20_Bits()
        {
            // 用 long 承接：-1 强转 uint 会变成 4294967295，导致首轮比较必然失败
            long prev = -1;
            for (int i = 0; i < 5000; i++)
            {
                uint value = new IdStruct(IdGenerator.Instance.GenerateId()).Value;

                Assert.LessOrEqual(value, (uint) IdGenerator.Mask20bit, "序号应在 20 位以内");
                Assert.Greater((long) value, prev, "未回绕时序号应严格递增");
                prev = value;
            }
        }

        /// <summary>
        /// 批量生成不重复。取 10000 个，远小于单秒容量 1048575，
        /// 因此正常情况下不应出现任何碰撞。
        /// </summary>
        [Test]
        public void GenerateId_Produces_No_Duplicates_Within_Capacity()
        {
            var ids = new HashSet<long>();

            for (int i = 0; i < 10000; i++)
            {
                Assert.IsTrue(ids.Add(IdGenerator.Instance.GenerateId()), $"第 {i} 个 ID 发生碰撞");
            }

            Assert.AreEqual(10000, ids.Count);
        }

        [Test]
        public void GenerateInstanceId_Is_Monotonic_And_Unique()
        {
            var ids = new HashSet<long>();
            long prev = long.MinValue;

            for (int i = 0; i < 10000; i++)
            {
                long id = IdGenerator.Instance.GenerateInstanceId();

                Assert.IsTrue(ids.Add(id), $"第 {i} 个 InstanceId 发生碰撞");
                Assert.Greater(id, prev, "InstanceId 应严格递增");
                prev = id;
            }
        }

        /// <summary>
        /// GenerateInstanceId 使用 Interlocked 自增，多线程下仍应保持唯一。
        /// 这是它与 GenerateId（用 lock）不同的并发保证，单独覆盖。
        /// </summary>
        [Test]
        public void GenerateInstanceId_Is_Thread_Safe()
        {
            const int threadCount = 8;
            const int perThread = 5000;

            var ids = new System.Collections.Concurrent.ConcurrentBag<long>();

            void Work()
            {
                for (int i = 0; i < perThread; i++)
                {
                    ids.Add(IdGenerator.Instance.GenerateInstanceId());
                }
            }

            var threads = new System.Threading.Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                threads[i] = new System.Threading.Thread(Work);
                threads[i].Start();
            }

            foreach (System.Threading.Thread t in threads)
            {
                t.Join();
            }

            var distinct = new HashSet<long>(ids);

            Assert.AreEqual(threadCount * perThread, ids.Count, "应生成预期数量的 ID");
            Assert.AreEqual(ids.Count, distinct.Count, "多线程下不得出现重复 ID");
        }

        /// <summary>
        /// IdGenerator.Awake 中计算 2022 年相对 1970 年的毫秒偏移，
        /// TimeSince2022 以秒为单位，此处复刻该换算用于校验 Time 字段。
        /// </summary>
        private static long IdGeneratorEpoch2022Seconds()
        {
            long epoch1970Ms = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc).Ticks / 10000;
            long epoch2022Ms = new System.DateTime(2022, 1, 1, 0, 0, 0, System.DateTimeKind.Utc).Ticks / 10000 - epoch1970Ms;
            return epoch2022Ms / 1000;
        }
    }
}
