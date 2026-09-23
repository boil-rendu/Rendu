using RD.Core;

namespace RD
{
    public struct NetComponentOnRead
    {
        public Session Session;
        public object Message;
    }
    
    [ComponentOf(typeof(Scene))]
    public class NetComponent: Entity, IAwake<IKcpTransport>, IDestroy, IUpdate
    {
        public AService AService { get; set; }
    }
}