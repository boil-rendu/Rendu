using System;
using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    /// <summary>RefPool 的分类型存储集合（对单一 IRefPool 类型维护一个空闲队列）</summary>
    public static partial class RefPool
    {
        private sealed class RefCollection
        {
            private readonly Queue<IRefPool> m_Refs;
            private readonly Type            m_RefType;

            public RefCollection(Type refType)
            {
                m_Refs    = new Queue<IRefPool>();
                m_RefType = refType;
            }

            public Type RefType
            {
                get { return m_RefType; }
            }

            /// <summary>取一个实例（队列空则 new）；类型不匹配直接抛异常</summary>
            public T Get<T>() where T : class, IRefPool, new()
            {
                if (typeof(T) != m_RefType)
                {
                    throw new Exception("类型无效");
                }

                lock (m_Refs)
                {
                    if (m_Refs.Count > 0)
                    {
                        return (T)m_Refs.Dequeue();
                    }
                }

                return new T();
            }

            /// <summary>取一个实例（非泛型版本，队列空则按记录的类型反射创建）</summary>
            public IRefPool Get()
            {
                lock (m_Refs)
                {
                    if (m_Refs.Count > 0)
                    {
                        return m_Refs.Dequeue();
                    }
                }

                return (IRefPool)Activator.CreateInstance(m_RefType);
            }

            /// <summary>归还实例：先 Recycle，再入队；已在队列中则返回 false（避免重复归还）</summary>
            public bool Put(IRefPool iRef)
            {
                iRef.Recycle();
                lock (m_Refs)
                {
                    if (!m_Refs.Contains(iRef))
                    {
                        m_Refs.Enqueue(iRef);
                        return true;
                    }
                }

                return false;
            }

            /// <summary>预热：预先 new 出 count 个实例入队；类型不匹配直接抛异常</summary>
            public void Add<T>(int count) where T : class, IRefPool, new()
            {
                if (typeof(T) != m_RefType)
                {
                    throw new Exception("类型无效。");
                }

                lock (m_Refs)
                {
                    while (count-- > 0)
                    {
                        m_Refs.Enqueue(new T());
                    }
                }
            }

            /// <summary>预热（非泛型版本，按记录的类型反射创建）</summary>
            public void Add(int count)
            {
                lock (m_Refs)
                {
                    while (count-- > 0)
                    {
                        m_Refs.Enqueue((IRefPool)Activator.CreateInstance(m_RefType));
                    }
                }
            }

            /// <summary>丢弃至多 count 个空闲实例（不足则全部丢弃）</summary>
            public void Remove(int count)
            {
                lock (m_Refs)
                {
                    if (count > m_Refs.Count)
                    {
                        count = m_Refs.Count;
                    }

                    while (count-- > 0)
                    {
                        m_Refs.Dequeue();
                    }
                }
            }

            /// <summary>清空该类型的全部空闲实例</summary>
            public void RemoveAll()
            {
                lock (m_Refs)
                {
                    m_Refs.Clear();
                }
            }
        }
    }
}
