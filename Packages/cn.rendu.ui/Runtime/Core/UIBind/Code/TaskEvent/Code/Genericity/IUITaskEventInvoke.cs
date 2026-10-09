using RD;
using RD.Core;

namespace RD.Client
{
    /// <summary>UI 异步事件调用契约（0 参），返回 Promise 以便统一等待</summary>
    public interface IUITaskEventInvoke
    {
        Promise Invoke();
    }

    /// <summary>UI 异步事件调用契约（1 参）</summary>
    public interface IUITaskEventInvoke<P1>
    {
        Promise Invoke(P1 p1);
    }

    /// <summary>UI 异步事件调用契约（2 参）</summary>
    public interface IUITaskEventInvoke<P1, P2>
    {
        Promise Invoke(P1 p1, P2 p2);
    }

    /// <summary>UI 异步事件调用契约（3 参）</summary>
    public interface IUITaskEventInvoke<P1, P2, P3>
    {
        Promise Invoke(P1 p1, P2 p2, P3 p3);
    }

    /// <summary>UI 异步事件调用契约（4 参）</summary>
    public interface IUITaskEventInvoke<P1, P2, P3, P4>
    {
        Promise Invoke(P1 p1, P2 p2, P3 p3, P4 p4);
    }

    /// <summary>UI 异步事件调用契约（5 参）</summary>
    public interface IUITaskEventInvoke<P1, P2, P3, P4, P5>
    {
        Promise Invoke(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
    }
}
