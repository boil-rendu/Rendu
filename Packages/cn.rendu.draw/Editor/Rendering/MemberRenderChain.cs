using System.Collections.Generic;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 成员渲染链: 顺序即优先级, 第一个 <see cref="IMemberRenderer.CanDraw"/> 为真的获胜。
    ///
    /// 它是"这个成员由谁画"这条知识的唯一归属地。加一种新的画法 = 新增一个实现 + 在这里登记一档,
    /// 渲染主流程不动。
    ///
    /// 登记的是**对象**而不是裸委托: 每一档自带它的画法与它的适用条件,
    /// 读这一段就知道全部优先级, 也不必在一堆签名不同的 Action 之间猜参数顺序。
    /// </summary>
    internal sealed class MemberRenderChain : DrawChain<IMemberRenderer, MemberDrawArgs, Nothing>
    {
        public MemberRenderChain(IEnumerable<IMemberRenderer> renderers) : base(renderers)
        {
        }

        /// <inheritdoc />
        protected override bool CanHandle(IMemberRenderer stage, in MemberDrawArgs args)
        {
            return stage.CanDraw(args);
        }

        /// <inheritdoc />
        protected override Nothing Handle(IMemberRenderer stage, in MemberDrawArgs args)
        {
            stage.Draw(args);
            return Nothing.Instance;
        }

        /// <summary>
        /// 全链都不接管时的兜底: **记一条警告并什么都不画**。
        ///
        /// 链上垫底的 <c>EditableValueRenderer</c> 对任何成员都返回 true, 所以正常走不到这里。
        /// 真走到这里只有一种可能 —— 那一档被摘掉了, 或者新加的档把 CatchAll 之后的都遮住了。
        /// 此时成员会凭空消失, 而用户看不出原因; 报一条日志至少让问题可定位。
        /// </summary>
        protected override Nothing Fallback(in MemberDrawArgs args)
        {
            DrawLog.WarnOnce("MemberRenderChain.Fallback",
                $"[Inspector] 成员渲染链没有任何一档接管 '{args.Value.Member?.RawName}', 该成员被跳过。" +
                "请检查 InspectorRenderer.BuildMemberChain 是否仍以可编辑值那一档垫底。");
            return Nothing.Instance;
        }
    }
}
