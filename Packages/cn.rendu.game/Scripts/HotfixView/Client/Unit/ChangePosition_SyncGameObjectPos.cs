using UnityEngine;
using RD.Core;

namespace RD.Client
{
    [Event(SceneType.Current)]
    public class ChangePosition_SyncGameObjectPos: AEvent<Scene, ChangePosition>
    {
        protected override async Promise Run(Scene scene, ChangePosition args)
        {
            Unit unit = args.Unit;
            GameObjectComponent gameObjectComponent = unit.GetComponent<GameObjectComponent>();
            if (gameObjectComponent == null)
            {
                return;
            }

            Transform transform = gameObjectComponent.Transform;
            transform.position = unit.Position;
            await Promise.CompletedTask;
        }
    }
}