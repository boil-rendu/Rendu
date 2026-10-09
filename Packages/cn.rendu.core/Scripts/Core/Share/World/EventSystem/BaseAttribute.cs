using System;

namespace RD.Core
{
    /// <summary>
    /// 框架特性基类：所有「会被 <see cref="CodeTypes"/> 反射收集、且 <c>EnableClass</c> 源生成器需要处理」的特性都派生自它
    ///（<see cref="EventAttribute"/>、<see cref="EntitySystemAttribute"/>、<see cref="CodeProcessAttribute"/> 等）。
    /// 派生特性需在类上标注 <c>[AttributeUsage(AttributeTargets.Class)]</c>，框架通过 <see cref="CodeTypes.Awake"/> 反射时统一 <c>GetCustomAttributes(typeof(BaseAttribute), true)</c> 收口。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [EnableClass]
    public class BaseAttribute: EnableClassAttribute
    {
    }
}
