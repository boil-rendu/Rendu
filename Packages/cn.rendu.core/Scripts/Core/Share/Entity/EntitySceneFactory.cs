namespace RD.Core
{
    /// <summary>Scene 工厂扩展：便捷创建 <see cref="Scene"/> 实例并挂入 parent。</summary>
    public static class EntitySceneFactory
    {
        /// <summary>创建 <see cref="Scene"/> 实例并挂为 <paramref name="parent"/> 的子 Entity。</summary>
        /// <param name="parent">父 Entity，可为 null（顶层 Scene）。</param>
        /// <param name="id">业务 ID。</param>
        /// <param name="instanceId">运行时实例 ID。</param>
        /// <param name="sceneType">Scene 类型枚举。</param>
        /// <param name="name">Scene 名称。</param>
        public static Scene CreateScene(Entity parent, long id, long instanceId, int sceneType, string name)
        {
            Scene scene = new(parent.Fiber(), id, instanceId, sceneType, name);
            parent?.AddChild(scene);
            return scene;
        }
    }
}
