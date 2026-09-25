using RD.Inspector;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    [HideLabel]
    public enum ECheckPrefabFiltrate
    {
        [LabelText("所有")]
        All,

        [LabelText("所有可删除")]
        Delete,

        [LabelText("可删除的CDE")]
        CDE,

        [LabelText("可删除的Prefab")]
        Prefab,

        [LabelText("忽略")]
        Igonre
    }
}