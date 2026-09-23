using RD.Core;
namespace RD.Client
{
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
