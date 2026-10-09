using RD.Core;
//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

namespace RD.Client
{
    /// <summary>UIRootComponent 系统（生命周期部分）</summary>
    [FriendOf(typeof(UIRootComponent))]
    [EntitySystemOf(typeof(UIRootComponent))]
    public static partial class UIRootComponentSystem
    {
        /// <summary>创建时持有 UIMgr 引用并挂载 HashWait（用于等待面板的同步点）</summary>
        [EntitySystem]
        private static void Awake(this UIRootComponent self)
        {
            self.m_UIMgrRef = self.UIMgr();
            self.AddComponent<HashWait>();
        }
    }
}
