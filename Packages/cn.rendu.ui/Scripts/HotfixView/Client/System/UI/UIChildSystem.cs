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
    /// UI主体
    /// </summary>
    [FriendOf(typeof(UIChild))]
    [EntitySystemOf(typeof(UIChild))]
    public static partial class UIChildSystem
    {
        [EntitySystem]
        private static void Awake(this UIChild self, UIBindVo uiBindVo, GameObject obj)
        {
            self.InitUIBase(uiBindVo, obj);
        }

        [EntitySystem]
        private static void Destroy(this UIChild self)
        {
            if (self.OwnerGameObject == null) return;
            UnityEngine.Object.Destroy(self.OwnerGameObject);
        }

        //设置当前拥有的这个实际UI 之后初始化
        internal static void InitOwnerUIEntity(this UIChild self, Entity uiEntity)
        {
            self.m_OwnerUIEntity = uiEntity;
            self.UIInitialize();
        }

        /// <summary>
        /// 初始化UIBase 由PanelMgr创建对象后调用
        /// 外部禁止
        /// </summary>
        private static void InitUIBase(this UIChild self, UIBindVo uiBindVo, GameObject ownerGameObject)
        {
            if (ownerGameObject == null)
            {
                Debug.LogError($"null对象无法初始化");
                return;
            }

            self.OwnerGameObject = ownerGameObject;
            self.OwnerRectTransform = self.OwnerGameObject.GetComponent<RectTransform>();
            self.CDETable = self.OwnerGameObject.GetComponent<UIBindCDETable>();
            if (self.CDETable == null)
            {
                Debug.LogError($"{self.OwnerGameObject.name} 没有UIBindCDETable组件 这是必须的");
                return;
            }

            self.ComponentTable = self.CDETable.ComponentTable;
            self.DataTable = self.CDETable.DataTable;
            self.EventTable = self.CDETable.EventTable;
            self.m_UIBindVo = uiBindVo;
            self.UIResName = uiBindVo.ResName;
            self.m_UIBaseInit = true;
            self.CDETable.Entity = self;
            self.CDETable.UIBaseStart = self.UIBaseStart;
            self.CDETable.UIBaseOnDestroy = self.UIBaseOnDestroy;
            self.AddUIDataComponent();
        }

        //根据UI类型添加其他组件
        private static void AddUIDataComponent(this UIChild self)
        {
            switch (self.m_UIBindVo.CodeType)
            {
                case EUICodeType.Panel:
                    self.AddComponent<UIWindowComponent>();
                    self.AddComponent<UIPanelComponent>();
                    break;
                case EUICodeType.View:
                    self.AddComponent<UIWindowComponent>();
                    self.AddComponent<UIViewComponent>();
                    break;
                case EUICodeType.Common:
                    break;
                default:
                    Debug.LogError($"没有这个类型 {self.m_UIBindVo.CodeType}");
                    break;
            }
        }

        private static void UIDataComponentInitialize(this UIChild self)
        {
            try
            {
                switch (self.m_UIBindVo.CodeType)
                {
                    case EUICodeType.Panel:
                        UIEventSystem.Initialize(self.GetComponent<UIWindowComponent>());
                        UIEventSystem.Initialize(self.GetComponent<UIPanelComponent>());
                        break;
                    case EUICodeType.View:
                        UIEventSystem.Initialize(self.GetComponent<UIWindowComponent>());
                        UIEventSystem.Initialize(self.GetComponent<UIViewComponent>());
                        break;
                    case EUICodeType.Common:
                        break;
                    default:
                        Debug.LogError($"没有这个类型 {self.m_UIBindVo.CodeType}");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        #region 公共方法

        /// <summary>
        /// 设置显隐
        /// </summary>
        public static void SetActive(this UIChild self, bool value)
        {
            if (self.OwnerGameObject == null) return;
            self.OwnerGameObject.SetActive(value);
        }

        //其他的关于 RectTransform 相关的 不建议包一层
        //就直接 OwnerRectTransform. 使用Unity API 就可以了 没必要包一成
        //这么多方法 都有可能用到你都包一层嘛

        #endregion

        #region 生命周期

        private static void UIBaseStart(this UIChild self)
        {
        }

        private static void UIInitialize(this UIChild self)
        {
            self.UIDataComponentInitialize();
            try
            {
                UIEventSystem.Bind(self.OwnerUIEntity);
                UIEventSystem.Initialize(self.OwnerUIEntity);

                if (self.OwnerUIEntity is IUIEnable)
                {
                    if (self.ActiveSelf)
                    {
                        self.UIBaseOnEnable();
                    }

                    self.CDETable.UIBaseOnEnable = self.UIBaseOnEnable;
                }

                if (self.OwnerUIEntity is IUIDisable)
                {
                    if (!self.ActiveSelf)
                    {
                        self.UIBaseOnDisable();
                    }

                    self.CDETable.UIBaseOnDisable = self.UIBaseOnDisable;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private static void UIBaseOnEnable(this UIChild self)
        {
            try
            {
                UIEventSystem.Enable(self.OwnerUIEntity);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        private static void UIBaseOnDisable(this UIChild self)
        {
            try
            {
                UIEventSystem.Disable(self.OwnerUIEntity);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        //UI对象被移除时
        private static void UIBaseOnDestroy(this UIChild self)
        {
            if (!self.IsDisposed)
            {
                self.Parent.RemoveChild(self.Id);
                EventSystem.Instance?.UIInvokeEntitySyncSafety(UISingletonHelper.UIMgr, new UIInvokeEntity_ReleaseInstantiate
                {
                    obj = self.OwnerGameObject
                });
            }
        }

        #endregion
    }
}