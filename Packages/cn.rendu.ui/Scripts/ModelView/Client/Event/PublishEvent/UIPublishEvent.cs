using RD.Client;
using RD.Core;

//文档 https://lib9kmxvq7k.feishu.cn/wiki/GQb8wtMiYibrxaklR9ic6RHUnUe
namespace RD.Client
{
    //UI初始化前,单例前
    public struct UIEventInitializeBefore
    {
    }

    //UI初始化过后的事件 其他模块各自初始化
    public struct UIEventInitializeAfter
    {
    }

    //一个UI的信息 在不同环境下表达的意境不同
    public struct UIEventPanelInfo
    {
        public string UIPkgName; //所在包名
        public string UIResName; //资源名称
        public string UIComponentName; //组件名称
        public EPanelLayer PanelLayer; //所在层级
    }
}