namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 一个"值的画法": 声明自己能处理哪类类型, 并给出该类型的控件调用。
    /// 存在的意义是把"哪种类型用哪个控件"这条知识从渲染主流程里搬出来。
    /// 加类型 = 新增一个实现类 + 在 <see cref="ValueControlChain"/> 注册一行,
    /// 主流程与已有策略都不动。    ///
    /// 注意 <see cref="CanHandle"/> 必须只看**声明类型与特性**, 不要看运行时值:
    /// 值可能在编辑过程中变成 null / 变成另一个类型, 用值来判断会导致控件在编辑中途跳变。
    /// </summary>
    internal interface IValueControl
    {
        /// <summary>这个画法是否接管该值(第一个返回 true 的策略获胜)。</summary>
        bool CanHandle(in ValueDrawArgs args);

        /// <summary>产出该值的叶子控件节点(新值由渲染器回填, 写回由 ChangeApplier 统一做)。</summary>
        void Build(in ValueDrawArgs args);
    }
}