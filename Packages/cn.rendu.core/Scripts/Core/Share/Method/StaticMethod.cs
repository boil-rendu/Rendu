using System.Reflection;

namespace RD.Core
{
    /// <summary>
    /// <see cref="IStaticMethod"/> 的具体实现：构造时从 <see cref="Assembly"/> 反射拿到 <c>(Type, MethodName)</c> 描述的静态方法，
    /// 调用时 <see cref="MethodInfo.Invoke"/>。参数以 <c>object</c> 数组缓存复用，避免每次 <see cref="Run(object[])"/> 重新分配数组。
    /// </summary>
    /// <remarks>典型场景：热更 DLL 加载后用 <c>(HotfixAssembly, "ClassName", "MethodName")</c> 反射调起入口方法。</remarks>
    public class StaticMethod : IStaticMethod
    {
        private readonly MethodInfo methodInfo;

        private readonly object[] param;

        /// <summary>按 (程序集, 类型全名, 方法名) 反射拿到目标静态方法，并按参数数量预分配参数数组。</summary>
        /// <param name="assembly">目标程序集。</param>
        /// <param name="typeName">类型全名（含命名空间）。</param>
        /// <param name="methodName">静态方法名。</param>
        public StaticMethod(Assembly assembly, string typeName, string methodName)
        {
            this.methodInfo = assembly.GetType(typeName).GetMethod(methodName);
            this.param = new object[this.methodInfo.GetParameters().Length];
        }

        /// <inheritdoc />
        public override void Run()
        {
            this.methodInfo.Invoke(null, param);
        }

        /// <inheritdoc />
        public override void Run(object a)
        {
            this.param[0] = a;
            this.methodInfo.Invoke(null, param);
        }

        /// <inheritdoc />
        public override void Run(object a, object b)
        {
            this.param[0] = a;
            this.param[1] = b;
            this.methodInfo.Invoke(null, param);
        }

        /// <inheritdoc />
        public override void Run(object a, object b, object c)
        {
            this.param[0] = a;
            this.param[1] = b;
            this.param[2] = c;
            this.methodInfo.Invoke(null, param);
        }
    }
}
