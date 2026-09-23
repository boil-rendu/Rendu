namespace RD.Core
{
    /// <summary>
    /// Fiber 初始化上下文载体：由 <see cref="FiberManager.Create"/> 在纤程线程内通过 EventSystem 按 sceneType 分发给对应的
    /// <c>&lt;SceneType&gt;FiberInit</c> 初始化逻辑（不同 Scene 类型创建不同的起始 Entity 树）。<see cref="Fiber"/> 字段指向待初始化的纤程。
    /// </summary>
    public struct FiberInit
    {
        /// <summary>待初始化的纤程实例。</summary>
        public Fiber Fiber { get; set; }
    }
}