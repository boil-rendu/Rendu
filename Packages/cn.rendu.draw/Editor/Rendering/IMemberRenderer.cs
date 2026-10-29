namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 成员渲染链上的一档: 声明"什么条件下这个成员该由我画"。
    ///
    /// 存在的意义是把"谁来画这个成员"从渲染主流程里搬出来。
    /// 也就是**加特性必须改核心流程**。现在加特性 = 新增一个实现 + 链上登记一档。
    ///
    /// 与 <see cref="IValueControl"/> 的分工: 那条链回答"值用哪个控件",
    /// 这条链回答"这个成员整体由谁画"。两层互不干扰。
    /// </summary>
    internal interface IMemberRenderer
    {
        /// <summary>这一档是否接管该成员(链上第一个返回 true 的获胜)。</summary>
        bool CanDraw(in MemberDrawArgs args);

        /// <summary>画出这个成员。</summary>
        void Draw(in MemberDrawArgs args);
    }
}
