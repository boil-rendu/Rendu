using RD.Core;

namespace RD
{
    [ChildOf(typeof(CoroutineLockQueue))]
    public class CoroutineLock: Entity, IAwake<long, long, int>, IDestroy
    {
        public long type;
        public long key;
        public int level;
    }
}