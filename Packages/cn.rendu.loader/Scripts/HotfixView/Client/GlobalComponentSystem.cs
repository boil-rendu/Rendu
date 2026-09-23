using System;
using RD.Core;
using RD.Loader;
using UnityEngine;

namespace RD.Client
{
    [EntitySystemOf(typeof(GlobalComponent))]
    public static partial class GlobalComponentSystem
    {
        /// <summary>
        /// 场景常驻节点路径 对应 Launcher 场景里 Global 下的三个容器
        /// UnitRoot 放实体视图 / ScenesRoot 放 Entity 视图(编辑器调试) / UIRoot 是 UIRoot.prefab 实例
        /// </summary>
        private const string GlobalPath = "/Global";
        private const string UnitRootPath = "/Global/UnitRoot";
        private const string UIRootPath = "/Global/UIRoot";

        [EntitySystem]
        private static void Awake(this GlobalComponent self)
        {
            self.Global = FindOrCreateSlot(GlobalPath);
            self.Unit = FindOrCreateSlot(UnitRootPath);
            self.UI = FindOrCreateSlot(UIRootPath);
            self.GlobalConfig = Resources.Load<GlobalConfig>("GlobalConfig");

            if (self.GlobalConfig == null)
            {
                Log.Error("GlobalComponent 初始化失败: Resources 下没有找到 GlobalConfig");
            }
        }

        /// <summary>
        /// 场景里摆好的容器直接用 没摆的运行时补一个同名空节点
        /// 与 UnitRoot / ScenesRoot 同一套路: 都是 Global 下的空容器 内容运行时挂进去
        /// 这样场景或 prefab 缺节点时不会直接 NRE 也不用为了取节点去改场景文件
        /// </summary>
        private static Transform FindOrCreateSlot(string path)
        {
            GameObject slot = GameObject.Find(path);
            if (slot != null)
            {
                return slot.transform;
            }

            int split = path.LastIndexOf('/');
            GameObject go = new GameObject(path.Substring(split + 1));

            if (split > 0)
            {
                go.transform.SetParent(FindOrCreateSlot(path.Substring(0, split)), false);
            }
            else
            {
                // 连 Global 根节点都没有 创建出来的需要保持与 Init.cs 一致的跨场景语义
                UnityEngine.Object.DontDestroyOnLoad(go);
            }

            Log.Warning($"场景中没有找到 {path} 节点, 已在运行时创建");
            return go.transform;
        }
    }
}