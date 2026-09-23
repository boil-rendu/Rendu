using RD;
using RD.Core;

namespace RD.Client
{
    /*public interface IUIEntity
    {
        EntityRef<Entity> EntityRef { get; set; }
    }*/

    public interface IUIManager : IUISingleton
    {
        /// <summary>
        /// 已经初始化结束 且成功了
        /// 失败了可以重复初始化
        /// </summary>
        bool InitedSucceed { get; }

        //激活状态
        //激活被关闭时 Update,LateUpdate,FixedUpdate 都会被停止
        //其他不影响
        bool Enabled { get; }
    }

    public interface IUIManagerAsyncInit : IUIManager
    {
        Promise<bool> ManagerAsyncInit();
    }

    public interface IUIManagerUpdate : IUIManager
    {
        void ManagerUpdate();
    }

    public interface IUIManagerLateUpdate : IUIManager
    {
        void ManagerLateUpdate();
    }

    public interface IUIManagerFixedUpdate : IUIManager
    {
        void ManagerFixedUpdate();
    }
}