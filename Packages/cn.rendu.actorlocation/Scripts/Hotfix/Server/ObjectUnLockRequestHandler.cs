using RD.Core;

namespace RD.Server
{
    [MessageHandler(SceneType.Location)]
    public class ObjectUnLockRequestHandler: MessageHandler<Scene, ObjectUnLockRequest, ObjectUnLockResponse>
    {
        protected override async Promise Run(Scene scene, ObjectUnLockRequest request, ObjectUnLockResponse response)
        {
            scene.GetComponent<LocationManagerComoponent>().Get(request.Type).UnLock(request.Key, request.OldActorId, request.NewActorId);

            await Promise.CompletedTask;
        }
    }
}