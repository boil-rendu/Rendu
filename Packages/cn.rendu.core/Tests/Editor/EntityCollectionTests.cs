using NUnit.Framework;
using RD.Core;

namespace RD.Core.Tests
{
    /// <summary>
    /// ComponentsCollection / ChildrenCollection 的基础字典行为测试。
    /// 重点守护 Entity 拆分时引入的 Remove 调用修正（移除双参数 out 重载，
    /// 改用 SortedDictionary 标准单参数 Remove + TryGetValue 取值），确保
    /// Add / TryGetValue / Remove / Count 语义不被破坏。
    /// </summary>
    [TestFixture]
    public class EntityCollectionTests
    {
        [SetUp]
        public void SetUp()
        {
            World.Instance.AddSingleton<ObjectPool>();
        }

        [TearDown]
        public void TearDown()
        {
            ObjectPool.Instance?.Dispose();
        }

        [Test]
        public void ComponentsCollection_Add_And_TryGetValue()
        {
            var collection = ObjectPool.Fetch<ComponentsCollection>();
            TestEntity a = ObjectPool.Fetch<TestEntity>();
            long key = a.GetLongHashCode();

            collection.Add(key, a);

            Assert.IsTrue(collection.TryGetValue(key, out Entity got));
            Assert.AreSame(a, got);
            Assert.AreEqual(1, collection.Count);

            collection.Dispose();
        }

        [Test]
        public void ComponentsCollection_Remove_Decrements_Count_And_Clears_Value()
        {
            var collection = ObjectPool.Fetch<ComponentsCollection>();
            TestEntity a = ObjectPool.Fetch<TestEntity>();
            long key = a.GetLongHashCode();

            collection.Add(key, a);
            bool removed = collection.Remove(key);

            Assert.IsTrue(removed, "存在的 key 应移除成功");
            Assert.AreEqual(0, collection.Count);
            Assert.IsFalse(collection.TryGetValue(key, out _), "移除后不应再取到值");

            collection.Dispose();
        }

        [Test]
        public void ComponentsCollection_Remove_MissingKey_Returns_False()
        {
            var collection = ObjectPool.Fetch<ComponentsCollection>();
            Assert.IsFalse(collection.Remove(123456), "不存在的 key 移除应返回 false");

            collection.Dispose();
        }

        [Test]
        public void ChildrenCollection_Add_ById_And_TryGetValue()
        {
            var collection = ObjectPool.Fetch<ChildrenCollection>();
            TestEntity child = ObjectPool.Fetch<TestEntity>();
            child.SetId(777);

            collection.Add(child.Id, child);

            Assert.IsTrue(collection.TryGetValue(777, out Entity got));
            Assert.AreSame(child, got);
            Assert.AreEqual(1, collection.Count);

            collection.Dispose();
        }

        [Test]
        public void ChildrenCollection_Remove_ById_Works()
        {
            var collection = ObjectPool.Fetch<ChildrenCollection>();
            TestEntity child = ObjectPool.Fetch<TestEntity>();
            child.SetId(888);

            collection.Add(child.Id, child);
            bool removed = collection.Remove(888);

            Assert.IsTrue(removed);
            Assert.AreEqual(0, collection.Count);
            Assert.IsFalse(collection.TryGetValue(888, out _));

            collection.Dispose();
        }

        [Test]
        public void Collection_Dispose_On_Pooled_Instance_Clears_Items()
        {
            // 从池取出的集合，Dispose 后应清空内部项（Clear）
            var collection = ObjectPool.Fetch<ComponentsCollection>();
            TestEntity a = ObjectPool.Fetch<TestEntity>();
            collection.Add(a.GetLongHashCode(), a);

            Assert.AreEqual(1, collection.Count);
            collection.Dispose();

            // 再次从池取出同一集合实例时应已被清空
            var reused = ObjectPool.Fetch<ComponentsCollection>();
            Assert.AreEqual(0, reused.Count, "池化集合 Dispose 后内部应被清空");
            reused.Dispose();
        }
    }
}
