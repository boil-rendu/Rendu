using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    public static class MenuItemUIAllPopupView
    {
        [MenuItem("GameObject/UI/Create UIAllPopupView", false, 5)]
        static void CreateUIAllViewByGameObject()
        {
            var activeObject = Selection.activeObject as GameObject;
            if (activeObject == null)
            {
                UnityTipsHelper.ShowError($"请选择一个目标");
                return;
            }

            var uiBindCDETable = activeObject.GetComponent<UIBindCDETable>();
            if (uiBindCDETable == null)
            {
                UnityTipsHelper.ShowError($"请选择一个UIBindCDETable组件的GameObject");
                return;
            }

            if (uiBindCDETable.UICodeType != EUICodeType.Panel)
            {
                UnityTipsHelper.ShowError($"请选择一个面板类型的UIBindCDETable组件的GameObject");
                return;
            }

            if (uiBindCDETable.PanelSplitData.AllPopupViewParent != null)
            {
                UnityTipsHelper.ShowError($"该面板已经创建过AllPopupView");
                return;
            }

            Selection.activeObject = CreateUIAllPopupView(uiBindCDETable);
        }

        public static GameObject CreateUIAllPopupView(UIBindCDETable panelCdeTable )
        {
            var allViewObject = new GameObject();
            allViewObject.name = UIConstHelper.Const.UIAllPopupViewParentName;
            var viewRect = allViewObject.GetOrAddComponent<RectTransform>();
            allViewObject.GetOrAddComponent<CanvasRenderer>();
            viewRect.SetParent(panelCdeTable.transform, false);
            viewRect.ResetToFullScreen();
            allViewObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));

            panelCdeTable.PanelSplitData.AllPopupViewParent = viewRect;
            panelCdeTable.PanelSplitData.AllPopupView.Clear();
            EditorUtility.SetDirty(panelCdeTable);
            return allViewObject;
        }
    }
}