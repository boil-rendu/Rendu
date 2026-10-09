using RD.Client;
using RD.Core;

//文档 https://lib9kmxvq7k.feishu.cn/wiki/GQb8wtMiYibrxaklR9ic6RHUnUe
namespace RD.Client
{
    //UI消息  有UI被关闭之前 (有人调用了关闭XXpanel时)
    /// <summary>有 UI 被关闭之前（调用关闭面板时派发）</summary>
    public struct UIEventPanelCloseBefore
    {
        public string      UIPkgName;       //所在包名
        public string      UIResName;       //资源名称
        public string      UIComponentName; //组件名称
        public bool        StackOption;     //是否是堆栈操作来的消息
        public EPanelLayer PanelLayer;      //所在层级
    }

    //UI消息  有UI被关闭之后 (已经完成了所有加载包括动画后) 被摧毁前
    /// <summary>有 UI 被关闭之后（加载与关闭动画都已完成、即将摧毁前派发）</summary>
    public struct UIEventPanelCloseAfter
    {
        public string      UIPkgName;       //所在包名
        public string      UIResName;       //资源名称
        public string      UIComponentName; //组件名称
        public bool        StackOption;     //是否是堆栈操作来的消息
        public EPanelLayer PanelLayer;      //所在层级
    }

    //UI消息  被摧毁
    /// <summary>UI 被摧毁事件</summary>
    public struct UIEventPanelDestroy
    {
        public string      UIPkgName;       //所在包名
        public string      UIResName;       //资源名称
        public string      UIComponentName; //组件名称
        public EPanelLayer PanelLayer;      //所在层级
    }
}
