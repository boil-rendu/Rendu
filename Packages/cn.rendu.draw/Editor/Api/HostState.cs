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

        /// <summary>
        /// 告知宿主当前的视口宽(滚动区视口的宽, 与内容宽无关)。
        ///
        /// 引擎里的表格按它均分列宽, 不用"根节点量到的宽": 后者会被内容自己撑宽 ——
        /// 定宽的表头/单元格抬高面板的最小宽 → 面板内容宽跟着涨 → 根节点量到的宽再涨 → 列宽再涨,
        /// 形成"越撑越宽"的死循环。宿主在 EndScrollView 之后量一次滚动区矩形报上来即可
        /// (量到的值与内容无关, 所以不会自己喂自己)。
        /// </summary>
        public void SetViewportWidth(float width)
        {
            Session.SetViewportWidth(width);
        }

        /// <summary>
        /// 告知宿主内容区的可用下沿(窗口坐标, 已按宿主自己的滚动位置折算): 表格的数据区
        /// 按它填满外框剩下的高度, 底边贴着表格盒下沿, 窗口拉高时跟着拉长。
        ///
        /// 传 0 = 不给(检视器这类宿主量不到, 表格退回"按行数估高度")。
        /// 滚动位置要折算掉: 否则一滚滚轮, "还剩多少高"就变, 表格会跟着长/缩。
        /// </summary>
        public void SetContentBottom(float bottom)
        {
            Session.SetContentBottom(bottom);
        }
    }
}
