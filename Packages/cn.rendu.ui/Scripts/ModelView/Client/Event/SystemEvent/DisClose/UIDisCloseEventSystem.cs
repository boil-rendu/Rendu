using System;
using RD.Core;

namespace RD.Client
{
    public static partial class UIEventSystem
    {
        /// <summary>
        /// 当一个界面 EPanelOption.DisClose 时 (禁止关闭)
        /// 且又被调用时 则会触发 可根据需求继承
        /// 根据需求返回 是否可以被关闭
        /// 返回true 就是可以被关闭
        /// </summary>
        public static async Promise<bool> DisClose(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return false;
            }

            var iUIBanCloseSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof(IUIDisCloseSystem));
            if (iUIBanCloseSystems == null)
            {
                return false;
            }

            EntityRef<Entity> componentRef = component;
            foreach (IUIDisCloseSystem aUIBanCloseSystem in iUIBanCloseSystems)
            {
                if (aUIBanCloseSystem == null)
                {
                    continue;
                }

                try
                {
                    return await aUIBanCloseSystem.Run(componentRef);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            return false;
        }
    }
}