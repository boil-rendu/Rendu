using RD.Core;
namespace RD.Client
{
    /// <summary>UIBindComponent 快捷获取扩展（Entity / Scene 两种入口）</summary>
    public static class UIBindHelper
    {
        /// <summary>取实体所在场景的 UIBindComponent</summary>
        public static UIBindComponent UIBind(this Entity entity)
        {
            return entity.Root().GetComponent<UIMgrComponent>().GetComponent<UIBindComponent>();
        }

        /// <summary>取场景的 UIBindComponent</summary>
        public static UIBindComponent UIBind(this Scene scene)
        {
            return scene.Root().GetComponent<UIMgrComponent>().GetComponent<UIBindComponent>();
        }
    }
}