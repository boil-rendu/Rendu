using RD.Core;
namespace RD.Client
{
    public static class UIBindHelper
    {
        public static UIBindComponent UIBind(this Entity entity)
        {
            return entity.Root().GetComponent<UIMgrComponent>().GetComponent<UIBindComponent>();
        }

        public static UIBindComponent UIBind(this Scene scene)
        {
            return scene.Root().GetComponent<UIMgrComponent>().GetComponent<UIBindComponent>();
        }
    }
}