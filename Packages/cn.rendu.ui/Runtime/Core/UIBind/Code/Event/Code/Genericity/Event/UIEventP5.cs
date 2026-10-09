using RD;
using System;
using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 同步 UI 事件（5 参）
    /// 用 LinkedList 保存句柄，调用时先取 next 再执行（允许回调中增删）；句柄与链表均走池
    /// </summary>
    public class UIEventP5<P1, P2, P3, P4, P5> : UIEventBase, IUIEventInvoke<P1, P2, P3, P4, P5>
    {
        private LinkedList<UIEventHandleP5<P1, P2, P3, P4, P5>> m_UIEventHandles;
        /// <summary>当前所有句柄（可空）</summary>
        public  LinkedList<UIEventHandleP5<P1, P2, P3, P4, P5>> UIEventHandles => m_UIEventHandles;

        public UIEventP5()
        {
        }

        public UIEventP5(string name) : base(name)
        {
        }

        /// <summary>派发事件（5 参）</summary>
        public void Invoke(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
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
                    if (!value.Invoke(p1, p2, p3, p4, p5))
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
                PublicUIEventP5<P1, P2, P3, P4, P5>.HandlerPool.Release(first.Value);
                first = m_UIEventHandles.First;
            }

            LinkedListPool<UIEventHandleP5<P1, P2, P3, P4, P5>>.Release(m_UIEventHandles);
            m_UIEventHandles = null;
            return true;
        }

        /// <summary>添加句柄：由 Entity + Invoke 类型名触发</summary>
        public UIEventHandleP5<P1, P2, P3, P4, P5> Add(Entity trigger, string onEventInvokeType)
        {
            m_UIEventHandles ??= LinkedListPool<UIEventHandleP5<P1, P2, P3, P4, P5>>.Get();
            var handler = PublicUIEventP5<P1, P2, P3, P4, P5>.HandlerPool.Get();
            var node    = m_UIEventHandles.AddLast(handler);
            return handler.Init(m_UIEventHandles, node, trigger, onEventInvokeType);
        }

        /// <summary>添加句柄：由回调委托触发（空回调会报错）</summary>
        public UIEventHandleP5<P1, P2, P3, P4, P5> Add(UIEventDelegate<P1, P2, P3, P4, P5> callback)
        {
            m_UIEventHandles ??= LinkedListPool<UIEventHandleP5<P1, P2, P3, P4, P5>>.Get();

            if (callback == null)
            {
                Logger.LogError($"{EventName} 添加了一个空回调");
            }

            var handler = PublicUIEventP5<P1, P2, P3, P4, P5>.HandlerPool.Get();
            var node    = m_UIEventHandles.AddLast(handler);
            return handler.Init(m_UIEventHandles, node, callback);
        }

        /// <summary>移除句柄</summary>
        public bool Remove(UIEventHandleP5<P1, P2, P3, P4, P5> handle)
        {
            m_UIEventHandles ??= LinkedListPool<UIEventHandleP5<P1, P2, P3, P4, P5>>.Get();

            if (handle == null)
            {
                Logger.LogError($"{EventName} UIEventParamHandle == null");
                return false;
            }

            return m_UIEventHandles.Remove(handle);
        }

        #if UNITY_EDITOR
        /// <summary>事件类型名（编辑器用）</summary>
        public override string GetEventType()
        {
            return $"UIEventP5<{GetParamTypeString(0)},{GetParamTypeString(1)},{GetParamTypeString(2)},{GetParamTypeString(3)},{GetParamTypeString(4)}>";
        }

        /// <summary>句柄类型名（编辑器用）</summary>
        public override string GetEventHandleType()
        {
            return $"UIEventHandleP5<{GetParamTypeString(0)},{GetParamTypeString(1)},{GetParamTypeString(2)},{GetParamTypeString(3)},{GetParamTypeString(4)}>";
        }
        #endif
    }
}
