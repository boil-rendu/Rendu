using RD.Inspector;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>预制体检查结果的筛选方式</summary>
    [HideLabel]
    public enum ECheckPrefabFiltrate
    {
        /// <summary>显示全部</summary>
        [LabelText("所有")]
        All,

        /// <summary>只显示所有可删除项</summary>
        [LabelText("所有可删除")]
        Delete,

        /// <summary>只显示可删除的 CDE</summary>
        [LabelText("可删除的CDE")]
        CDE,

        /// <summary>只显示可删除的 Prefab</summary>
        [LabelText("可删除的Prefab")]
        Prefab,

        /// <summary>只显示忽略项</summary>
        [LabelText("忽略")]
        Igonre
    }
}