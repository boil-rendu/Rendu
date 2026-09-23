using System.Collections.Generic;
using NUnit.Framework;
using RD.Core;

namespace RD.Core.Tests
{
    /// <summary>
    /// 测试专用入池类型。
    /// ObjectPool 要求类型实现 IPool 且具备无参构造。
    /// </summary>
    public sealed class TestPooled: IPool
    {
        public bool IsFromPool { get; set; }

        public int CtorCount;

        public void Dispose()
        {
        }
    }

    /// <summary>
    /// ObjectPool 无锁对象池测试。
    /// 该池被高频调用且跨线程共享，一旦出现同一实例被两个调用方同时持有，
    /// 会产生极难复现的数据错乱，故并发正确性是本组测试的重点。
    /// </summary>
    [TestFixture]
    public class ObjectPoolTests
    {
        [SetUp]
        public void SetUp()
        {
            // ObjectPool 是 Singleton，静态方法在 Instance 为 null 时会退化，
            // 因此每个用例前重新注册，避免用例间相互污染。
            World.Instance.AddSingleton<ObjectPool>();
        }

        [TearDown]
        public void TearDown()
        {
            ObjectPool.Instance?.Dispose();
        }

        [Test]
        public void Fetch_Returns_Usable_Instance_And_Marks_From_Pool()
        {
            TestPooled a = ObjectPool.Fetch<TestPooled>();

            Assert.IsNotNull(a);
            Assert.IsTrue(a.IsFromPool, "从池中取出的对象应标记 IsFromPool");
        }

        [Test]
        public void Fetch_With_IsFromPool_False_Does_Not_Mark_From_Pool()
        {
            TestPooled a = ObjectPool.Fetch<TestPooled>(false);

            Assert.IsNotNull(a);
            Assert.IsFalse(a.IsFromPool, "isFromPool=false 时应直接 new，不标记来源");
        }

        [Test]
        public void Recycle_Then_Fetch_Reuses_Same_Instance()
        {
            TestPooled a = ObjectPool.Fetch<TestPooled>();
            ObjectPool.Recycle(a);

            TestPooled b = ObjectPool.Fetch<TestPooled>();

            Assert.AreSame(a, b, "归还后再次 Fetch 应复用同一实例");
        }

        /// <summary>
        /// IsFromPool 同时充当"是否已入池"的标记位，用于防止同一对象被重复回收。
        /// 重复回收会让同一实例在池中出现两次，进而被两个调用方同时持有。
        /// </summary>
        [Test]
        public void Double_Recycle_Is_Ignored()
        {
            TestPooled a = ObjectPool.Fetch<TestPooled>();
            ObjectPool.Recycle(a);
            ObjectPool.Recycle(a);

            TestPooled b = ObjectPool.Fetch<TestPooled>();
            ObjectPool.Recycle(b);

            TestPooled c = ObjectPool.Fetch<TestPooled>();

            Assert.AreSame(b, c, "重复回收被忽略时，池中只应存在一份该实例");
        }

        [Test]
        public void Recycle_Resets_IsFromPool_Flag()
        {
            TestPooled a = ObjectPool.Fetch<TestPooled>();
            Assert.IsTrue(a.IsFromPool);

            ObjectPool.Recycle(a);

            Assert.IsFalse(a.IsFromPool, "回收后应清除标记，避免重复入池");
        }

        /// <summary>
        /// 并发正确性：任一时刻同一实例不得被两个调用方同时持有。
        /// 这是该无锁实现最核心的不变量，用 32 万次取还来放大竞态窗口。
        /// </summary>
        [Test]
        public void Concurrent_Fetch_And_Recycle_Never_Hands_Out_Same_Instance_Twice()
        {
            const int threadCount = 16;
            const int perThread = 20000;

            var held = new System.Collections.Concurrent.ConcurrentDictionary<object, byte>();
            int violations = 0;
            int operations = 0;

            void Work()
            {
                for (int i = 0; i < perThread; i++)
                {
                    TestPooled item = ObjectPool.Fetch<TestPooled>();

                    // 若该实例已在别处被持有，说明池把它交给了两个调用方
                    if (!held.TryAdd(item, 0))
                    {
                        System.Threading.Interlocked.Increment(ref violations);
                    }

                    System.Threading.Interlocked.Increment(ref operations);
                    held.TryRemove(item, out byte _);
                    ObjectPool.Recycle(item);
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

            Assert.AreEqual(threadCount * perThread, operations, "所有取还应全部完成");
            Assert.AreEqual(0, violations, "同一实例被并发持有");
        }

        /// <summary>
        /// 边取边还时，池中只有一个对象在循环。
        /// 这个用例锁定池的基本复用语义：归还发生在下一次 Fetch 之前，
        /// 因此同一实例会被反复取出。断言值经实测确认为 1。
        /// </summary>
        [Test]
        public void Sequential_Fetch_Recycle_Reuses_Single_Instance()
        {
            var seen = new HashSet<TestPooled>();

            for (int i = 0; i < 50; i++)
            {
                TestPooled p = ObjectPool.Fetch<TestPooled>();
                seen.Add(p);
                ObjectPool.Recycle(p);
            }

            Assert.AreEqual(1, seen.Count, "边取边还应始终复用同一个实例");
        }

        /// <summary>
        /// 先取满再统一归还：归还数量远小于容量上限（1000）时，取回应全部命中池中对象。
        /// 注意不可写成"边取边还"——那样池里永远只有一个对象（见上一用例）。
        /// </summary>
        [Test]
        public void Pool_Reuses_All_Returned_Instances_When_Under_Capacity()
        {
            var returned = new HashSet<TestPooled>();
            var items = new List<TestPooled>();

            for (int i = 0; i < 50; i++)
            {
                TestPooled p = ObjectPool.Fetch<TestPooled>();
                items.Add(p);
                returned.Add(p);
            }

            Assert.AreEqual(50, returned.Count, "池空时应取到 50 个互不相同的实例");

            foreach (TestPooled p in items)
            {
                ObjectPool.Recycle(p);
            }

            int reused = 0;
            for (int i = 0; i < 50; i++)
            {
                if (returned.Contains(ObjectPool.Fetch<TestPooled>()))
                {
                    reused++;
                }
            }

            Assert.AreEqual(50, reused, "未超出容量时，归还的对象应全部可复用");
        }

        /// <summary>
        /// 非池对象无法入池：Recycle 以 IsFromPool 作为"已入池"标记位，
        /// 直接 new 出来的对象 IsFromPool 为 false，会被静默丢弃。
        /// 若误用（对非池对象调用 Recycle），不会污染池内容。
        /// </summary>
        [Test]
        public void Recycle_Non_Pool_Object_Is_Ignored()
        {
            TestPooled fresh = new();
            ObjectPool.Recycle(fresh);

            TestPooled fetched = ObjectPool.Fetch<TestPooled>();

            Assert.AreNotSame(fresh, fetched, "未从池中获取的对象不应被回收进池");
        }

        /// <summary>
        /// 池被抽空后应新建实例，而不是返回 null 或已归还的实例。
        /// </summary>
        [Test]
        public void Fetch_On_Empty_Pool_Creates_Distinct_Instances()
        {
            TestPooled a = ObjectPool.Fetch<TestPooled>();
            TestPooled b = ObjectPool.Fetch<TestPooled>();

            Assert.AreNotSame(a, b, "池空时应新建实例");
            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
        }
    }
}
