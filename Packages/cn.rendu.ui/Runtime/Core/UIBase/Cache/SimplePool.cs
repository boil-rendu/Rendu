using RD.Core;
namespace RD.Client
{
    /// <summary>最简单的一类对象池：直接 new 出 T，交给 ObjCache 管理创建与缓存</summary>
    public class SimplePool<T> : ObjCache<T> where T : new()
    {
        public SimplePool(int capacity = 0) : base(null, capacity)
        {
            m_createCallback = Create;
        }

        private T Create()
        {
            return new T();
        }
    }
}
