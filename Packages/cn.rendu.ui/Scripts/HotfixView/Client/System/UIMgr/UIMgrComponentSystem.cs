//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using RD.Client;
using RD.Core;

namespace RD.Client
{
    [FriendOf(typeof(UIMgrComponent))]
    [EntitySystemOf(typeof(UIMgrComponent))]
    public static partial class UIMgrComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIMgrComponent self)
        {
            self.m_RootRef = self.Root()?.AddComponent<UIRootComponent>();
        }

        [EntitySystem]
        private static void Destroy(this UIMgrComponent self)
        {
            self.Root()?.RemoveComponent<UIRootComponent>();
            UISingletonHelper.DisposeAll();
            self.ResetRoot();
        }
    }
}