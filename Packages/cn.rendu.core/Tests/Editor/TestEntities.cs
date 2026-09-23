using RD.Core;

namespace RD.Core.Tests
{
    /// <summary>
    /// 测试用最简实体。重写 RegisterSystem 为空，避免依赖 Fiber/EntitySystem。
    /// </summary>
    public class TestEntity : Entity
    {
        protected override void RegisterSystem()
        {
        }

        // 测试辅助：IScene / Id 的 setter 受保护，仅供 Entity 自身及子类访问，
        // 因此由 TestEntity 这个子类提供 public 包装，供测试代码挂场景/设 Id。
        public void SetScene(IScene scene) => this.IScene = scene;

        public void SetId(long id) => this.Id = id;
    }

    /// <summary>
    /// 满足 AddChild/AddComponent 泛型约束（IAwake）的测试实体。
    /// </summary>
    public class TestChildEntity : Entity, IAwake
    {
        protected override void RegisterSystem()
        {
        }
    }

    public class TestComponentEntity : Entity, IAwake
    {
        protected override void RegisterSystem()
        {
        }
    }
}
