using System;
using System.Collections.Generic;

namespace RD.Core
{
    /// <summary>
    /// 池化的 <see cref="Dictionary{TKey,TValue}"/>：<see cref="IDisposable.Dispose"/> 时自动 <see cref="Dictionary{TKey,TValue}.Clear"/> + <see cref="ObjectPool.Recycle"/> 回收，
    /// <see cref="Create"/> 工厂方法从 <see cref="ObjectPool"/> 取实例，避免每次使用 <c>Dictionary</c> 都分配。
    /// </summary>
    /// <remarks>
    /// 使用模式：<c>using var dict = DictionaryComponent&lt;int, Entity&gt;.Create();</c> → 作用域结束自动回池。
    /// </remarks>
    /// <typeparam name="T">Key 类型。</typeparam>
    /// <typeparam name="V">Value 类型。</typeparam>
    public class DictionaryComponent<T, V> : Dictionary<T, V>, IPool
    {
        public DictionaryComponent()
        {
        }

        /// <summary>从 <see cref="ObjectPool"/> 取一个空（或已 <c>Clear</c>）的 <see cref="DictionaryComponent{T,V}"/>；无池实例时新建。</summary>
        public static DictionaryComponent<T, V> Create()
        {
            return ObjectPool.Fetch(typeof(DictionaryComponent<T, V>)) as DictionaryComponent<T, V>;
        }

        /// <summary>释放：先 <c>Clear</c> 解除引用 + 回 <see cref="ObjectPool"/>。配合 <c>using</c> 使用可自动归还。</summary>
        public void Dispose()
        {
            this.Clear();
            ObjectPool.Recycle(this);
        }

        /// <inheritdoc />
        public bool IsFromPool { get; set; }
    }
}
