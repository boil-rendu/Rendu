using RD.Core;
namespace RD.Client
{
    // 分发UI关闭之前监听
    /// <summary>
    /// 面板关闭「前」事件分发：先经 UIEventComponent 派发给绑定监听者，再走 DynamicEvent
    /// 用 EntityRef 持有 scene，避免 await 期间场景被销毁后访问到失效对象
    /// </summary>
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
    /// <summary>
    /// 面板关闭「后」事件分发：先经 UIEventComponent 派发给绑定监听者，再走 DynamicEvent
    /// </summary>
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
    /// <summary>
    /// 面板被摧毁事件分发：额外先判 UILoad 是否存在，其余同上
    /// </summary>
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