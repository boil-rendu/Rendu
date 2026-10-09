using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>UI 事件系统扩展（预加载分发部分）</summary>
    public static partial class UIEventSystem
    {
        /// <summary>
        /// 依次执行该实体上所有 IUIPreLoadSystem；单个系统异常只记日志，不中断其余系统的执行
        /// </summary>
        public static async Promise PreLoad(Entity component)
        {
            var iPreLoadSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof(IUIPreLoadSystem));
            if (iPreLoadSystems == null)
            {
                return;
            }

            EntityRef<Entity> componentRef = component;
            foreach (IUIPreLoadSystem aPreLoadSystem in iPreLoadSystems)
            {
                if (aPreLoadSystem == null)
                {
                    continue;
                }

                try
                {
                    await aPreLoadSystem.Run(componentRef);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}