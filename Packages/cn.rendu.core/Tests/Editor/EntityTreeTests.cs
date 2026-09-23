using System;
using NUnit.Framework;
using RD.Core;

namespace RD.Core.Tests
{
    /// <summary>
    /// 最小 IScene 桩。测试实体均非 IScene 类型，因此不会读取 Fiber，
    /// 仅用于满足 Entity 父子挂载"parent 必须已在数据树上(IScene!=null)"的前置条件。
    /// </summary>
    public class TestScene : IScene
    {
        public Fiber Fiber { get; set; }
        public int SceneType { get; set; }
    }

    /// <summary>
    /// Entity 组件/子实体树的行为测试（改进建议 2 在 P1 明确点名的缺口）：
    /// 覆盖 AddComponent/AddChild 的挂载、RemoveComponent/RemoveChild 的摘除与级联 Dispose、
    /// 以及根 Dispose 对整棵子树的递归释放。不依赖 Fiber/视图运行时。
    /// 父子挂载必须先在数据树(IScene)上，因此 root 会被挂到 TestScene。
    /// </summary>
    [TestFixture]
    public class EntityTreeTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            World.Instance.AddSingleton<ObjectPool>();
            World.Instance.AddSingleton<IdGenerator>();
            World.Instance.AddSingleton<EntitySystemSingleton>();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            ObjectPool.Instance?.Dispose();
        }

        private TestEntity NewRoot()
        {
            var root = ObjectPool.Fetch<TestEntity>();
            root.SetScene(new TestScene());
            return root;
        }

        [Test]
        public void AddComponent_Mounts_And_Resolves()
        {
            var root = NewRoot();
            Entity component = root.AddComponent<TestComponentEntity>();

            Assert.AreEqual(1, root.ComponentsCount());
            Assert.AreSame(component, root.GetComponent<TestComponentEntity>());
            Assert.IsFalse(component.IsDisposed, "挂载后不应被释放");
            Assert.AreSame(root, component.GetParent<TestEntity>());
        }

        [Test]
        public void RemoveComponent_Detaches_And_Disposes_Child()
        {
            var root = NewRoot();
            Entity component = root.AddComponent<TestComponentEntity>();

            root.RemoveComponent<TestComponentEntity>();

            Assert.AreEqual(0, root.ComponentsCount());
            Assert.IsNull(root.GetComponent<TestComponentEntity>());
            Assert.IsTrue(component.IsDisposed, "移除组件应级联释放该组件");
        }

        [Test]
        public void AddChild_Mounts_And_Resolves_By_Id()
        {
            var root = NewRoot();
            TestChildEntity child = root.AddChild<TestChildEntity>();

            Assert.AreEqual(1, root.ChildrenCount());
            Assert.AreSame(child, root.GetChild<TestChildEntity>(child.Id));
            Assert.AreSame(root, child.GetParent<TestEntity>());
        }

        [Test]
        public void RemoveChild_Detaches_And_Disposes_Child()
        {
            var root = NewRoot();
            TestChildEntity child = root.AddChild<TestChildEntity>();

            root.RemoveChild(child.Id);

            Assert.AreEqual(0, root.ChildrenCount());
            Assert.IsNull(root.GetChild<TestChildEntity>(child.Id));
            Assert.IsTrue(child.IsDisposed, "移除子实体应级联释放");
        }

        [Test]
        public void Duplicate_AddComponent_Throws()
        {
            var root = NewRoot();
            root.AddComponent<TestComponentEntity>();
            Assert.Throws<Exception>(() => root.AddComponent<TestComponentEntity>());
            root.Dispose();
        }

        [Test]
        public void Dispose_Recursively_Releases_Subtree()
        {
            var root = NewRoot();
            TestChildEntity child = root.AddChild<TestChildEntity>();
            Entity grandChildComp = child.AddComponent<TestComponentEntity>();
            Entity rootComp = root.AddComponent<TestComponentEntity>();

            root.Dispose();

            Assert.IsTrue(root.IsDisposed);
            Assert.IsTrue(child.IsDisposed);
            Assert.IsTrue(grandChildComp.IsDisposed, "子实体的组件应随子树递归释放");
            Assert.IsTrue(rootComp.IsDisposed);
        }
    }
}
