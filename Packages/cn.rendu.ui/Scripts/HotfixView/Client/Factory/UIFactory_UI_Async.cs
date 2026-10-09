//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System;
using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UIFactory 工厂（UI 异步部分）
    /// 一组重载：按 类型 / Vo / pkg+res / 仅 resName 定位绑定数据后异步创建 UI
    /// </summary>
    public static partial class UIFactory
    {
        /// <summary>异步实例化 UI（按泛型类型查绑定 Vo 后创建）</summary>
        public static async Promise<T> InstantiateAsync<T>(Scene scene, Entity parentEntity, Transform parent = null) where T : Entity
        {
            var data = parentEntity.UIBind().GetBindVoByType<T>();
            if (data == null) return null;
            var vo = data.Value;

            return await InstantiateAsync<T>(scene, vo, parentEntity, parent);
        }

        /// <summary>异步实例化 UI（按泛型类型 + Vo）；父节点为 null 时挂到 UIMgr.UICache 下</summary>
        public static async Promise<T> InstantiateAsync<T>(Scene scene, UIBindVo vo, Entity parentEntity, Transform parent = null) where T : Entity
        {
            EntityRef<Scene> sceneRef = scene;
            var uiCom = await CreateAsync(scene, vo, parentEntity);
            scene = sceneRef;
            SetParent(uiCom.GetParent<UIChild>().OwnerRectTransform, parent ? parent : scene.UIMgr().UICache);
            return (T)uiCom;
        }

        /// <summary>异步实例化 UI（按 Vo，返回基类 Entity）</summary>
        public static async Promise<Entity> InstantiateAsync(Scene scene, UIBindVo vo, Entity parentEntity, Transform parent = null)
        {
            EntityRef<Scene> sceneRef = scene;
            var uiCom = await CreateAsync(scene, vo, parentEntity);
            scene = sceneRef;
            SetParent(uiCom.GetParent<UIChild>().OwnerRectTransform, parent ? parent : scene.UIMgr().UICache);
            return uiCom;
        }

        /// <summary>异步实例化 UI（按 UI 组件 Type 查绑定 Vo）</summary>
        public static async Promise<Entity> InstantiateAsync(Scene scene, Type uiType, Entity parentEntity, Transform parent = null)
        {
            var data = parentEntity.UIBind().GetBindVoByType(uiType);
            if (data == null) return null;
            var vo = data.Value;

            return await InstantiateAsync(scene, vo, parentEntity, parent);
        }

        /// <summary>异步实例化 UI（按 pkgName + resName 路径查绑定 Vo）</summary>
        public static async Promise<Entity> InstantiateAsync(Scene scene,
                                                            string pkgName,
                                                            string resName,
                                                            Entity parentEntity,
                                                            Transform parent = null)
        {
            var data = parentEntity.UIBind().GetBindVoByPath(pkgName, resName);
            if (data == null) return null;
            var vo = data.Value;

            return await InstantiateAsync(scene, vo, parentEntity, parent);
        }

        /// <summary>异步实例化 UI（仅按 resName 查绑定 Vo）</summary>
        public static async Promise<Entity> InstantiateAsync(Scene scene,
                                                            string resName,
                                                            Entity parentEntity,
                                                            Transform parent = null)
        {
            var data = parentEntity.UIBind().GetBindVoByResName(resName);
            if (data == null) return null;
            var vo = data.Value;

            return await InstantiateAsync(scene, vo, parentEntity, parent);
        }

        /// <summary>
        /// 异步创建面板实体
        /// 若 panelInfo 里带有预加载好的 GameObject 则直接复用（用后置空），否则走正常异步加载
        /// </summary>
        public static async Promise<Entity> CreatePanelAsync(Scene scene, PanelInfo panelInfo, Entity parentEntity)
        {
            var bingVo = parentEntity.UIBind().GetBindVoByPath(panelInfo.PkgName, panelInfo.ResName);
            if (bingVo == null) return null;
            Entity panelEntity = null;
            if (panelInfo.PreLoadGameObject == null)
            {
                panelEntity = await CreateAsync(scene, bingVo.Value, parentEntity);
            }
            else
            {
                panelEntity = CreateByObjVo(bingVo.Value, panelInfo.PreLoadGameObject, parentEntity);
                panelInfo.ResetPreLoadGameObject(null);
            }

            return panelEntity;
        }

        /// <summary>异步加载预制体并创建 UI 实体（创建流程的公共入口）</summary>
        public static async Promise<Entity> CreateAsync(Scene scene, UIBindVo vo, Entity parentEntity)
        {
            EntityRef<Entity> parentEntityRef = parentEntity;
            var obj = await scene.UILoad()?.LoadAssetAsyncInstantiate(vo.PkgName, vo.ResName);
            if (obj == null)
            {
                Debug.LogError($"没有加载到这个资源 {vo.PkgName}/{vo.ResName}");
                return null;
            }

            return CreateByObjVo(vo, obj, parentEntityRef);
        }

        /// <summary>异步创建面板的预制体（只加载不建 Entity）</summary>
        internal static async Promise<GameObject> CreatePanelGameObjectAsync(Scene scene, PanelInfo panelInfo)
        {
            var bingVo = scene.UIBind().GetBindVoByPath(panelInfo.PkgName, panelInfo.ResName);
            if (bingVo == null) return null;
            return await CreateGameObjectAsync(scene, bingVo.Value);
        }

        /// <summary>异步加载 Vo 对应的预制体（只加载不建 Entity，用于预加载）</summary>
        internal static async Promise<GameObject> CreateGameObjectAsync(Scene scene, UIBindVo vo)
        {
            var obj = await scene.UILoad()?.LoadAssetAsyncInstantiate(vo.PkgName, vo.ResName);
            if (obj == null)
            {
                Debug.LogError($"没有加载到这个资源 {vo.PkgName}/{vo.ResName}");
                return null;
            }

            return obj;
        }
    }
}