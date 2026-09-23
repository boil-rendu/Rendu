using RD;
using RD.Core;

namespace RD.Client
{
    public interface IUITaskEventInvoke
    {
        Promise Invoke();
    }

    public interface IUITaskEventInvoke<P1>
    {
        Promise Invoke(P1 p1);
    }

    public interface IUITaskEventInvoke<P1, P2>
    {
        Promise Invoke(P1 p1, P2 p2);
    }

    public interface IUITaskEventInvoke<P1, P2, P3>
    {
        Promise Invoke(P1 p1, P2 p2, P3 p3);
    }

    public interface IUITaskEventInvoke<P1, P2, P3, P4>
    {
        Promise Invoke(P1 p1, P2 p2, P3 p3, P4 p4);
    }

    public interface IUITaskEventInvoke<P1, P2, P3, P4, P5>
    {
        Promise Invoke(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
    }
}
