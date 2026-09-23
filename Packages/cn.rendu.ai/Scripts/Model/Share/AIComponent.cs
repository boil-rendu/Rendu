using RD.Core;

namespace RD
{
    // 客户端挂在ClientScene上，服务端挂在Unit上
    [ComponentOf(typeof(Scene))]
    public class AIComponent: Entity, IAwake<int>, IDestroy
    {
        public int AIConfigId;
        
        public PromiseCancellationToken CancellationToken;

        public long Timer;

        public int Current;
    }
}