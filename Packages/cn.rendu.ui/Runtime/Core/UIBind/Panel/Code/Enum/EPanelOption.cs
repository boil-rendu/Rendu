using System;
using RD.Draw;
using RD.Core;

namespace RD.Client
{
    //不要修改值 否则已存在的界面会错误
    //只能新增 不允许修改
    /// <summary>
    /// 界面选项（[Flags] 位标志）
    /// 描述一个界面的特殊行为，按位组合；注意值不可修改、只能新增（改值会让已存在的界面错乱）
    /// </summary>
    [LabelText("界面选项")]
    [Flags]
    public enum EPanelOption
    {
        /// <summary>无</summary>
        [LabelText("无")]
        None = 0,

        /// <summary>容器类界面（如伤害飘字；这类界面放到 Panel 层会被特殊处理，建议不要放 Panel 层）</summary>
        [LabelText("容器类界面")]
        Container = 1, //比如伤害飘字 此类界面如果做到panel层会被特殊处理 建议还是不要放到panel层

        /// <summary>永久缓存界面（永不销毁；不同于禁止关闭——它可关闭，只是不销毁，相当于无限长倒计时）</summary>
        [LabelText("永久缓存界面")]
        ForeverCache = 1 << 1, //永远不会被摧毁 与禁止关闭不同这个可以关闭 只是不销毁 也可相当于无限长的倒计时

        /// <summary>倒计时缓存界面（关闭后 X 秒再销毁，否则立即销毁）</summary>
        [LabelText("倒计时缓存界面")]
        TimeCache = 1 << 2, //被关闭后X秒之后在摧毁 否则 立即摧毁

        /// <summary>禁止关闭的界面（需一直存在，可隐藏但不可销毁）</summary>
        [LabelText("禁止关闭的界面")]
        DisClose = 1 << 3, //是需要一直存在的界面 你可以隐藏 但是你不能摧毁

        /// <summary>忽略返回：返回操作会跳过这个界面（打开/关闭不参与返回堆栈）</summary>
        [LabelText("忽略返回 返回操作会跳过这个界面")]
        IgnoreBack = 1 << 4, //他的打开与关闭不会触发返回功能 堆栈功能

        /// <summary>可忽略非指向性的关闭（如 GM 面板/加载 UI；明确指定关闭它仍可关闭）</summary>
        [LabelText("可忽略非指向性的关闭")]
        IgnoreClose = 1 << 5, //可忽略非指向性的关闭 比如 GM面板 比如加载UI
        //(比如你 明确调用关闭GM面板 那是可以关闭的  但是你调用关闭Top层 正好GM也在Top层 那么这个时候GM是不能被关闭的)
    }

    /// <summary>
    /// EPanelOption 位操作的扩展方法
    /// </summary>
    public static class PanelOptionExt
    {
        /// <summary>置位：把 option 加到 owner 上</summary>
        public static void Set(ref this EPanelOption owner, EPanelOption option)
        {
            owner |= option;
        }

        /// <summary>清位：把 option 从 owner 上移除</summary>
        public static void Unset(ref this EPanelOption owner, EPanelOption option)
        {
            owner &= (~option);
        }
    }
}
