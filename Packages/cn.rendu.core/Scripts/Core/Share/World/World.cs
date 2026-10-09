using System;
using System.Collections.Generic;

namespace RD.Core
{
    /// <summary>
    /// 单例容器：管理所有 <see cref="ASingleton"/> 派生类的注册/释放，<see cref="Instance"/> 是进程级唯一入口。
    /// 与 <see cref="Singleton{T}"/>（自管理单例）不同，本类是「宿主」：把各种零散的 <c>ASingleton</c> 装进来，并按
    /// <see cref="ISingletonReverseDispose"/> 标记记录压栈顺序，<see cref="Dispose"/> 时按 LIFO 释放。
    /// 同一进程可多次 <c>Dispose</c>/重建，实现 Server/Client 间的热重载。
    /// </summary>
    public class World: IDisposable
    {
        [StaticField]
        private static World instance;

        /// <summary>进程级 <see cref="World"/> 单例（懒构造）。</summary>
        [StaticField]
        public static World Instance
        {
            get
            {
                return instance ??= new World();
            }
        }

        private readonly Stack<Type> stack = new();
        private readonly Dictionary<Type, ASingleton> singletons = new();

        private World()
        {
        }

        /// <summary>释放所有已注册的 <see cref="ASingleton"/>：先按 <see cref="ISingletonReverseDispose"/> 入栈顺序 LIFO 弹出，再遍历剩余兜底 Dispose，最后置空 <see cref="Instance"/>。</summary>
        public void Dispose()
        {
            instance = null;

            lock (this)
            {
                while (this.stack.Count > 0)
                {
                    Type type = this.stack.Pop();
                    if (this.singletons.Remove(type, out ASingleton singleton))
                    {
                        singleton.Dispose();
                    }
                }

                // dispose剩下的singleton，主要为了把instance置空
                foreach (var kv in this.singletons)
                {
                    kv.Value.Dispose();
                }
            }
        }

        /// <summary>注册一个无参 Awakable 单例：先 <c>new</c> + <c>Awake()</c>，再纳入本 <see cref="World"/> 容器。</summary>
        /// <typeparam name="T">具体单例类型，必须实现 <see cref="ISingletonAwake"/> 并含无参构造函数。</typeparam>
        public T AddSingleton<T>() where T : ASingleton, ISingletonAwake, new()
        {
            T singleton = new();
            singleton.Awake();

            AddSingleton(singleton);
            return singleton;
        }

        /// <summary>注册一个 1 参 Awakable 单例。</summary>
        public T AddSingleton<T, A>(A a) where T : ASingleton, ISingletonAwake<A>, new()
        {
            T singleton = new();
            singleton.Awake(a);

            AddSingleton(singleton);
            return singleton;
        }

        /// <summary>注册一个 2 参 Awakable 单例。</summary>
        public T AddSingleton<T, A, B>(A a, B b) where T : ASingleton, ISingletonAwake<A, B>, new()
        {
            T singleton = new();
            singleton.Awake(a, b);

            AddSingleton(singleton);
            return singleton;
        }

        /// <summary>注册一个 3 参 Awakable 单例。</summary>
        public T AddSingleton<T, A, B, C>(A a, B b, C c) where T : ASingleton, ISingletonAwake<A, B, C>, new()
        {
            T singleton = new();
            singleton.Awake(a, b, c);

            AddSingleton(singleton);
            return singleton;
        }

        /// <summary>把已构造好的单例纳入容器：若实现 <see cref="ISingletonReverseDispose"/> 则记录入栈顺序供 <see cref="Dispose"/> 倒序释放，最后调用 <c>Register()</c> 让其设置自己的 <c>Instance</c>。</summary>
        /// <param name="singleton">已构造、已 <c>Awake</c> 完成的单例。</param>
        public void AddSingleton(ASingleton singleton)
        {
            lock (this)
            {
                Type type = singleton.GetType();
                if (singleton is ISingletonReverseDispose)
                {
                    this.stack.Push(type);
                }
                singletons[type] = singleton;
            }

            singleton.Register();
        }
    }
}
