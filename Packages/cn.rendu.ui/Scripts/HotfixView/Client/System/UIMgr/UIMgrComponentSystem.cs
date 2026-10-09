//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>UIMgrComponent 系统（生命周期部分）</summary>
    [FriendOf(typeof(UIMgrComponent))]
    [EntitySystemOf(typeof(UIMgrComponent))]
    public static partial class UIMgrComponentSystem
    {
        /// <summary>创建时在 Root 下挂载 UIRootComponent 并持有其引用</summary>
        [EntitySystem]
        private static void Awake(this UIMgrComponent self)
        {
            self.m_RootRef = self.Root()?.AddComponent<UIRootComponent>();
        }

        /// <summary>销毁时移除 UIRootComponent、释放所有 UI 单例并重置 Root 引用</summary>
        [EntitySystem]
        private static void Destroy(this UIMgrComponent self)
        {
            self.Root()?.RemoveComponent<UIRootComponent>();
            UISingletonHelper.DisposeAll();
            self.ResetRoot();
        }
    }
}