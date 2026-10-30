using System;

namespace RD.Serialization
{
    /// <summary>
    /// 标记：该成员交由序列化器处理 —— 在检视器中显示, 并写入 SerializedScriptableObject 的 JSON 数据。
    /// 可标字段 / 属性 / 类（Inherited）。
    ///
    /// 本特性属于**序列化器**（cn.rendu.serialization, 位于绘制引擎之下）而非检视器框架：
    /// 它描述的是"数据要落盘", 脱离任何 UI 也成立; 检视器只是顺带认它。
    /// 显示规则见绘制引擎的成员可见性判定, JSON 成员规则见本包的 Serialization, 两边同源。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class SerializeAttribute : Attribute
    {
    }
}
