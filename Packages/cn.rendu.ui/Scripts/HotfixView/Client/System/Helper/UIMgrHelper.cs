using RD.Core;
namespace RD.Client
{
    public static class UIMgrHelper
    {
        /// <summary>
        /// 从任意位置获取到UI管理器组件
        /// </summary>
        public static UIMgrComponent UIMgr(this Entity entity)
        {
            return entity.Root().GetComponent<UIMgrComponent>();
        }

        /// <summary>
        /// 从任意位置获取到UI根组件
        /// </summary>
        public static UIRootComponent UIRoot(this Entity entity)
        {
            return entity.Root().GetComponent<UIRootComponent>();
        }

        /// <summary>
        /// 获取指定场景的UI根组件
        /// </summary>
        public static UIRootComponent UISceneRoot(this Entity entity)
        {
            return entity.Scene().GetComponent<UIRootComponent>();
        }

        /// <summary>
        /// 获取指定场景的UI根组件
        /// </summary>
        public static UIRootComponent UISceneRoot(this Scene scene)
        {
            return scene.GetComponent<UIRootComponent>();
        }
    }
}