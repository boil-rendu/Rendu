using RD.Core;
namespace RD.Client
{
    public static class UICountDownHelper
    {
        public static CountDownMgr UICountDown(this Entity entity)
        {
            return entity.UIMgr().GetComponent<CountDownMgr>();
        }

        public static CountDownMgr UICountDown(this Scene scene)
        {
            return scene.UIMgr().GetComponent<CountDownMgr>();
        }
    }
}