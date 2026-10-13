#if UNITY_EDITOR
using System.Collections.Generic;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>System 代码生成所需的数据（模板占位符取值来源）</summary>
    public class CreateSystemData
    {
        /// <summary>生成后是否自动刷新资源</summary>
        public bool                                                 AutoRefresh;
        /// <summary>是否弹出操作提示</summary>
        public bool                                                 ShowTips;
        /// <summary>命名空间</summary>
        public string                                               Namespace; //命名空间
        /// <summary>包名 / 模块名</summary>
        public string                                               PkgName;   //包名/模块名
        /// <summary>资源名（类名 + Base）</summary>
        public string                                               ResName;   //资源名 类名+Base
        /// <summary>重写字典：按分组组织「键 → 值」的模板替换项</summary>
        public Dictionary<string, List<Dictionary<string, string>>> OverrideDic;
    }
}
#endif
