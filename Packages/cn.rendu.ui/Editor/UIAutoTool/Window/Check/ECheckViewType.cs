using RD.Inspector;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>检查窗口的视图类型（脚本 / 预制体）</summary>
    public enum ECheckViewType
    {
        /// <summary>脚本检查</summary>
        [LabelText("脚本")]
        Script,

        /// <summary>预制体检查</summary>
        [LabelText("预制")]
        Prefab,
    }
}