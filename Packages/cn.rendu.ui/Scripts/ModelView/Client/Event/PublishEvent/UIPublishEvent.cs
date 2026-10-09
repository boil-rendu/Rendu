using RD.Client;
using RD.Core;

//文档 https://lib9kmxvq7k.feishu.cn/wiki/GQb8wtMiYibrxaklR9ic6RHUnUe
namespace RD.Client
{
    //UI初始化前,单例前
    /// <summary>UI 初始化前事件（在 UI 单例初始化前派发，各模块借此挂载自己的管理器）</summary>
    public struct UIEventInitializeBefore
    {
    }

    //UI初始化过后的事件 其他模块各自初始化
    /// <summary>UI 初始化后事件（框架就绪后派发，其他模块各自完成初始化）</summary>
    public struct UIEventInitializeAfter
    {
    }

    //一个UI的信息 在不同环境下表达的意境不同
    /// <summary>一个 UI 的描述信息（包名 / 资源名 / 组件名 / 层级）</summary>
    public struct UIEventPanelInfo
    {
        public string UIPkgName; //所在包名
        public string UIResName; //资源名称
        public string UIComponentName; //组件名称
        public EPanelLayer PanelLayer; //所在层级
    }
}