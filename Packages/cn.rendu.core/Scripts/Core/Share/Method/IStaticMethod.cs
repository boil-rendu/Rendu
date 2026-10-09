namespace RD.Core
{
    /// <summary>
    /// 静态方法反射调用的抽象基类：把任意「按字符串名反射到的静态方法」包装成可统一调用的对象。
    /// 4 个 <see cref="Run(object[])"/> 重载覆盖 0~3 参场景，参数以 <c>object</c> 数组传递（避免编译期依赖参数类型，由反射在运行时强转）。
    /// </summary>
    /// <remarks>派生：<see cref="StaticMethod"/>（具体反射实现）。</remarks>
    public abstract class IStaticMethod
    {
        /// <summary>无参调用。</summary>
        public abstract void Run();
        /// <summary>1 参调用。</summary>
        public abstract void Run(object a);
        /// <summary>2 参调用。</summary>
        public abstract void Run(object a, object b);
        /// <summary>3 参调用。</summary>
        public abstract void Run(object a, object b, object c);
    }
}
