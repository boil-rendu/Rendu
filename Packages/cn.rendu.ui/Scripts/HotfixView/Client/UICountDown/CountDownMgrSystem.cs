using RD.Core;
namespace RD.Client
{
    /// <summary>倒计时管理器系统（Life 周期驱动；具体增删逻辑在 CountDownMgr 内部）</summary>
    [FriendOf(typeof(CountDownMgr))]
    [EntitySystemOf(typeof(CountDownMgr))]
    public static partial class CountDownMgrSystem
    {
        [EntitySystem]
        private static void Awake(this CountDownMgr self)
        {
        }

        [EntitySystem]
        private static void Destroy(this CountDownMgr self)
        {
        }

        /// <summary>每帧末驱动倒计时推进</summary>
        [EntitySystem]
        private static void LateUpdate(this CountDownMgr self)
        {
            self.ManagerUpdate();
        }
    }
}