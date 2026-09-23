using System.Collections.Generic;
using RD.Core;

namespace RD.Server
{
    [MessageHandler(SceneType.NetInner)]
    public class A2NetInner_MessageHandler: MessageHandler<Scene, A2NetInner_Message>
    {
        protected override async Promise Run(Scene root, A2NetInner_Message innerMessage)
        {
            root.GetComponent<ProcessOuterSender>().Send(innerMessage.ActorId, innerMessage.MessageObject);
            await Promise.CompletedTask;
        }
    }
}