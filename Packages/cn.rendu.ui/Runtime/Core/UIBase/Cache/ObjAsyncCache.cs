using System;
using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 异步对象缓存池
    /// </summary>
    public class ObjAsyncCache<T>
    {
        private EntityRef<Entity> m_EntityRef;
        public Entity Entity => m_EntityRef;

        private readonly Stack<T> m_Pool;
        private readonly Func<Promise<T>> m_CreateCallback;

        private float m_CreateInterval = 0f;
        private float m_LastCreateTime = 0f;

        public ObjAsyncCache(Entity entity, Func<Promise<T>> createCallback, int capacity = 0, float createInterval = 0f)
        {
            m_EntityRef = entity;
            m_Pool = capacity > 0 ? new Stack<T>(capacity) : new Stack<T>();
            m_CreateInterval = createInterval;
            m_CreateCallback = createCallback;
        }

        public void ChangeCreateInterval(float createInterval)
        {
            m_CreateInterval = createInterval;
        }

        public async Promise<T> Get()
        {
            return m_Pool.Count > 0 ? m_Pool.Pop() : await Create();
        }

        private async Promise<T> Create()
        {
            if (m_CreateCallback == null)
            {
                Log.Error($" CreateCallback == null ");
                return default;
            }

            if (m_CreateInterval > 0)
            {
                using var _ = await EventSystem.Instance.UIInvokeEntityAsync<UIInvokeEntity_CoroutineLock, Promise<Entity>>(Entity, new UIInvokeEntity_CoroutineLock { Lock = this.GetHashCode() });

                if (m_LastCreateTime > 0)
                {
                    var waitTime = m_LastCreateTime - UnityEngine.Time.time;
                    if (waitTime > 0)
                    {
                        await EventSystem.Instance.UIInvokeEntityAsync<UIInvokeEntity_WaitAsync, Promise>(Entity, new UIInvokeEntity_WaitAsync
                        {
                            Time = (long)(waitTime * 1000)
                        });
                    }
                }

                var createBeforeTime = UnityEngine.Time.time;
                var item = await m_CreateCallback.Invoke();
                var currentTime = UnityEngine.Time.time;
                var residueTime = m_CreateInterval - (currentTime - createBeforeTime);
                m_LastCreateTime = residueTime <= 0 ? 0 : currentTime + residueTime;
                return item;
            }
            else
            {
                return await m_CreateCallback();
            }
        }

        public void Put(T value)
        {
            m_Pool.Push(value);
        }

        public void Clear(bool disposeItem = false)
        {
            if (disposeItem)
            {
                foreach (var item in m_Pool)
                {
                    if (item is IDisposer disposer)
                    {
                        disposer.Dispose();
                    }
                    else if (item is IDisposable disposer2)
                    {
                        disposer2.Dispose();
                    }
                }
            }

            m_Pool.Clear();
        }

        public void Clear(Action<T> disposeAction)
        {
            while (m_Pool.Count >= 1)
            {
                disposeAction?.Invoke(m_Pool.Pop());
            }

            m_Pool.Clear();
        }

        public int Count => m_Pool.Count;
    }
}