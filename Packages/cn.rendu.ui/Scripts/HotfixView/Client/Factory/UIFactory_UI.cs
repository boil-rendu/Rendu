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
    /// <summary>UIFactory 工厂（UI 部分）：由 GameObject 创建 UI Entity</summary>
    public static partial class UIFactory
    {
        /// <summary>设置父节点为世界坐标无关，并重置 UI 的锚点/坐标</summary>
        private static void SetParent(RectTransform self, Transform parent)
        {
            self.SetParent(parent, false);
            self.AutoReset();
        }

        /// <summary>
        /// 由「UIBindVo + 已实例化的 GameObject」创建 UI Entity
        /// 流程：校验并初始化 CDE 表 → 校验父实体必须是场景实体 → 挂到父实体下 → 添加 Vo 指定的组件 →
        /// 递归创建所有子 CDE → 最后 InitOwnerUIEntity（刻意放在最后，保证此时其他内部组件已初始化完）
        /// </summary>
        [EnableAccessEntiyChild]
        internal static Entity CreateByObjVo(UIBindVo vo, GameObject obj, Entity parentEntity)
        {
            var cdeTable = obj.GetComponent<UIBindCDETable>();
            if (cdeTable == null)
            {
                Debug.LogError($"{obj.name} 没有 UIBindCDETable 组件 无法创建 请检查");
                return null;
            }

            // 显式初始化 CDE 表，解决同一帧内激活-关闭导致 Awake 不触发的问题
            cdeTable.InitializeCDE();

            if (parentEntity is { IScene: null })
            {
                Log.Error($"{parentEntity.GetType()} 不是场景实体 无法创建");
                return null;
            }

            var uiBase = parentEntity.AddChild<UIChild, UIBindVo, GameObject>(vo, obj);
            var component = uiBase.AddComponent(vo.ComponentType);
            cdeTable.CreateComponent(component);
            uiBase.InitOwnerUIEntity(component); //这里就是要晚于其他内部组件这样才能访问时别人已经初始化好了

            return component;
        }

        /// <summary>
        /// 递归为所有子 CDE 节点创建 UI Entity
        /// 按路径查不到对应绑定 Vo 的子节点会被跳过；每层同样保持「先建完子级再 InitOwnerUIEntity」的顺序
        /// </summary>
        [EnableAccessEntiyChild]
        private static void CreateComponent(this UIBindCDETable cdeTable, Entity parentEntity)
        {
            foreach (var childCde in cdeTable.AllChildCdeTable)
            {
                if (childCde == null)
                {
                    Debug.LogError($"{cdeTable.name} 存在null对象的childCde 检查是否因为删除或丢失或未重新生成");
                    continue;
                }

                // 显式初始化子组件的 CDE 表
                childCde.InitializeCDE();

                var childGameObject = childCde.gameObject;
                var data = parentEntity.UIBind().GetBindVoByPath(childCde.PkgName, childCde.ResName);
                if (data == null) continue;
                var vo = data.Value;

                var uiBase = parentEntity.AddChild<UIChild, UIBindVo, GameObject>(vo, childGameObject);
                var component = uiBase.AddComponent(vo.ComponentType);
                cdeTable.AddUIOwner(childGameObject.name, component);
                childCde.CreateComponent(component);
                uiBase.InitOwnerUIEntity(component); //这里就是要晚于其他内部组件这样才能访问时别人已经初始化好了
            }
        }
    }
}