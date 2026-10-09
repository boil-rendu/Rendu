using System;

namespace RD.Core
{
    /// <summary>
    /// 异步 Awake 派发辅助（静态扩展+内部派发）：提供 <c>entity.AwakeAsync(...)</c> 扩展以及 <c>Entity.AddComponentAsync</c> 内部使用的
    /// 强类型派发器。内部 <c>EntityAwakeAsync</c> 重载从 <see cref="EntitySystemSingleton"/> 查询并 <c>await</c> 执行所有匹配的 <see cref="IAwakeAsyncSystem"/>。
    /// </summary>
    public static class EntityAwakeAsyncSystem
    {
        /// <summary>扩展：<c>await entity.AwakeAsync()</c>（无参）。</summary>
        public static async Promise<T> AwakeAsync<T>(this T self) where T : Entity, IAwakeAsync
        {
            await EntityAwakeAsync(self);
            return self;
        }

        /// <summary>扩展：<c>await entity.AwakeAsync(p1)</c>（1 参）。</summary>
        public static async Promise<T> AwakeAsync<T, P1>(this T self, P1 p1) where T : Entity, IAwakeAsync<P1>
        {
            await EntityAwakeAsync(self, p1);
            return self;
        }

        /// <summary>扩展：<c>await entity.AwakeAsync(p1, p2)</c>（2 参）。</summary>
        public static async Promise<T> AwakeAsync<T, P1, P2>(this T self, P1 p1, P2 p2) where T : Entity, IAwakeAsync<P1, P2>
        {
            await EntityAwakeAsync(self, p1, p2);
            return self;
        }

        /// <summary>扩展：<c>await entity.AwakeAsync(p1, p2, p3)</c>（3 参）。</summary>
        public static async Promise<T> AwakeAsync<T, P1, P2, P3>(this T self, P1 p1, P2 p2, P3 p3) where T : Entity, IAwakeAsync<P1, P2, P3>
        {
            await EntityAwakeAsync(self, p1, p2, p3);
            return self;
        }

        /// <summary>扩展：<c>await entity.AwakeAsync(p1, p2, p3, p4)</c>（4 参）。</summary>
        public static async Promise<T> AwakeAsync<T, P1, P2, P3, P4>(this T self, P1 p1, P2 p2, P3 p3, P4 p4) where T : Entity, IAwakeAsync<P1, P2, P3, P4>
        {
            await EntityAwakeAsync(self, p1, p2, p3, p4);
            return self;
        }

        /// <summary>扩展：<c>await entity.AwakeAsync(p1, p2, p3, p4, p5)</c>（5 参）。</summary>
        public static async Promise<T> AwakeAsync<T, P1, P2, P3, P4, P5>(this T self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5) where T : Entity, IAwakeAsync<P1, P2, P3, P4, P5>
        {
            await EntityAwakeAsync(self, p1, p2, p3, p4, p5);
            return self;
        }

