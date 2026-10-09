using System.Diagnostics;

namespace RD.Core
{
    internal sealed class ComponentsCollectionDebugView
    {
        private readonly ComponentsCollection _collection;

        public ComponentsCollectionDebugView(ComponentsCollection collection)
        {
            ArgumentNullException.ThrowIfNull(collection);

            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public Entity[] Items
        {
            get
            {
                Entity[] items = new Entity[_collection.Count];
                _collection.Values.CopyTo(items, 0);
                return items;
            }
        }
    }

    /// <summary>
    /// Entity 的 Component 容器（按 <see cref="Entity.InstanceId"/> 排序的字典），支持对象池复用。
    /// 派生 Entity 通过 <c>AddComponent</c>/<c>GetComponent</c> 维护此集合。
    /// </summary>
    [DebuggerTypeProxy(typeof(ComponentsCollectionDebugView))]
    [DebuggerDisplay("Count = {Count}")]
    public class ComponentsCollection : SortedDictionary<long, Entity>, IPool
    {
        /// <summary>从池中获取（或新建）一个 <see cref="ComponentsCollection"/>。</summary>
        public static ComponentsCollection Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<ComponentsCollection>(isFromPool);
        }

        /// <summary>此实例是否来自对象池。</summary>
        public bool IsFromPool { get; set; }

        /// <summary>归还到对象池（或仅 <see cref="SortedDictionary{TKey,TValue}.Clear"/>，若非池化实例）。</summary>
        public void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.IsFromPool = false;
            this.Clear();

            ObjectPool.Recycle(this);
        }
    }
}
