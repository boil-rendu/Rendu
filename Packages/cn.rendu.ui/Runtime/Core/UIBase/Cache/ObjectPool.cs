using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 通用对象池（内部用 Stack 缓存空闲元素）
    /// 编辑器下额外提供引用追踪与重复归还/非池来源的告警
    /// </summary>
    public class ObjectPool<T> where T : new()
    {
        #if UNITY_EDITOR
        public static    bool       isRunning = false;
        private readonly HashSet<T> Trace     = new HashSet<T>();
        #endif

        private readonly Stack<T>       m_Stack = new Stack<T>();
        private readonly UnityAction<T> m_ActionOnGet;
        private readonly UnityAction<T> m_ActionOnRelease;

        /// <summary>累计创建过的对象数</summary>
        public int countAll { get; private set; }

        /// <summary>当前被借出（活跃）的对象数</summary>
        public int countActive
        {
            get { return countAll - countInactive; }
        }

        /// <summary>当前池内空闲的对象数</summary>
        public int countInactive
        {
            get { return m_Stack.Count; }
        }

        public ObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease)
        {
            m_ActionOnGet     = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
        }

        /// <summary>取一个元素：池空则新建，取出后执行 actionOnGet</summary>
        public T Get()
        {
            T element;
            if (m_Stack.Count == 0)
            {
                element = new T();
                countAll++;
                #if UNITY_EDITOR
                Trace.Add(element);
                #endif
            }
            else
            {
                element = m_Stack.Pop();
            }

            if (m_ActionOnGet != null)
                m_ActionOnGet(element);
            return element;
        }

        /// <summary>
        /// 归还元素：先执行 actionOnRelease 再入栈
        /// 编辑器下检查「连续重复归还」与「非本池来源」；isRunning 为 false 时只执行回调不入栈
        /// </summary>
        public void Release(T element)
        {
            #if UNITY_EDITOR
            if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            if (element == null)
            {
                Debug.LogError("Internal error. Release element is null.");
                return;
            }

            if (ObjectPool<bool>.isRunning == false)
            {
                if (m_ActionOnRelease != null)
                    m_ActionOnRelease(element);
                return;
            }

            if (Trace.Contains(element) == false)
            {
                Debug.LogError("Internal error. Release element is not from pool ");
            }
            #endif

            if (m_ActionOnRelease != null)
                m_ActionOnRelease(element);
            m_Stack.Push(element);
        }
    }
}
