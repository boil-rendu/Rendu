namespace RD
{
    public static partial class Definition
    {
        // UI EntitySystem 扫描的接口清单, 接口方法名统一为接口名去掉 I 前缀

        public const string IUIWindowCloseInterface   = "RD.Client.IUIWindowClose";
        public const string UIWindowCloseMethod       = "UIWindowClose";
        public const string IUIBackCloseInterface     = "RD.Client.IUIBackClose";
        public const string UIBackCloseMethod         = "UIBackClose";
        public const string IUIBackOpenInterface      = "RD.Client.IUIBackOpen";
        public const string UIBackOpenMethod          = "UIBackOpen";
        public const string IUIBackHomeCloseInterface = "RD.Client.IUIBackHomeClose";
        public const string UIBackHomeCloseMethod     = "UIBackHomeClose";
        public const string IUICloseInterface         = "RD.Client.IUIClose";
        public const string UICloseMethod             = "UIClose";
        public const string IUIOpenInterface          = "RD.Client.IUIOpen";
        public const string UIOpenMethod              = "UIOpen";
        public const string IUIDisCloseInterface      = "RD.Client.IUIDisClose";
        public const string UIDisCloseMethod          = "UIDisClose";

   

        public const string IUIBackHomeOpenInterface  = "RD.Client.IUIBackHomeOpen";
        public const string UIBackHomeOpenMethod      = "UIBackHomeOpen";
        public const string IUICloseTweenInterface    = "RD.Client.IUICloseTween";
        public const string UICloseTweenMethod        = "UICloseTween";
        public const string IUIOpenTweenInterface     = "RD.Client.IUIOpenTween";
        public const string UIOpenTweenMethod         = "UIOpenTween";
        public const string IUIBindInterface          = "RD.Client.IUIBind";
        public const string UIBindMethod              = "UIBind";
        public const string IUIDisableInterface       = "RD.Client.IUIDisable";
        public const string UIDisableMethod           = "UIDisable";
        public const string IUIEnableInterface        = "RD.Client.IUIEnable";
        public const string UIEnableMethod            = "UIEnable";
        public const string IUIInitializeInterface    = "RD.Client.IUIInitialize";
        public const string UIInitializeMethod        = "UIInitialize";
        public const string IUICloseTweenEndInterface = "RD.Client.IUICloseTweenEnd";
        public const string UICloseTweenEndMethod     = "UICloseTweenEnd";
        public const string IUIOpenTweenEndInterface  = "RD.Client.IUIOpenTweenEnd";
        public const string UIOpenTweenEndMethod      = "UIOpenTweenEnd";

        public const string UIEventPanelInfoType      = "RD.Client.UIEventPanelInfo";

        // UI EntitySystem 接口方法的返回类型签名, 即 MethodMetaName 里 "|" 之后的部分
        public const string AsyncPromiseReturnType      = "async Promise";
        public const string AsyncPromiseBoolReturnType  = "async Promise<bool>";
        public const string BoolType                    = "bool";
    }
}
