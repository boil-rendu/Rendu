using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>单条 UI 消息的分发信息（消息类型 + 目标组件名 + 反射创建的处理实例）</summary>
    //消息分发信息
    [EnableClass]
    public class UIEventInfo
    {
        public Type             EventType     { get; } //消息类型
        public string           ComponentName { get; } //UI组件类型 你要具体监听的那个组件是谁
        public IUICommonEvent UIEvent       { get; } //反射创建的消息处理实例

        public UIEventInfo(Type eventType, string componentName, IUICommonEvent uiEvent)
        {
            EventType     = eventType;
            ComponentName = componentName;
            UIEvent       = uiEvent;
        }
    }
}