        /// <summary>无参派发：从 <see cref="EntitySystemSingleton.TypeSystems"/> 查找并 <c>await</c> 执行所有 <see cref="IAwakeAsyncSystem"/>（要求实体实现 <see cref="IAwakeAsync"/>）。</summary>
        internal static async Promise EntityAwakeAsync(Entity self)
        {
            if (self is not IAwakeAsync)
            {
                return;
            }

            var iAwakeSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(self.GetType(), typeof(IAwakeAsyncSystem));
            if (iAwakeSystems == null)
            {
                return;
            }

            EntityRef<Entity> selfRef = self;
            foreach (IAwakeAsyncSystem aAwakeAsyncSystem in iAwakeSystems)
            {
                if (aAwakeAsyncSystem == null)
                {
                    continue;
                }

                try
                {
                    await aAwakeAsyncSystem.Run(selfRef);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        /// <summary>1 参派发：要求实体实现 <see cref="IAwakeAsync{P1}"/>。</summary>
        internal static async Promise EntityAwakeAsync<P1>(Entity self, P1 p1)
        {
            if (self is not IAwakeAsync<P1>)
            {
                return;
            }

            var iAwakeAsyncSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(self.GetType(), typeof(IAwakeAsyncSystem<P1>));
            if (iAwakeAsyncSystems == null)
            {
                return;
            }

            EntityRef<Entity> selfRef = self;
            foreach (IAwakeAsyncSystem<P1> aAwakeAsyncSystem in iAwakeAsyncSystems)
            {
                if (aAwakeAsyncSystem == null)
                {
                    continue;
                }

                try
                {
                    await aAwakeAsyncSystem.Run(selfRef, p1);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        /// <summary>2 参派发：要求实体实现 <see cref="IAwakeAsync{P1, P2}"/>。</summary>
        internal static async Promise EntityAwakeAsync<P1, P2>(Entity self, P1 p1, P2 p2)
        {
            if (self is not IAwakeAsync<P1, P2>)
            {
                return;
            }

            var iAwakeAsyncSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(self.GetType(), typeof(IAwakeAsyncSystem<P1, P2>));
            if (iAwakeAsyncSystems == null)
            {
                return;
            }

            EntityRef<Entity> selfRef = self;
            foreach (IAwakeAsyncSystem<P1, P2> aAwakeAsyncSystem in iAwakeAsyncSystems)
            {
                if (aAwakeAsyncSystem == null)
                {
                    continue;
                }

                try
                {
                    await aAwakeAsyncSystem.Run(selfRef, p1, p2);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        /// <summary>3 参派发：要求实体实现 <see cref="IAwakeAsync{P1, P2, P3}"/>。</summary>
        internal static async Promise EntityAwakeAsync<P1, P2, P3>(Entity self, P1 p1, P2 p2, P3 p3)
        {
            if (self is not IAwakeAsync<P1, P2, P3>)
            {
                return;
            }

            var iAwakeAsyncSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(self.GetType(), typeof(IAwakeAsyncSystem<P1, P2, P3>));
            if (iAwakeAsyncSystems == null)
            {
                return;
            }

            EntityRef<Entity> selfRef = self;
            foreach (IAwakeAsyncSystem<P1, P2, P3> aAwakeAsyncSystem in iAwakeAsyncSystems)
            {
                if (aAwakeAsyncSystem == null)
                {
                    continue;
                }

                try
                {
                    await aAwakeAsyncSystem.Run(selfRef, p1, p2, p3);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        /// <summary>4 参派发：要求实体实现 <see cref="IAwakeAsync{P1, P2, P3, P4}"/>。</summary>
        internal static async Promise EntityAwakeAsync<P1, P2, P3, P4>(Entity self, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            if (self is not IAwakeAsync<P1, P2, P3, P4>)
            {
                return;
            }

            var iAwakeAsyncSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(self.GetType(), typeof(IAwakeAsyncSystem<P1, P2, P3, P4>));
            if (iAwakeAsyncSystems == null)
            {
                return;
            }

            EntityRef<Entity> selfRef = self;
            foreach (IAwakeAsyncSystem<P1, P2, P3, P4> aAwakeAsyncSystem in iAwakeAsyncSystems)
            {
                if (aAwakeAsyncSystem == null)
                {
                    continue;
                }

                try
                {
                    await aAwakeAsyncSystem.Run(selfRef, p1, p2, p3, p4);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        /// <summary>5 参派发：要求实体实现 <see cref="IAwakeAsync{P1, P2, P3, P4, P5}"/>。</summary>
        internal static async Promise EntityAwakeAsync<P1, P2, P3, P4, P5>(Entity self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            if (self is not IAwakeAsync<P1, P2, P3, P4, P5>)
            {
                return;
            }

            var iAwakeAsyncSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(self.GetType(), typeof(IAwakeAsyncSystem<P1, P2, P3, P4, P5>));
            if (iAwakeAsyncSystems == null)
            {
                return;
            }

            EntityRef<Entity> selfRef = self;
            foreach (IAwakeAsyncSystem<P1, P2, P3, P4, P5> aAwakeAsyncSystem in iAwakeAsyncSystems)
            {
                if (aAwakeAsyncSystem == null)
                {
                    continue;
                }

                try
                {
                    await aAwakeAsyncSystem.Run(selfRef, p1, p2, p3, p4, p5);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}
