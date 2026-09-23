using RD.Core;
namespace RD.Client
{
    // 分发UI关闭之前监听
    [Event(SceneType.All)]
    public class UIEventPanelCloseBeforeHandler : AEvent<Scene, UIEventPanelCloseBefore>
    {
        protected override async Promise Run(Scene scene, UIEventPanelCloseBefore arg)
        {
            if (scene == null || scene.IsDisposed)
            {
                return;
            }

            EntityRef<Scene> sceneRef = scene;
            await UIEventComponent.Instance.Run(scene, arg.UIComponentName, arg);
            scene = sceneRef;
            if (scene is { IsDisposed: false })
            {
                await scene.DynamicEvent(arg);
            }
        }
    }

    // 分发UI关闭之后监听
    [Event(SceneType.All)]
    public class UIEventPanelCloseAfterHandler : AEvent<Scene, UIEventPanelCloseAfter>
    {
        protected override async Promise Run(Scene scene, UIEventPanelCloseAfter arg)
        {
            if (scene == null || scene.IsDisposed)
            {
                return;
            }

            EntityRef<Scene> sceneRef = scene;
            await UIEventComponent.Instance.Run(scene, arg.UIComponentName, arg);
            scene = sceneRef;
            if (scene is { IsDisposed: false })
            {
                await scene.DynamicEvent(arg);
            }
        }
    }

    // 分发UI被摧毁
    [Event(SceneType.All)]
    public class UIEventPanelDestroyHandler : AEvent<Scene, UIEventPanelDestroy>
    {
        protected override async Promise Run(Scene scene, UIEventPanelDestroy arg)
        {
            if (scene == null || scene.IsDisposed)
            {
                return;
            }

            if (scene.UILoad() == null) return;
            EntityRef<Scene> sceneRef = scene;
            await UIEventComponent.Instance.Run(scene, arg.UIComponentName, arg);
            scene = sceneRef;
            if (scene is { IsDisposed: false })
            {
                await scene.DynamicEvent(arg);
            }
        }
    }
}