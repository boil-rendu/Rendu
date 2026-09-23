using RD.Core;

namespace RD.Server
{
    public static class GateMapFactory
    {
        public static async Promise<Scene> Create(Entity parent, long id, long instanceId, string name)
        {
            await Promise.CompletedTask;
            Scene scene = EntitySceneFactory.CreateScene(parent, id, instanceId, SceneType.Map, name);
            scene.AddComponent<UnitComponent>();
            scene.AddComponent<AOIManagerComponent>();
            scene.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            
            return scene;
        }
        
    }
}