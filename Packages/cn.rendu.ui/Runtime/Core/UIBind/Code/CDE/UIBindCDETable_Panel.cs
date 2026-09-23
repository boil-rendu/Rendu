using RD.Inspector;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    //Panel的分块数据
    public sealed partial class UIBindCDETable
    {
        [SerializeField]
        [LabelText("源数据")]
        [ReadOnly]
        [HideInInspector]
        internal bool IsSplitData;

        [SerializeField]
        [HideLabel]
        [BoxGroup("面板拆分数据", centerLabel: true)]
        #if UNITY_EDITOR
        [ShowIf("ShowIfPanelSplit")]
        #endif
        internal UIPanelSplitData PanelSplitData = new();

        #if UNITY_EDITOR

        private bool ShowIfPanelSplit()
        {
            return PanelSplitData.AllViewParent != null || PanelSplitData.AllPopupViewParent != null;
        }
        #endif
    }
}