using RD.Core;
//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

namespace RD.Client
{
    [FriendOf(typeof(UIRootComponent))]
    [EntitySystemOf(typeof(UIRootComponent))]
    public static partial class UIRootComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIRootComponent self)
        {
            self.m_UIMgrRef = self.UIMgr();
            self.AddComponent<HashWait>();
        }
    }
}
