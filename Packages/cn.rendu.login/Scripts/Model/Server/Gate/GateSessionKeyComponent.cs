using System.Collections.Generic;
using RD.Core;

namespace RD.Server
{
    [ComponentOf(typeof(Scene))]
    public class GateSessionKeyComponent : Entity, IAwake
    {
        public readonly Dictionary<long, string> sessionKey = new();
    }
}