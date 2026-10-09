using RD.Core;
namespace RD.Client
{
    //应该不用扩展了 5个参数你都不够用嘛
    //如果有请包起来用参数类sy
    //或使用ParamVo
    /// <summary>UI 事件回调委托（0 参）</summary>
    public delegate void UIEventDelegate();

    /// <summary>UI 事件回调委托（1 参）</summary>
    public delegate void UIEventDelegate<in P1>(P1 p1);

    /// <summary>UI 事件回调委托（2 参）</summary>
    public delegate void UIEventDelegate<in P1, in P2>(P1 p1, P2 p2);

    /// <summary>UI 事件回调委托（3 参）</summary>
    public delegate void UIEventDelegate<in P1, in P2, in P3>(P1 p1, P2 p2, P3 p3);

    /// <summary>UI 事件回调委托（4 参）</summary>
    public delegate void UIEventDelegate<in P1, in P2, in P3, in P4>(P1 p1, P2 p2, P3 p3, P4 p4);

    /// <summary>UI 事件回调委托（5 参）</summary>
    public delegate void UIEventDelegate<in P1, in P2, in P3, in P4, in P5>(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
}
