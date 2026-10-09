using RD.Core;
namespace RD.Client
{
    // 分发UI打开之前监听
    /// <summary>
    /// 面板打开「前」事件分发：先经 UIEventComponent 派发给绑定监听者，再走 DynamicEvent
    /// 用 EntityRef 持有 scene，避免 await 期间场景被销毁后访问到失效对象
    /// </summary>
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
    /// <summary>
    /// 面板打开「后」事件分发：先经 UIEventComponent 派发给绑定监听者，再走 DynamicEvent
    /// </summary>
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