using System.Collections.Generic;
using NUnit.Framework;
using RD.Core;

namespace RD.Core.Tests
{
    /// <summary>
    /// IdStruct / InstanceIdStruct 的位打包往返测试。
    /// 二者是纯结构体、无外部依赖，是分布式 ID 正确性的根基：
    /// 一旦位移宽度或掩码写错，ID 会静默串域（Process 污染 Time 等），
    /// 且这类缺陷在功能测试中极难暴露，故单独覆盖。
    /// </summary>
    [TestFixture]
    public class IdStructTests
    {
        [Test]
        public void IdStruct_RoundTrip_Preserves_All_Fields()
        {
            (short Process, uint Time, uint Value)[] cases =
            {
                (0, 0u, 0u),
                (1, 1u, 1u),
                (1, 0u, 0u),
                (0, 1u, 0u),
                (0, 0u, 1u),
                (1023, 12345u, 999999u),
            };

            foreach ((short process, uint time, uint value) in cases)
            {
                long id = new IdStruct(time, process, value).ToLong();
                IdStruct restored = new(id);

                Assert.AreEqual(process, restored.Process, $"Process, id={id}");
                Assert.AreEqual(time, restored.Time, $"Time, id={id}");
                Assert.AreEqual(value, restored.Value, $"Value, id={id}");
            }
        }

        /// <summary>
        /// 三个域取满位时，64 位全部置 1，ToLong() 返回 -1。
        /// 这是最容易出错的边界：任何一位的位移宽度不对，最大值就无法还原。
        /// </summary>
        [Test]
        public void IdStruct_MaxValues_RoundTrip()
        {
            const short maxProcess = IdGeneratorMask.Process;
            const uint maxTime = IdGeneratorMask.Time;
            const uint maxValue = IdGeneratorMask.Value;

            long id = new IdStruct(maxTime, maxProcess, maxValue).ToLong();

            Assert.AreEqual(-1L, id, "14+30+20 位全为 1 时，long 应为 -1");

            IdStruct restored = new(id);
            Assert.AreEqual(maxProcess, restored.Process);
            Assert.AreEqual(maxTime, restored.Time);
            Assert.AreEqual(maxValue, restored.Value);
        }

        /// <summary>
        /// 域之间不得串扰：仅一个域非 0 时，另两个域必须为 0。
        /// 用于捕获位移宽度或掩码应用顺序的错误。
        /// </summary>
        [Test]
        public void IdStruct_Fields_Do_Not_Bleed_Into_Each_Other()
        {
            long onlyProcess = new IdStruct(0u, 1, 0u).ToLong();
            long onlyTime = new IdStruct(1u, 0, 0u).ToLong();
            long onlyValue = new IdStruct(0u, 0, 1u).ToLong();

            Assert.AreEqual(1, new IdStruct(onlyProcess).Process);
            Assert.AreEqual(0u, new IdStruct(onlyProcess).Time);
            Assert.AreEqual(0u, new IdStruct(onlyProcess).Value);

            Assert.AreEqual(0, new IdStruct(onlyTime).Process);
            Assert.AreEqual(1u, new IdStruct(onlyTime).Time);
            Assert.AreEqual(0u, new IdStruct(onlyTime).Value);

            Assert.AreEqual(0, new IdStruct(onlyValue).Process);
            Assert.AreEqual(0u, new IdStruct(onlyValue).Time);
            Assert.AreEqual(1u, new IdStruct(onlyValue).Value);

            CollectionAssert.AllItemsAreUnique(new[] { onlyProcess, onlyTime, onlyValue });
        }

        /// <summary>
        /// 锁定 Process 必须非负这一隐式约束。
        /// Process 声明为 short（有符号），而位域只分配 14 位：
        /// 负值经 (ushort) 转换后占满 16 位，整体左移会溢出 64 位，导致高位丢失且无法还原。
        /// 该测试不是断言"期望行为"，而是记录"已知限制"——若将来有人放宽约束，此处会失败并提示重新评估。
        /// </summary>
        [Test]
        public void IdStruct_NegativeProcess_Does_Not_RoundTrip_Documented_Limitation()
        {
            long id = new IdStruct(0u, -1, 0u).ToLong();
            IdStruct restored = new(id);

            Assert.AreNotEqual((short) (-1), restored.Process,
                    "Process 为负时无法还原，说明 Process 位域仅支持非负区间 [0, 16383]。");
        }

        [Test]
        public void InstanceIdStruct_RoundTrip_Preserves_All_Fields()
        {
            (uint Time, uint Value)[] cases =
            {
                (0u, 0u),
                (1u, 1u),
                (uint.MaxValue, uint.MaxValue),
                (0u, uint.MaxValue),
                (uint.MaxValue, 0u),
            };

            foreach ((uint time, uint value) in cases)
            {
                long id = new InstanceIdStruct(time, value).ToLong();
                InstanceIdStruct restored = new(id);

                Assert.AreEqual(time, restored.Time, $"Time, id={id}");
                Assert.AreEqual(value, restored.Value, $"Value, id={id}");
            }
        }

        /// <summary>
        /// InstanceId 由 32 位时间 + 32 位值组成，两者均为 uint，
        /// 因此不存在 IdStruct 那样的符号问题，全区间都应可还原。
        /// </summary>
        [Test]
        public void InstanceIdStruct_Covers_Full_UInt_Range()
        {
            long id = new InstanceIdStruct(uint.MaxValue, uint.MaxValue).ToLong();
            InstanceIdStruct restored = new(id);

            Assert.AreEqual(uint.MaxValue, restored.Time);
            Assert.AreEqual(uint.MaxValue, restored.Value);
        }

        /// <summary>
        /// 掩码常量必须与各域声明的位宽一致。
        /// 若有人改动了掩码却未同步位移宽度，上述往返测试未必能覆盖，
        /// 此处直接断言常量值，把约定固化下来。
        /// </summary>
        [Test]
        public void Mask_Constants_Match_Declared_Bit_Widths()
        {
            // 显式转型后再比较：NUnit 比较装箱值时对 short/int 差异敏感
            Assert.AreEqual(0x3fff, (int) IdGeneratorMask.Process, "Process 应为 14 位");
            Assert.AreEqual(0x3fffffff, (int) IdGeneratorMask.Time, "Time 应为 30 位");
            Assert.AreEqual(0xfffff, (int) IdGeneratorMask.Value, "Value 应为 20 位");

            // 14 + 30 + 20 = 64，恰好填满 long
            Assert.AreEqual(64, 14 + 30 + 20);
        }

        /// <summary>
        /// 直接从 RD.Core 的 IdGenerator 读取掩码常量，避免本文件硬编码后与实现脱节。
        /// IdGenerator 的常量为 int，此处统一为无符号便于位运算断言。
        /// </summary>
        private static class IdGeneratorMask
        {
            public const short Process = (short) IdGenerator.Mask14bit;
            public const uint Time = (uint) IdGenerator.Mask30bit;
            public const uint Value = (uint) IdGenerator.Mask20bit;
        }
    }
}
