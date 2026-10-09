#if UNITY_EDITOR

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>脚本（Component / System）代码生成所需的数据</summary>
    public class CreateScriptData
    {
        /// <summary>生成后是否自动刷新资源</summary>
        public bool             AutoRefresh;
        /// <summary>是否弹出操作提示</summary>
        public bool             ShowTips;
        /// <summary>命名空间</summary>
        public string           Namespace; //命名空间
        /// <summary>脚本名</summary>
        public string           Name;
        /// <summary>说明</summary>
        public string           Desc;
        /// <summary>组件类型</summary>
        public EScriptComponentType ComponentTpye;
        /// <summary>生命周期类型</summary>
        public EScriptLifeType      LifeTpye;
        /// <summary>组件脚本输出路径</summary>
        public string           ComponentPath;
        /// <summary>系统脚本输出路径</summary>
        public string           SystemPath;
    }
}
#endif