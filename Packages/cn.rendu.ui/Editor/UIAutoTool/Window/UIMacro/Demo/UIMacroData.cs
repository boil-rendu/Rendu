#if UNITY_EDITOR

using System;
using RD.Inspector;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    [Flags]
    [LabelText("RD")]
    [UIEnumUnityMacro]
    public enum EUIETMacroType : long
    {
        [LabelText("所有")]
        ALL = -1,

        [LabelText("无")]
        NONE = 0,

        [LabelText("ET10")]
        ET10 = 1,

        [LabelText("已初始化")]
        INITED = 1 << 1,

        [LabelText("可视化")]
        ENABLE_VIEW = 1 << 2,
    }

    [Flags]
    [LabelText("UI")]
    [UIEnumUnityMacro]
    public enum EUIMacroType : long
    {
        [LabelText("所有")]
        ALL = -1,

        [LabelText("无")]
        NONE = 0,

        [LabelText("UI")]
        UI = 1,

        [LabelText("模拟非编辑器状态")]
        UIMACRO_SIMULATE_NONEEDITOR = 1 << 1,

        [LabelText("UIBing 初始化")]
        UIMACRO_BIND_INITIALIZE = 1 << 2,

        [LabelText("界面开关")]
        UIMACRO_PANEL_OPENCLOSE = 1 << 3,

        [LabelText("运行时调试UI")]
        UIMACRO_BIND_RUNTIME_EDITOR = 1 << 4,

        [LabelText("红点堆栈收集")]
        UIMACRO_REDDOT_STACK = 1 << 5,

        [LabelText("绑定使用反射")]
        UIMACRO_BIND_REFLECTION = 1 << 6, //打包后你也想用反射加载绑定信息的 打开这个

        [LabelText("绑定使用反射 编辑器时使用ET DLL")]
        UIMACRO_BIND_BYETDLL = 1 << 7, //使用反射时 有2种方式 1.使用DLL(ET工程编译出来的) 2.使用Unity的程序集(Unity编译的)

        [LabelText("绑定使用反射 运行时使用AppDomain程序集 全部")]
        UIMACRO_BIND_BYUNITYDLL_ALL = 1 << 8, //默认只找RD.ModelView 开了这个会找全部的程序集
    }

    [Flags]
    [LabelText("资源加载")]
    [UIEnumUnityMacro]
    public enum EUIAssetsMacroType : long
    {
        [LabelText("所有")]
        ALL = -1,

        [LabelText("无")]
        NONE = 0,

        [LabelText("关闭同步加载")]
        UIMACRO_SYNCLOAD_CLOSE = 1,
    }
}
#endif