using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    using Object = UnityEngine.Object;
    public static class MenuItemUIPanel
    {
        [MenuItem("Assets/UI/Create UIPanel", false, 1)]
        static void CreateUIPanelByFolder()
        {
            var activeObject = Selection.activeObject as DefaultAsset;
            if (activeObject == null)
            {
                UnityTipsHelper.ShowError($"请在路径 {UIConstHelper.Const.UIProjectResPath}/xxx/{UIConstHelper.Const.UIPrefabs} 下右键创建");
                return;
            }

            var path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (!path.Contains(UIConstHelper.Const.UIProjectResPath))
            {
                UnityTipsHelper.ShowError($"请在路径 {UIConstHelper.Const.UIProjectResPath}/xxx/{UIConstHelper.Const.UIPrefabs} 下右键创建");
                return;
            }

            CreateUIPanelByPath(path);
        }

        internal static void CreateUIPanelByPath(string path)
        {
            if (!path.Contains(UIConstHelper.Const.UIProjectResPath))
            {
                UnityTipsHelper.ShowError($"请在路径 {UIConstHelper.Const.UIProjectResPath}/xxx/{UIConstHelper.Const.UIPrefabs} 下右键创建");
                return;
            }

            var saveName = $"{UIConstHelper.Const.UIProjectName}{UIConstHelper.Const.UIPanelName}";
            var savePath = $"{path}/{saveName}.prefab";

            if (AssetDatabase.LoadAssetAtPath(savePath, typeof(Object)) != null)
            {
                UnityTipsHelper.ShowError($"已存在 请先重命名 {saveName}");
                return;
            }

            var createPanel = CreateUIPanel();
            PrefabUtility.SaveAsPrefabAsset(createPanel, savePath);
            Object.DestroyImmediate(createPanel);
            UIMenuItemHelper.SelectAssetAtPath(savePath);
        }

        private static GameObject CreateUIPanel(GameObject activeObject = null)
        {
            //panel
            var panelObject = new GameObject();
            var panelRect   = panelObject.GetOrAddComponent<RectTransform>();
            panelObject.GetOrAddComponent<CanvasRenderer>();
            var cdeTable = panelObject.GetOrAddComponent<UIBindCDETable>();
            cdeTable.UICodeType  =  EUICodeType.Panel;
            cdeTable.IsSplitData =  false;
            cdeTable.PanelOption |= EPanelOption.TimeCache;

            var panelEditorData = cdeTable.PanelSplitData;
            panelEditorData.Panel = panelObject;
            panelObject.name      = $"{UIConstHelper.Const.UIProjectName}{UIConstHelper.Const.UIPanelName}";
            if (activeObject != null)
            {
                panelRect.SetParent(activeObject.transform, false);
                EditorUtility.SetDirty(activeObject);
            }
            panelRect.ResetToFullScreen();

            //阻挡图
            var backgroundObject = new GameObject();
            var backgroundRect   = backgroundObject.GetOrAddComponent<RectTransform>();
            backgroundObject.GetOrAddComponent<CanvasRenderer>();
            backgroundObject.GetOrAddComponent<UIBlock>();
            backgroundObject.name = "UIBlockBG";
            backgroundRect.SetParent(panelRect, false);
            backgroundRect.ResetToFullScreen();

            panelObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));
            return panelObject;
        }
    }
}