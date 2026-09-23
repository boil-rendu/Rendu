using System;
using RD.Core;
using UnityEngine.SceneManagement;
using Scene = RD.Core.Scene;

namespace RD.Client
{
    [Event(SceneType.StateSync)]
    public class SceneChangeStart_AddComponent: AEvent<Scene, SceneChangeStart>
    {
        protected override async Promise Run(Scene root, SceneChangeStart args)
        {
            try
            {
                Scene currentScene = root.CurrentScene();

                ResourcesLoaderComponent resourcesLoaderComponent = currentScene.GetComponent<ResourcesLoaderComponent>();
            
                // 加载场景资源，并实时更新 Loading 进度
                await resourcesLoaderComponent.LoadSceneAsync($"Packages/cn.rendu.gameres/Scenes/{currentScene.Name}.unity", LoadSceneMode.Single, progress =>
                {
                    UIMgrComponent uiComponent = root.GetComponent<UIMgrComponent>();
                    // TODO(RD): 新 UI 框架无 UILoadingComponent 对应实现, 需确认加载进度接口(如 UIMgr 打开 Loading 面板后更新进度)
#if RD_UI_LEGACY_LOADING_PROGRESS
                    UILoadingComponent loading = uiComponent?.Get(typeof(UILoadingComponent))?.GetComponent<UILoadingComponent>();
                    loading?.SetProgress(progress);
#endif
                });
                // 切换到map场景

                //await SceneManager.LoadSceneAsync(currentScene.Name);

                currentScene.AddComponent<OperaComponent>();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

        }
    }
}