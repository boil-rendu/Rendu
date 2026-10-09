namespace RD.Core
{
    /// <summary>标记一个 <see cref="ASingleton"/> 在 <see cref="World.Dispose"/> 阶段按 LIFO 倒序释放（与「普通先入先出 Dispose」区分）。</summary>
    /// <remarks>实际只是个空标记接口，<see cref="World.AddSingleton"/> 检测到后压入 <c>stack</c>，<see cref="World.Dispose"/> 弹栈时优先 Dispose。</remarks>
    public interface ISingletonReverseDispose
    {

    }

    /// <summary>单例基类：持有 <c>Register</c> 抽象钩子和 <see cref="DisposeObject"/> 释放能力，由 <see cref="World.AddSingleton(ASingleton)"/> 调 <c>Register</c> 完成自注册。</summary>
    public abstract class ASingleton: DisposeObject
    {
        /// <summary>由 <see cref="World.AddSingleton(ASingleton)"/> 调用：派生类在此赋值自己的 <c>Instance</c> 静态字段。</summary>
        internal abstract void Register();
    }

    /// <summary>泛型单例模板：<see cref="Instance"/> 静态属性懒获取，<see cref="Dispose"/> 走幂等检查并调用 <see cref="Destroy"/> 钩子。</summary>
    /// <remarks>与 <see cref="World"/> 的区别：本类自管生命周期（<c>Instance</c> = <c>null</c> 之前不释放）；<see cref="World"/> 是容器，管理多个 <see cref="ASingleton"/>。</remarks>
    /// <typeparam name="T">具体单例类型（CRTP 约束，保证 <c>Singleton&lt;T&gt;.Instance</c> 类型正确）。</typeparam>
    public abstract class Singleton<T>: ASingleton where T: Singleton<T>
    {
        private bool isDisposed;

        [StaticField]
        private static T instance;

        /// <summary>当前单例实例（可能为 <c>null</c>，业务侧需自行 <c>!= null</c> 判空或确保 <see cref="World.AddSingleton{T}()"/> 先调）。</summary>
        [StaticField]
        public static T Instance
        {
            get
            {
                return instance;
            }
            private set
            {
                instance = value;
            }
        }

        /// <inheritdoc />
        internal override void Register()
        {
            Instance = (T)this;
        }

        /// <summary>是否已 Dispose：与 <see cref="Dispose"/> 配合实现幂等。</summary>
        public bool IsDisposed()
        {
            return this.isDisposed;
        }

        /// <summary>派生类重写：Dispose 阶段的资源释放钩子，基类默认空实现。</summary>
        protected virtual void Destroy()
        {

        }

        /// <summary>幂等释放：先调 <see cref="Destroy"/>，再置空 <see cref="Instance"/>；重复调用安全。</summary>
        public override void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            this.Destroy();

            Instance = null;
        }
    }
}
