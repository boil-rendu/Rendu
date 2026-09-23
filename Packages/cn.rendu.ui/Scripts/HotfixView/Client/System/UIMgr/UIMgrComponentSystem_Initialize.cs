using RD.Client;
using RD.Core;

namespace RD.Client
{
    public static partial class UIMgrComponentSystem
    {
        //UI 初始化
        //不要在这个类中增加逻辑,应该用其他消息来实现
        //如果觉得消息不够顺序不对可扩展
        public static async Promise<bool> Initialize(this UIMgrComponent self)
        {
            EntityRef<UIMgrComponent> selfRef = self;

            //UI资源管理
            var loadComponent = self.AddComponent<UILoadComponent>();
            var loadResult = await loadComponent.Initialize();
            if (!loadResult) return false;

            //UI常量管理
            self = selfRef;
            var constResult = await UIConstHelper.LoadAsset(self.Scene());
            if (!constResult) return false;

            //初始化UI绑定
            self = selfRef;
            var bindComponent = self.AddComponent<UIBindComponent>();
            // 由 RD.UI.BindSourceGenerator 自动生成（源码见 cn.rendu.ui/DotNet~）
            var bindResult = bindComponent.InitAllBind(UICodeGenerated.UIBindProvider.Get());
            if (!bindResult) return false;

            //初始化其他UI框架中的管理器
            self = selfRef;
            await EventSystem.Instance.PublishAsync(self.Scene(), new UIEventInitializeBefore());

            //初始化所有UI相关 单例
            self = selfRef;
            await UISingletonHelper.InitializeAll(self);

            //初始化UIRoot
            self = selfRef;
            var rootResult = await self.InitRoot();
            if (!rootResult) return false;
            self = selfRef;
            self.InitSafeArea();

            //其他模块各自初始化
            await EventSystem.Instance.PublishAsync(self.Scene(), new UIEventInitializeAfter());

            return true;
        }
    }
}
