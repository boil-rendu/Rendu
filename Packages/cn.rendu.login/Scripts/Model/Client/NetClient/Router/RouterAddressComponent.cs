using System.Net.Sockets;
using RD.Core;

namespace RD.Client
{
    [ComponentOf(typeof(Scene))]
    public class RouterAddressComponent: Entity, IAwake<string>
    {
        public AddressFamily AddressFamily { get; set; }
        public string Address;
        public HttpGetRouterResponse Info;
        public int RouterIndex;
    }
}