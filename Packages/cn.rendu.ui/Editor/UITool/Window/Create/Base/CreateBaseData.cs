#if UNITY_EDITOR

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>UI 基础代码（Base 类）生成所需的数据，各字段对应模板里的一个占位符</summary>
    public class CreateBaseData
    {
        /// <summary>生成后是否自动刷新资源</summary>
        public bool        AutoRefresh;
        /// <summary>是否弹出操作提示</summary>
        public bool        ShowTips;
        /// <summary>命名空间</summary>
        public string      Namespace;     //命名空间
        /// <summary>包名 / 模块名</summary>
        public string      PkgName;       //包名/模块名
        /// <summary>资源名（类名 + Base）</summary>
        public string      ResName;       //资源名 类名+Base
        /// <summary>变量声明</summary>
        public string      Variables;     //变量
        /// <summary>友好组件可修改的部分</summary>
        public string      UIFriend;      //友好组件可修改的
        /// <summary>基础组件的获取</summary>
        public string      UIBase;        //基础组件的获取
        /// <summary>绑定方法体</summary>
        public string      UIBind;        //绑定方法里面的东西
        /// <summary>解绑方法体</summary>
        public string      UIUnBind;      //解绑里面的东西
        /// <summary>所有虚方法（Event 里的注册方法）</summary>
        public string      VirtualMethod; //所有虚方法  Event里面的那些注册方法
        /// <summary>面板 / 视图枚举生成</summary>
        public string      PanelViewEnum; //枚举生成
        /// <summary>UI 类型</summary>
        public EUICodeType CodeType;      //UI类型
        /// <summary>为 Panel 时的所在层级</summary>
        public EPanelLayer PanelLayer;    //当是panel时所在层级
    }
}
#endif