#if !UIMACRO_SYNCLOAD_CLOSE
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
    /// UIFactory 工厂（UI 同步部分，宏 UIMACRO_SYNCLOAD_CLOSE 打开时整文件不参与编译）
    /// 与异步版一一对应，内部走同步 LoadAssetInstantiate
    /// </summary>
    public static partial class UIFactory
    {
        /// <summary>按 pkgName + resName 同步创建 UI Entity</summary>
        internal static Entity CreateCommon(string pkgName, string resName, GameObject obj, Entity parentEntity)
        {
            var bingVo = parentEntity.UIBind().GetBindVoByPath(pkgName, resName);
            if (bingVo == null) return null;
            var vo = bingVo.Value;
            return CreateByObjVo(vo, obj, parentEntity);
        }

        /// <summary>同步创建面板实体（同步版 CreatePanelAsync）</summary>
        internal static Entity CreatePanel(Scene scene, PanelInfo panelInfo, Entity parentEntity)
        {
            return Create(scene, panelInfo.PkgName, panelInfo.ResName, parentEntity);
        }

        /// <summary>同步创建 UI Entity（按泛型类型查绑定 Vo）</summary>
        private static T Create<T>(Scene scene, Entity parentEntity) where T : Entity
        {
            var data = parentEntity.UIBind().GetBindVoByType<T>();
            if (data == null) return null;
            var vo = data.Value;

            return (T)Create(scene, vo, parentEntity);
        }

        /// <summary>同步创建 UI Entity（按 pkgName + resName 查绑定 Vo）</summary>
        private static Entity Create(Scene scene, string pkgName, string resName, Entity parentEntity)
        {
            var bingVo = parentEntity.UIBind().GetBindVoByPath(pkgName, resName);
            return bingVo == null ? null : Create(scene, bingVo.Value, parentEntity);
        }

        /// <summary>同步创建 UI Entity（核心：加载预制体 + CreateByObjVo）</summary>
        private static Entity Create(Scene scene, UIBindVo vo, Entity parentEntity)
        {
            var obj = scene.UILoad()?.LoadAssetInstantiate(vo.PkgName, vo.ResName);
            if (obj == null)
            {
                Debug.LogError($"没有加载到这个资源 {vo.PkgName}/{vo.ResName}");
                return null;
            }

            return CreateByObjVo(vo, obj, parentEntity);
        }

        /// <summary>同步实例化 UI（按泛型类型查绑定 Vo，走场景的 UIBind）</summary>
        public static T Instantiate<T>(Scene scene, Entity parentEntity, Transform parent = null) where T : Entity
        {
            var data = scene.UIBind().GetBindVoByType<T>();
            if (data == null) return null;
            var vo = data.Value;

            return Instantiate<T>(scene, vo, parentEntity, parent);
        }

        /// <summary>同步实例化 UI（按泛型类型 + Vo）；父节点为 null 时挂到 UIMgr.UICache 下</summary>
        public static T Instantiate<T>(Scene scene, UIBindVo vo, Entity parentEntity, Transform parent = null) where T : Entity
        {
            var instance = (T)Create(scene, vo, parentEntity);
            if (instance == null) return null;

            SetParent(instance.GetParent<UIChild>().OwnerRectTransform, parent ? parent : scene.UIMgr().UICache);

            return instance;
        }

        /// <summary>同步实例化 UI（按 UI 组件 Type 查绑定 Vo）</summary>
        public static Entity Instantiate(Scene scene, Type uiType, Entity parentEntity, Transform parent = null)
        {
            var data = scene.UIBind().GetBindVoByType(uiType);
            if (data == null) return null;
            var vo = data.Value;

            return Instantiate(scene, vo, parentEntity, parent);
        }

        /// <summary>同步实例化 UI（按 Vo）</summary>
        public static Entity Instantiate(Scene scene, UIBindVo vo, Entity parentEntity, Transform parent = null)
        {
            var instance = Create(scene, vo, parentEntity);
            if (instance == null) return null;

            SetParent(instance.GetParent<UIChild>().OwnerRectTransform, parent ? parent : scene.UIMgr().UICache);

            return instance;
        }

        /// <summary>同步实例化 UI（按 pkgName + resName 查绑定 Vo）</summary>
        public static Entity Instantiate(Scene scene, string pkgName, string resName, Entity parentEntity, Transform parent = null)
        {
            var data = scene.UIBind().GetBindVoByPath(pkgName, resName);
            if (data == null) return null;
            var vo = data.Value;

            return Instantiate(scene, vo, parentEntity, parent);
        }

        /// <summary>同步实例化 UI（仅按 resName 查绑定 Vo）</summary>
        public static Entity Instantiate(Scene scene, string resName, Entity parentEntity, Transform parent = null)
        {
            var data = scene.UIBind().GetBindVoByResName(resName);
            if (data == null) return null;
            var vo = data.Value;

            return Instantiate(scene, vo, parentEntity, parent);
        }
    }
}
#endif