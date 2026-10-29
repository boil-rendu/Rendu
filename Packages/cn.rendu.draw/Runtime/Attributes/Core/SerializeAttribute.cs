using System;

namespace RD.Draw
{
    /// <summary>
    /// 标记：该成员交由序列化器处理 —— 在检视器中显示, 并写入 SerializedScriptableObject 的 JSON 数据。
    /// 可标字段 / 属性 / 类（Inherited）。
    /// 显示规则见 Engine 可见性判定, JSON 成员规则见 RD.Serialization.Editor.Serialization, 两边同源。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class SerializeAttribute : Attribute
    {
    }
}
