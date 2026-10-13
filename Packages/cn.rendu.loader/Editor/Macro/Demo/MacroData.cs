#if UNITY_EDITOR

using System;
using RD.Panel;

namespace RD.Loader.Editor
{
    /// <summary>框架自身的宏开关(RD 段)</summary>
    [Flags]
    [LabelText("RD")]
    [EnumMacro]
    public enum EMacroType : long
    {
        /// <summary>全选</summary>
        [LabelText("所有")]
        ALL = -1,

        /// <summary>都不选</summary>
        [LabelText("无")]
        NONE = 0,

        /// <summary>已初始化</summary>
        [LabelText("已初始化")]
        INITED = 1 << 1,

        /// <summary>可视化</summary>
        [LabelText("可视化")]
        ENABLE_VIEW = 1 << 2,
    }

    /// <summary>UI 框架的宏开关(UI 段)</summary>
    [Flags]
    [LabelText("UI")]
    [EnumMacro]
    public enum EUIMacroType : long
    {
        /// <summary>全选</summary>
        [LabelText("所有")]
        ALL = -1,

        /// <summary>都不选</summary>
        [LabelText("无")]
        NONE = 0,

        /// <summary>启用 UI 系统</summary>
        [LabelText("UI")]
        UI = 1,

        /// <summary>模拟非编辑器状态</summary>
        [LabelText("模拟非编辑器状态")]
        UIMACRO_SIMULATE_NONEEDITOR = 1 << 1,

        /// <summary>UIBind 初始化</summary>
        [LabelText("UIBing 初始化")]
        UIMACRO_BIND_INITIALIZE = 1 << 2,

        /// <summary>界面开关</summary>
        [LabelText("界面开关")]
        UIMACRO_PANEL_OPENCLOSE = 1 << 3,

        /// <summary>运行时调试 UI</summary>
        [LabelText("运行时调试UI")]
        UIMACRO_BIND_RUNTIME_EDITOR = 1 << 4,

        /// <summary>红点堆栈收集</summary>
        [LabelText("红点堆栈收集")]
        UIMACRO_REDDOT_STACK = 1 << 5,

        /// <summary>绑定使用反射(打包后若也走反射则开启)</summary>
        [LabelText("绑定使用反射")]
        UIMACRO_BIND_REFLECTION = 1 << 6, //打包后你也想用反射加载绑定信息的 打开这个

        /// <summary>反射时使用编辑器 DLL 方式</summary>
        [LabelText("绑定使用反射 编辑器时使用 DLL")]
        UIMACRO_BIND_BYETDLL = 1 << 7, //使用反射时 有2种方式 1.使用DLL 2.使用Unity的程序集(Unity编译的)

        /// <summary>反射时扫描 AppDomain 下全部程序集(默认只找 RD.ModelView)</summary>
        [LabelText("绑定使用反射 运行时使用AppDomain程序集 全部")]
        UIMACRO_BIND_BYUNITYDLL_ALL = 1 << 8, //默认只找RD.ModelView 开了这个会找全部的程序集
    }

    /// <summary>资源加载相关的宏开关</summary>
    [Flags]
    [LabelText("资源加载")]
    [EnumMacro]
    public enum EUIAssetsMacroType : long
    {
        /// <summary>全选</summary>
        [LabelText("所有")]
        ALL = -1,

        /// <summary>都不选</summary>
        [LabelText("无")]
        NONE = 0,

        /// <summary>关闭同步加载</summary>
        [LabelText("关闭同步加载")]
        UIMACRO_SYNCLOAD_CLOSE = 1,
    }
}
#endif
