using RD.Inspector;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    //Panel的分块数据
    /// <summary>
    /// UIBindCDETable 的面板拆分数据（partial）
    /// 携带 Panel 的拆分信息（各 View 的父节点等），供编辑器拆分检查使用
    /// </summary>
    public sealed partial class UIBindCDETable
    {
        /// <summary>是否为拆分数据</summary>
        [SerializeField]
        [LabelText("源数据")]
        [ReadOnly]
        [HideInInspector]
        internal bool IsSplitData;

        /// <summary>面板拆分数据（含 AllView/AllPopupView 的父节点）</summary>
        [SerializeField]
        [HideLabel]
        [BoxGroup("面板拆分数据", centerLabel: true)]
        #if UNITY_EDITOR
        [ShowIf("ShowIfPanelSplit")]
        #endif
        internal UIPanelSplitData PanelSplitData = new();

        #if UNITY_EDITOR

        /// <summary>仅当存在任一 View 父节点时才展开显示</summary>
        private bool ShowIfPanelSplit()
        {
            return PanelSplitData.AllViewParent != null || PanelSplitData.AllPopupViewParent != null;
        }
        #endif
    }
}
