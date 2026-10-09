namespace RD.Core
{
    /// <summary>
    /// Handler 类的根类型：标记为「不持有 Entity 生命周期」的对象，统一从 <see cref="Object"/> 继承供源生成器识别。
    /// 派生：<see cref="AInvokeHandler{A}"/>、<see cref="AInvokeHandler{A,T}"/> 等 Invoke 处理类（不在 Entity 池里）。
    /// </summary>
    public abstract class HandlerObject: Object
    {

    }
}
