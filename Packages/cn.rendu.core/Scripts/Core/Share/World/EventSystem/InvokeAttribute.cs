using System;

namespace RD.Core
{
    /// <summary>
    /// 标记一个 <see cref="AInvokeHandler{A}"/> 处理类的调用键。可选三种形式：
    /// <list type="number">
    /// <item><c>[Invoke]</c>：无参，调用键默认为 0（业务可改 <c>Type</c>）。</item>
    /// <item><c>[Invoke(long type)]</c>：用 long 直接作为键。</item>
    /// <item><c>[Invoke(string invokeType)]</c>：用字符串经 <see cref="InvokeId.Of"/> 哈希为 long，源生成器常产出这种形式（<c>[Invoke("LoginPanelComponent.OnEventLoginInvoke")]</c>）。</item>
    /// </list>
    /// 字符串键比 long 键更易重构（重命名类/方法时字符串跟着变，源生成器同步更新），<see cref="EventSystem.Awake"/> 会优先用 <c>InvokeType</c>。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class InvokeAttribute : BaseAttribute
    {
        /// <summary>long 形式的调用键（<c>[Invoke(long)]</c> 或 <c>[Invoke()]</c> 默认 0）。</summary>
        public long Type { get; private set; }
        /// <summary>字符串形式的调用键（<c>[Invoke(string)]</c>，经 <see cref="InvokeId.Of"/> 哈希为 long）。</summary>
        public string InvokeType { get; private set; }

        // 使用 long type 参数
        /// <summary>用 long 直接作为调用键。</summary>
        /// <param name="type">调用键。</param>
        public InvokeAttribute(long type = 0)
        {
            this.Type = type;
        }

        // 使用 string invokeType 参数
        /// <summary>用字符串经 <see cref="InvokeId.Of"/> 哈希为 long 作调用键（源生成器产出形式，重命名时字符串随符号一起变）。</summary>
        /// <param name="invokeType">调用键字符串。</param>
        public InvokeAttribute(string invokeType)
        {
            this.InvokeType = invokeType;
        }

        // 无参数，读取方法名
        /// <summary>无参，调用键默认为 0（业务可改 <see cref="Type"/>）。</summary>
        public InvokeAttribute()
        {
        }
    }

}
