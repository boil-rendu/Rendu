namespace RD.Core
{
    /// <summary>Entity 到所属 Scene / Fiber / Zone / Root 的快捷访问扩展。</summary>
    public static class EntityHelper
    {
        /// <summary>获取 Entity 所属的 Zone（来自 <c>Fiber.Zone</c>）。</summary>
        public static int Zone(this Entity entity)
        {
            return entity.IScene.Fiber.Zone;
        }

        /// <summary>获取 Entity 所属的 <see cref="Scene"/>（强类型）。</summary>
        public static Scene Scene(this Entity entity)
        {
            return entity.IScene as Scene;
        }

        /// <summary>获取 Entity 所属的 Scene（自定义类型，泛型版）。</summary>
        public static T Scene<T>(this Entity entity) where T: class, IScene
        {
            return entity.IScene as T;
        }

        /// <summary>获取 Entity 所属 Fiber 的 <see cref="Scene.Root"/>。</summary>
        public static Scene Root(this Entity entity)
        {
            return entity.IScene.Fiber.Root;
        }

        /// <summary>获取 Entity 所属的 <see cref="Fiber"/>。</summary>
        public static Fiber Fiber(this Entity entity)
        {
            return entity.IScene.Fiber;
        }
    }
}
