using RD.Core;
namespace RD.Client
{
    // 分发UI打开之前监听
    [Event(SceneType.All)]
    public class UIEventPanelOpenBeforeHandler : AEvent<Scene, UIEventPanelOpenBefore>
    {
        protected override async Promise Run(Scene scene, UIEventPanelOpenBefore arg)
        {
            if (scene == null || scene.IsDisposed)
            {
                return;
            }

            EntityRef<Scene> sceneRef = scene;
            await UIEventComponent.Instance.Run(scene, arg.UIComponentName, arg);
            scene = sceneRef;
            await scene.DynamicEvent(arg);
        }
    }

    // 分发UI打开之后监听
    [Event(SceneType.All)]
    public class UIEventPanelOpenAfterHandler : AEvent<Scene, UIEventPanelOpenAfter>
    {
        protected override async Promise Run(Scene scene, UIEventPanelOpenAfter arg)
        {
            if (scene == null || scene.IsDisposed)
            {
                return;
            }

            EntityRef<Scene> sceneRef = scene;
            await UIEventComponent.Instance.Run(scene, arg.UIComponentName, arg);
            scene = sceneRef;
            await scene.DynamicEvent(arg);
        }
    }
}