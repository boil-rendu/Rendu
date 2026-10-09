using System;
using RD.Core;

namespace RD.Client
{
    //UI上的消息
    /// <summary>UI 消息监听标记：声明「监听哪个 UI 消息」+「关注哪个 UI 组件」</summary>
    public class UIEventAttribute : BaseAttribute
    {
        public Type EventType     { get; } //消息类型
        public Type ComponentType { get; } //UI组件类型 你要具体监听的那个组件是谁

        public UIEventAttribute(Type eventType, Type componentType)
        {
            EventType     = eventType;
            ComponentType = componentType;
        }
    }

    /*//案例:
    //1:监听某个panel打开前的消息  2:这个panel是XXPanel
    [UIEventAttribut(typeof(UIEventPanelOpenBefore),typeof(XXPanel))]
    public class UIEventPanelOpenLoginPanelBefore : UICommonEventSystem<UIEventPanelOpenBefore>
    {
        protected override async Promise Event(Scene uiScene, UIEventPanelOpenBefore message)
        {
            await Promise.CompletedTask;
            Log.Error($" XXPanel 打开前");
        }
    }*/
}
