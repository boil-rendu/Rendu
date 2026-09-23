using RD.Core;

namespace RD
{
    [ComponentOf(typeof(Session))]
    public class SessionIdleCheckerComponent: Entity, IAwake, IDestroy
    {
        public long RepeatedTimer;
    }
}