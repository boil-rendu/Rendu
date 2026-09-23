using System;
using RD.Core;

namespace RD.Client
{
    public interface IUICommonEvent
    {
        /// <summary>
        /// 触发消息
        /// </summary>
        /// <param name="uiScene">当前UI所在的场景</param>
        /// <param name="message">你所监听的消息的数据</param>
        /// <returns></returns>
        Promise Run(Entity uiScene, object message);
    }

    [EnableClass]
    public abstract class UICommonEventSystem<T> : IUICommonEvent where T : struct
    {
        public async Promise Run(Entity uiScene, object message)
        {
            await this.Event((Scene)uiScene, (T)message);
        }

        protected abstract Promise Event(Scene uiScene, T message);
    }
}
