using System;

namespace RD.Core
{
    /// <summary>
    /// 标记一个 <see cref="AEvent{S,A}"/> 处理类只对特定 <c>SceneType</c> 生效。
    /// 在 <see cref="EventSystem.Awake"/> 阶段被收集到 <c>allEvents[eventType]</c> 列表，<see cref="EventSystem.PublishAsync"/> / <see cref="EventSystem.Publish"/> 派发时由 <see cref="SceneTypeSingleton.IsSame"/> 过滤。
    /// </summary>
    public class EventAttribute: BaseAttribute
    {
        /// <summary>归属的场景类型（与 <c>SceneType</c> 常量对应，0 表示通配）。</summary>
        public int SceneType { get; }

        public EventAttribute(int sceneType)
        {
            this.SceneType = sceneType;
        }
    }
}
