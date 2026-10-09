using RD.Core;
namespace RD.Client
{
    /// <summary>UILoadComponent 快捷获取扩展（Entity / Scene 两种入口）</summary>
    public static class UILoadHelper
    {
        /// <summary>取实体所属 UIMgr 下的资源加载组件</summary>
        public static UILoadComponent UILoad(this Entity entity)
        {
            return entity.UIMgr().GetComponent<UILoadComponent>();
        }

        /// <summary>取场景 UIMgr 下的资源加载组件</summary>
        public static UILoadComponent UILoad(this Scene scene)
        {
            return scene.UIMgr().GetComponent<UILoadComponent>();
        }
    }
}