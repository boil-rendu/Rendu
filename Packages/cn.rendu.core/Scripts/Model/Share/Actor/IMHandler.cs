using System;
using RD.Core;

namespace RD
{
    public interface IMHandler
    {
        Promise Handle(Entity entity, Address fromAddress, MessageObject actorMessage);
        Type GetRequestType();
        Type GetResponseType();
    }
}