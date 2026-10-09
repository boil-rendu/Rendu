namespace RD.Core
{
    /// <summary>
    /// 标记一个 <see cref="ASingleton"/> 派生类在 <see cref="CodeTypes.CodeProcess"/> 阶段被自动 <c>new</c> + <c>Awake()</c> + 注册到 <see cref="World"/>。
    /// 用于把依赖 <see cref="CodeTypes"/> 扫描结果的单例（如 <see cref="EventSystem"/>）串进启动链。
    /// </summary>
    public class CodeProcessAttribute: BaseAttribute
    {

    }
}
