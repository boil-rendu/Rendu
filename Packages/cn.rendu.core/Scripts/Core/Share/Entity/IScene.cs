namespace RD.Core
{
    /// <summary>
    /// Scene 容器抽象：一个 Scene 绑定一个 <see cref="Fiber"/>（逻辑线程），并以 <see cref="SceneType"/> 标识其类型（用于多 Scene 区分）。
    /// Scene 是 Entity 的容器，所有 Entity 通过 <see cref="Fiber.EntitySystem"/> 完成注册与派发。
    /// </summary>
    public interface IScene
    {
        /// <summary>所属 Fiber（逻辑线程），用于 ECS 派发隔离。</summary>
        Fiber Fiber { get; set; }

        /// <summary>Scene 类型枚举值（业务自定义，用于多 Scene 区分）。</summary>
        int SceneType { get; set; }
    }
}
