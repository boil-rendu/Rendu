using System;
using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI通用等待
    /// </summary>
    [FriendOf(typeof(UIWaitComponent))]
    [EntitySystemOf(typeof(UIWaitComponent))]
    public static partial class UIWaitComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIWaitComponent self, long waitId)
        {
            self.Reset(waitId);
        }

        [EntitySystem]
        private static void Destroy(this UIWaitComponent self)
        {
            self.Notify(EHashWaitError.Destroy);
        }

        [EntitySystem]
        private static async Promise UIWindowClose(this UIWaitComponent self, bool viewCloseResult)
        {
            if (viewCloseResult)
            {
                self.Notify(EHashWaitError.Success);
            }

            await Promise.CompletedTask;
        }

        public static void Notify(this UIWaitComponent self, EHashWaitError error)
        {
            if (self.m_IsWaitCompleted) return;
            self.m_IsWaitCompleted = true;
            self.Root().UISceneRoot()?.GetComponent<HashWait>()?.Notify(self.m_WaitId, error);
            self.m_WaitId = 0;
        }

        public static void Reset(this UIWaitComponent self, long waitId = 0)
        {
            if (self.m_WaitId == waitId) return;

            if (self.m_WaitId != 0)
            {
                self.Notify(EHashWaitError.Reset);
            }

            if (waitId == 0)
            {
                Log.Error($"错误 waitId不能等于0");
                self.m_IsWaitCompleted = true;
                return;
            }

            self.m_WaitId          = waitId;
            self.m_IsWaitCompleted = false;
        }
    }
}
