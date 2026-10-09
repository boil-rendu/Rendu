using RD.Core;
namespace RD.Client
{
    /// <summary>UI 事件调用契约（0 参）</summary>
    public interface IUIEventInvoke
    {
        void Invoke();
    }

    /// <summary>UI 事件调用契约（1 参）</summary>
    public interface IUIEventInvoke<P1>
    {
        void Invoke(P1 p1);
    }

    /// <summary>UI 事件调用契约（2 参）</summary>
    public interface IUIEventInvoke<P1, P2>
    {
        void Invoke(P1 p1, P2 p2);
    }

    /// <summary>UI 事件调用契约（3 参）</summary>
    public interface IUIEventInvoke<P1, P2, P3>
    {
        void Invoke(P1 p1, P2 p2, P3 p3);
    }

    /// <summary>UI 事件调用契约（4 参）</summary>
    public interface IUIEventInvoke<P1, P2, P3, P4>
    {
        void Invoke(P1 p1, P2 p2, P3 p3, P4 p4);
    }

    /// <summary>UI 事件调用契约（5 参）</summary>
    public interface IUIEventInvoke<P1, P2, P3, P4, P5>
    {
        void Invoke(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
    }
}
