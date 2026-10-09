//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>UIFactory 工厂（Common 部分）：实例化「普通 UI 预制体」（非 Entity 体系）</summary>
    public static partial class UIFactory
    {
        #if !UIMACRO_SYNCLOAD_CLOSE
        //普通的UI预制体 创建与摧毁 一定要成对
        //为了防止忘记 所以默认自动回收
        /// <summary>
        /// 同步实例化一个普通 UI 预制体
        /// 会强制挂上 UIReleaseInstantiate，被摧毁时自动回收资源，无需再调用 UIFactory.Destroy
        /// </summary>
        public static GameObject InstantiateGameObject(Scene scene, string pkgName, string resName)
        {
            var obj = scene.UILoad()?.LoadAssetInstantiate(pkgName, resName);
            if (obj == null)
            {
                Debug.LogError($"没有加载到这个资源 {pkgName}/{resName}");
                return null;
            }

            //强制添加 既然你要使用这个方法那就必须接受 否则请使用其他方式
            //被摧毁时 自动回收 无需调用 UIFactory.Destroy
            obj.AddComponent<UIReleaseInstantiate>().m_EntityRef = scene;

            return obj;
        }
        #endif

        /// <summary>异步实例化一个普通 UI 预制体（同样自动挂 UIReleaseInstantiate 自动回收）</summary>
        public static async Promise<GameObject> InstantiateGameObjectAsync(Scene scene, string pkgName, string resName)
        {
            EntityRef<Scene> sceneRef = scene;
            var obj = await scene.UILoad()?.LoadAssetAsyncInstantiate(pkgName, resName);
            if (obj == null)
            {
                Debug.LogError($"没有加载到这个资源 {pkgName}/{resName}");
                return null;
            }

            obj.AddComponent<UIReleaseInstantiate>().m_EntityRef = sceneRef.Entity;

            return obj;
        }
    }
}