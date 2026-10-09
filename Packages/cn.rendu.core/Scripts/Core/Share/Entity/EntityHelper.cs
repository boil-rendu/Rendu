using System;
using System.Linq;

namespace RD.Core
{
    /// <summary>Entity 扩展方法集合：所属 Scene / Fiber / Zone / Root 快捷访问 + 常用组件/父子操作（原 EntityHelper 与 Entity_Extend 合并）。</summary>
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

        /// <summary>强类型获取 Component：未找到时记错误日志并返回 <c>default</c>。</summary>
        /// <remarks>此方法一定会写错误日志（无论业务全局日志开关）；若不想报错请使用 <c>GetComponent&lt;K&gt;()</c>。</remarks>
        //自定义扩展 获取不到就报错 不管你log是true 还是false
        //不想报错就别调用这个方法
        public static K GetComponent<K>(this Entity self, bool log) where K : Entity
        {
            var component = self.GetComponent<K>();
            if (component == null)
            {
                Log.Error($"{self.GetType().Name} 目标没有这个组件 {typeof(K).Name}");
                return default;
            }

            return component;
        }

        /// <summary>设置父 Entity（幂等）。等同于 <c>parent.AddChild(self)</c>，提供更直观的命名；已是同一父时直接返回。</summary>
        //虽然使用 AddChild也可以实现
        //但是为了保持一致性 跟第一直觉 还是写一个方法
        //且相同时不报错
        public static void SetParent(this Entity self, Entity parent)
        {
            if (parent == null) return;
            if (self.Parent == parent) return;
            parent.AddChild(self);
        }

        /// <summary>从父 Entity 上获取类型为 <typeparamref name="K"/> 的 Component。跳过分析器解析路径，请谨慎使用。</summary>
        //跳过分析器用的 慎用 除非你知道你在做什么
        public static K GetParentComponent<K>(this Entity self) where K : Entity
        {
            return self.Parent.GetComponent<K>();
        }

        /// <summary>从父 Entity 的 Children 中按 id 查找子 Entity。跳过分析器解析路径，请谨慎使用。</summary>
        //跳过分析器用的 慎用 除非你知道你在做什么
        public static K GetParentChild<K>(this Entity self, int id) where K : Entity
        {
            return self.Parent.GetChild<K>(id);
        }

        /// <summary>递归移除所有子 Entity（快捷操作）。</summary>
        //快捷移除所有子类
        public static void RemoveAllChild(this Entity self)
        {
            if (self.ChildrenCount() <= 0) return;
            foreach (long unitId in self.Children.Keys.ToArray())
            {
                self.RemoveChild(unitId);
            }
        }
    }
}
