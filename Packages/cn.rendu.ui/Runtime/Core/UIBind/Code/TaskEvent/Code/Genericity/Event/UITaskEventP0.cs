using RD;
using System;
using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 异步 UI 事件（0 参）
    /// 用 LinkedList 保存句柄；调用时收集所有 Promise 后用 PromiseHelper.WaitAll 并发等待
    /// </summary>
    public class UITaskEventP0 : UIEventBase, IUITaskEventInvoke
    {
        private LinkedList<UITaskEventHandleP0> m_UITaskEventHandles;
        /// <summary>当前所有句柄（可空）</summary>
        public  LinkedList<UITaskEventHandleP0> UITaskEventHandles => m_UITaskEventHandles;

        public UITaskEventP0()
        {
        }

        public UITaskEventP0(string name) : base(name)
        {
        }

        /// <summary>异步派发事件（0 参）：并发等待全部回调，异常只记录不抛出</summary>
        public async Promise Invoke()
        {
            if (m_UITaskEventHandles == null)
            {
                Logger.LogWarning($"{EventName} 未绑定任何事件");
                return;
            }

            using var list = ListComponent<Promise>.Create();

            var handle = m_UITaskEventHandles.First;
            while (handle != null)
            {
                var next  = handle.Next;
                var value = handle.Value;

                if (value != null)
                {
                    list.Add(value.Invoke());
                }

                handle = next;
            }

            try
            {
                await PromiseHelper.WaitAll(list);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        /// <summary>异步事件</summary>
        public override bool IsTaskEvent => true;

        /// <summary>清空：归还所有句柄到池并释放链表</summary>
        public override bool Clear()
        {
            if (m_UITaskEventHandles == null) return false;

            var first = m_UITaskEventHandles.First;
            while (first != null)
            {
                PublicUITaskEventP0.HandlerPool.Release(first.Value);
                first = m_UITaskEventHandles.First;
            }

            LinkedListPool<UITaskEventHandleP0>.Release(m_UITaskEventHandles);
            m_UITaskEventHandles = null;
            return true;
        }

        /// <summary>添加句柄：由 Entity + Invoke 类型名触发</summary>
        public UITaskEventHandleP0 Add(Entity trigger, string onEventInvokeType)
        {
            m_UITaskEventHandles ??= LinkedListPool<UITaskEventHandleP0>.Get();
            var handler = PublicUITaskEventP0.HandlerPool.Get();
            var node    = m_UITaskEventHandles.AddLast(handler);
            return handler.Init(m_UITaskEventHandles, node, trigger, onEventInvokeType);
        }

        /// <summary>添加句柄：由回调委托触发（空回调会报错）</summary>
        public UITaskEventHandleP0 Add(UITaskEventDelegate callback)
        {
            m_UITaskEventHandles ??= LinkedListPool<UITaskEventHandleP0>.Get();

            if (callback == null)
            {
                Logger.LogError($"{EventName} 添加了一个空回调");
            }

            var handler = PublicUITaskEventP0.HandlerPool.Get();
            var node    = m_UITaskEventHandles.AddLast(handler);
            return handler.Init(m_UITaskEventHandles, node, callback);
        }

        /// <summary>移除句柄</summary>
        public bool Remove(UITaskEventHandleP0 handle)
        {
            m_UITaskEventHandles ??= LinkedListPool<UITaskEventHandleP0>.Get();

            if (handle == null)
            {
                Logger.LogError($"{EventName} UITaskEventParamHandle == null");
                return false;
            }

            return m_UITaskEventHandles.Remove(handle);
        }

        #if UNITY_EDITOR
        /// <summary>事件类型名（编辑器用）</summary>
        public override string GetEventType()
        {
            return "UITaskEventP0";
        }

        /// <summary>句柄类型名（编辑器用）</summary>
        public override string GetEventHandleType()
        {
            return "UITaskEventHandleP0";
        }
        #endif
    }
}
