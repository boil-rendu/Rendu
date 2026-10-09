namespace RD.Core
{
    /// <summary>
    /// System 类（按 <c>systemType</c> 注册的处理器）根类型：与 <see cref="HandlerObject"/> 平级，从 <see cref="Object"/> 继承。
    /// 派生示例：UI 模块的 <c>UIOpenSystem&lt;T&gt;</c>、<c>UICloseSystem&lt;T&gt;</c>、<c>UIInitializeSystem&lt;T&gt;</c> 等 <c>IUI*System</c> 处理器基类（这些不是 Entity，挂在 <see cref="TypeSystems"/> 的 systemType 索引上）。
    /// </summary>
    public abstract class SystemObject: Object
    {

    }
}
