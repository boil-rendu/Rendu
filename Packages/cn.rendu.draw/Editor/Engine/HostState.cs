namespace RD.Draw.Editor
{
    /// <summary>
    /// 宿主状态的不透明句柄: 折叠状态 / 分组展开 / 表格滚动都存在里面, 实现细节不外泄。
    ///
    /// 每个宿主(窗口 / 检视器)各持一份, 由 <see cref="Engine.CreateHostState"/> 创建,
    /// 绘制时经 <see cref="Engine.DrawObject(object, UnityEditor.EditorWindow, System.Action{object}, System.Action{object}, HostState)"/>
    /// 传回引擎 —— 这样同一进程里多个宿主的状态互不串台, 而宿主所在的程序集
    /// (例如独立的窗口包)不需要看到内部会话类型。
    /// </summary>
    public sealed class HostState
    {
        /// <summary>句柄包裹的会话, 仅本程序集可见。</summary>
        internal HostSession Session { get; }

        internal HostState(HostSession session)
        {
            Session = session;
        }
    }
}
