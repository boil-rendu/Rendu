using RD.Utilities;

namespace RD.Serialization.Editor
{
    /// <summary>
    /// 深拷贝门面: <see cref="CreateCopy{T}"/> 提供通用深拷贝能力。
    /// 实现转调 [cn.rendu.utilities](../cn.rendu.utilities/README.md) 的反射深拷
    /// (<c>ObjectExtensions.Clone</c>) —— 全仓只有一份深拷实现, 这里只做迁移兼容的入口。
    /// </summary>
    public static class SerializationUtility
    {
        /// <summary>深拷任意对象(含环引用兜底与 64 层深度截断, 语义同 Clone)。
        /// 显式走扩展方法而非 <c>source.Clone()</c>: 后者在目标类型实现 ICloneable 时
        /// 会让实例方法优先, 静默改变深拷语义。</summary>
        public static T CreateCopy<T>(T source)
        {
            return ObjectExtensions.Clone(source);
        }
    }
}
