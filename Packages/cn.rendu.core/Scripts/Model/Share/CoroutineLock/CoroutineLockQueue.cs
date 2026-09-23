using System.Collections.Generic;
using RD.Core;

namespace RD
{
    [ChildOf(typeof(CoroutineLockQueueType))]
    public class CoroutineLockQueue: Entity, IAwake<long>, IDestroy
    {
        public long type;

        public bool isStart;
        
        public Queue<WaitCoroutineLock> queue = new();

        public int Count
        {
            get
            {
                return this.queue.Count;
            }
        }
    }
}