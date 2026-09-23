using RD.Core;

namespace RD.Client
{
    [ChildOf(typeof(NetComponent))]
    public class RouterConnector: Entity, IAwake, IDestroy
    {
        public byte Flag { get; set; }
    }
}