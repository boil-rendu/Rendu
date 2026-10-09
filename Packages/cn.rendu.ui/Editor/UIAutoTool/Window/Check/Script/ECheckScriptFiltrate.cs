using RD.Inspector;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>脚本检查结果的筛选方式</summary>
    [HideLabel]
    public enum ECheckScriptFiltrate
    {
        /// <summary>显示全部</summary>
        [LabelText("所有")]
        All = 0,

        /// <summary>只显示可删除项</summary>
        [LabelText("可删除")]
        Delete = 1,

        /// <summary>只显示忽略项</summary>
        [LabelText("忽略")]
        Igonre = 2
    }
}