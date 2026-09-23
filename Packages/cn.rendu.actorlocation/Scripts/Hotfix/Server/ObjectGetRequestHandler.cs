using System;
using RD.Core;

namespace RD.Server
{
    [MessageHandler(SceneType.Location)]
    public class ObjectGetRequestHandler: MessageHandler<Scene, ObjectGetRequest, ObjectGetResponse>
    {
        protected override async Promise Run(Scene scene, ObjectGetRequest request, ObjectGetResponse response)
        {
            response.ActorId = await scene.GetComponent<LocationManagerComoponent>().Get(request.Type).Get(request.Key);
        }
    }
}