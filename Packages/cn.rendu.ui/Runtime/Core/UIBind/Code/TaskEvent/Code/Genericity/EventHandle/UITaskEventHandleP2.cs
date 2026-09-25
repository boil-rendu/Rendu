using RD;
using System;
using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI事件全局对象池
    /// </summary>
    public static class PublicUITaskEventP2<P1, P2>
    {
        public static readonly ObjectPool<UITaskEventHandleP2<P1, P2>> HandlerPool = new(null, handler => handler.Dispose());
    }

    /// <summary>
    /// UI事件 2个泛型参数
    /// </summary>
    public sealed class UITaskEventHandleP2<P1, P2>
    {
        private LinkedList<UITaskEventHandleP2<P1, P2>>     m_UITaskEventList;
        private LinkedListNode<UITaskEventHandleP2<P1, P2>> m_UITaskEventNode;

        public UITaskEventDelegate<P1, P2> UITaskEventParamDelegate { get; private set; }

        private EntityRef<Entity> m_Trigger;
        public  Entity            Trigger => m_Trigger;

        public string OnEventInvokeType { get; private set; }

        // 统一 Invoke 键：字符串只在绑定期哈希一次，调用期走 long
        private long m_InvokeId;

        public UITaskEventHandleP2()
        {
        }

        internal UITaskEventHandleP2<P1, P2> Init(LinkedList<UITaskEventHandleP2<P1, P2>> uiTaskEventList, LinkedListNode<UITaskEventHandleP2<P1, P2>> uiTaskEventNode, Entity trigger, string onEventInvokeType)
        {
            m_UITaskEventList = uiTaskEventList;
            m_UITaskEventNode = uiTaskEventNode;
            m_Trigger         = trigger;
            OnEventInvokeType = onEventInvokeType;
            m_InvokeId        = InvokeId.Of(onEventInvokeType);
            return this;
        }

        internal UITaskEventHandleP2<P1, P2> Init(LinkedList<UITaskEventHandleP2<P1, P2>> uiTaskEventList, LinkedListNode<UITaskEventHandleP2<P1, P2>> uiTaskEventNode, UITaskEventDelegate<P1, P2> uiTaskEventDelegate)
        {
            m_UITaskEventList        = uiTaskEventList;
            m_UITaskEventNode        = uiTaskEventNode;
            UITaskEventParamDelegate = uiTaskEventDelegate;
            return this;
        }

        internal async Promise Invoke(P1 p1, P2 p2)
        {
            if (OnEventInvokeType != null)
            {
                if (Trigger == null)
                {
                    Log.Error($"事件:{OnEventInvokeType} Trigger == null");
                    return;
                }

                if (!EventSystem.Instance.TryInvokeEntity(Trigger, m_InvokeId, new InvokeArgs<P1, P2>(p1, p2), out Promise promise))
                {
                    Log.Error($"未找到 Invoke 实现请检查 {OnEventInvokeType} 类型:InvokeArgs<{typeof(P1).Name},{typeof(P2).Name}>");
                    return;
                }
                await promise;
            }
            else if (UITaskEventParamDelegate != null)
            {
                try
                {
                    await UITaskEventParamDelegate.Invoke(p1, p2);
                }
                catch (Exception e)
                {
                    Logger.LogError($"委托:{UITaskEventParamDelegate.GetType().Name} 委托回调错误: {e}");
                }
            }
            else
            {
                Logger.LogError($"没有实现事件 也没有实现委托 请检查");
            }
        }

        public void Dispose()
        {
            OnEventInvokeType        = null;
            m_InvokeId               = 0;
            UITaskEventParamDelegate = null;
            m_Trigger                = default;
            if (m_UITaskEventList == null || m_UITaskEventNode == null) return;
            m_UITaskEventList.Remove(m_UITaskEventNode);
            m_UITaskEventNode = null;
            m_UITaskEventList = null;
        }
    }
}
