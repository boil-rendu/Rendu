using System.Collections.Generic;
using RD.Core;

namespace RD.Server
{
    [ComponentOf(typeof(Scene))]
    public class MessageSender: Entity, IAwake, IDestroy
    {
        public const long TIMEOUT_TIME = 40 * 1000;

        public int RpcId;

        public readonly Dictionary<int, MessageSenderStruct> requestCallback = new();
    }
}