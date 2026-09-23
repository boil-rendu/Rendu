using RD.Core;
namespace RD.Client
{
    public static class UILoadHelper
    {
        public static UILoadComponent UILoad(this Entity entity)
        {
            return entity.UIMgr().GetComponent<UILoadComponent>();
        }

        public static UILoadComponent UILoad(this Scene scene)
        {
            return scene.UIMgr().GetComponent<UILoadComponent>();
        }
    }
}