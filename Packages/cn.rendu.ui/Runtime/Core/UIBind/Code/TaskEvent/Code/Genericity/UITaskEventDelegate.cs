using RD;
using RD.Core;

namespace RD.Client
{
    /// <summary>UI 异步事件回调委托（0 参），返回 Promise 以便调用方等待全部完成</summary>
    public delegate Promise UITaskEventDelegate();

    /// <summary>UI 异步事件回调委托（1 参）</summary>
    public delegate Promise UITaskEventDelegate<in P1>(P1 p1);

    /// <summary>UI 异步事件回调委托（2 参）</summary>
    public delegate Promise UITaskEventDelegate<in P1, in P2>(P1 p1, P2 p2);

    /// <summary>UI 异步事件回调委托（3 参）</summary>
    public delegate Promise UITaskEventDelegate<in P1, in P2, in P3>(P1 p1, P2 p2, P3 p3);

    /// <summary>UI 异步事件回调委托（4 参）</summary>
    public delegate Promise UITaskEventDelegate<in P1, in P2, in P3, in P4>(P1 p1, P2 p2, P3 p3, P4 p4);

    /// <summary>UI 异步事件回调委托（5 参）</summary>
    public delegate Promise UITaskEventDelegate<in P1, in P2, in P3, in P4, in P5>(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
}
