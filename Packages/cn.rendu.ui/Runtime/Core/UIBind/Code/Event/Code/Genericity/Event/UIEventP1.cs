using RD;
using System;
using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 同步 UI 事件（1 参）
    /// 用 LinkedList 保存句柄，调用时先取 next 再执行（允许回调中增删）；句柄与链表均走池
    /// </summary>
    public class UIEventP1<P1> : UIEventBase, IUIEventInvoke<P1>
    {
        private LinkedList<UIEventHandleP1<P1>> m_UIEventHandles;
        /// <summary>当前所有句柄（可空）</summary>
        public  LinkedList<UIEventHandleP1<P1>> UIEventHandles => m_UIEventHandles;

        public UIEventP1()
        {
        }

        public UIEventP1(string name) : base(name)
        {
        }

        /// <summary>派发事件（1 参）</summary>
        public void Invoke(P1 p1)
        {
            if (m_UIEventHandles == null)
            {
                Logger.LogWarning($"{EventName} 未绑定任何事件");
                return;
            }

            var handle = m_UIEventHandles.First;
            while (handle != null)
            {
                var next  = handle.Next;
                var value = handle.Value;

                if (value != null)
                {
                    if (!value.Invoke(p1))
                    {
                        Logger.LogError($"UI事件名称:{EventName} 执行错误 请配合上面报错信息排查");
                    }
                }

                handle = next;
            }
        }

        /// <summary>非异步事件</summary>
        public override bool IsTaskEvent => false;

        /// <summary>清空：归还所有句柄到池并释放链表</summary>
        public override bool Clear()
        {
            if (m_UIEventHandles == null) return false;

            var first = m_UIEventHandles.First;
            while (first != null)
            {
                PublicUIEventP1<P1>.HandlerPool.Release(first.Value);
                first = m_UIEventHandles.First;
            }

            LinkedListPool<UIEventHandleP1<P1>>.Release(m_UIEventHandles);
            m_UIEventHandles = null;
            return true;
        }

        /// <summary>添加句柄：由 Entity + Invoke 类型名触发</summary>
        public UIEventHandleP1<P1> Add(Entity trigger, string onEventInvokeType)
        {
            m_UIEventHandles ??= LinkedListPool<UIEventHandleP1<P1>>.Get();
            var handler = PublicUIEventP1<P1>.HandlerPool.Get();
            var node    = m_UIEventHandles.AddLast(handler);
            return handler.Init(m_UIEventHandles, node, trigger, onEventInvokeType);
        }

        /// <summary>添加句柄：由回调委托触发（空回调会报错）</summary>
        public UIEventHandleP1<P1> Add(UIEventDelegate<P1> callback)
        {
            m_UIEventHandles ??= LinkedListPool<UIEventHandleP1<P1>>.Get();

            if (callback == null)
            {
                Logger.LogError($"{EventName} 添加了一个空回调");
            }

            var handler = PublicUIEventP1<P1>.HandlerPool.Get();
            var node    = m_UIEventHandles.AddLast(handler);
            return handler.Init(m_UIEventHandles, node, callback);
        }

        /// <summary>移除句柄</summary>
        public bool Remove(UIEventHandleP1<P1> handle)
        {
            m_UIEventHandles ??= LinkedListPool<UIEventHandleP1<P1>>.Get();

            if (handle == null)
            {
                Logger.LogError($"{EventName} UIEventParamHandle == null");
                return false;
            }

            return m_UIEventHandles.Remove(handle);
        }

        #if UNITY_EDITOR
        /// <summary>事件类型名（编辑器用，如 UIEventP1&lt;int&gt;）</summary>
        public override string GetEventType()
        {
            return $"UIEventP1<{GetParamTypeString(0)}>";
        }

        /// <summary>句柄类型名（编辑器用）</summary>
        public override string GetEventHandleType()
        {
            return $"UIEventHandleP1<{GetParamTypeString(0)}>";
        }
        #endif
    }
}
