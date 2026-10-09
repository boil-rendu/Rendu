using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>UIPanelComponent 的调试开关查询（均为 EPanelOption 标志位的语义化封装）</summary>
    public partial class UIPanelComponent
    {
        //容器类界面 //比如伤害飘字 此类界面如果做到panel层会被特殊处理 建议还是不要放到panel层
        /// <summary>容器类界面（如伤害飘字），放到 panel 层会被特殊处理，一般不建议</summary>
        public bool PanelContainer => PanelOption.HasFlag(EPanelOption.Container);

        //永久缓存界面 //永远不会被摧毁 与禁止关闭不同这个可以关闭 只是不销毁 也可相当于无限长的倒计时
        /// <summary>永久缓存界面：可关闭但永不销毁</summary>
        public bool PanelForeverCache => PanelOption.HasFlag(EPanelOption.ForeverCache);

        //倒计时缓存界面 //被关闭后X秒之后在摧毁 否则立即摧毁
        /// <summary>倒计时缓存界面：关闭后延迟一段时间再销毁</summary>
        public bool PanelTimeCache => PanelOption.HasFlag(EPanelOption.TimeCache);

        //禁止关闭的界面 //是需要一直存在的你可以隐藏 但是你不能摧毁
        /// <summary>禁止关闭的界面：可隐藏但不可摧毁</summary>
        public bool PanelDisClose => PanelOption.HasFlag(EPanelOption.DisClose);

        //忽略返回 返回操作会跳过这个界面 //他的打开与关闭不会触发返回功能 堆栈功能
        /// <summary>忽略返回：返回操作会跳过该界面，其开闭不参与堆栈</summary>
        public bool PanelIgnoreBack => PanelOption.HasFlag(EPanelOption.IgnoreBack);

        //可忽略非指向性的关闭
        /// <summary>可忽略非指向性（批量/间接）的关闭</summary>
        public bool PanelIgnoreClose => PanelOption.HasFlag(EPanelOption.IgnoreClose);
    }
}
