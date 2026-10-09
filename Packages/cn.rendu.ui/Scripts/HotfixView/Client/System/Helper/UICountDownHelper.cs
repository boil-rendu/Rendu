using RD.Core;
namespace RD.Client
{
    /// <summary>CountDownMgr 快捷获取扩展（Entity / Scene 两种入口）</summary>
    public static class UICountDownHelper
    {
        /// <summary>取实体所属 UIMgr 下的倒计时管理器</summary>
        public static CountDownMgr UICountDown(this Entity entity)
        {
            return entity.UIMgr().GetComponent<CountDownMgr>();
        }

        /// <summary>取场景 UIMgr 下的倒计时管理器</summary>
        public static CountDownMgr UICountDown(this Scene scene)
        {
            return scene.UIMgr().GetComponent<CountDownMgr>();
        }
    }
}