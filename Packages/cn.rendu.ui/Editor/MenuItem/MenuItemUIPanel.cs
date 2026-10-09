using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    using Object = UnityEngine.Object;
    /// <summary>创建 UIPanel 预制体（非拆分式：IsSplitData = false）</summary>
    public static class MenuItemUIPanel
    {
        /// <summary>在资源目录右键创建</summary>
        [MenuItem("Assets/UI/Create UIPanel", false, 1)]
        static void CreateUIPanelByFolder()
        {
            var activeObject = Selection.activeObject as DefaultAsset;
            if (activeObject == null)
            {
                UnityTipsHelper.ShowError($"请在路径 {UIConstHelper.Const.ProjectResPath}/xxx/{UIConstHelper.Const.Prefabs} 下右键创建");
                return;
            }

            var path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (!path.Contains(UIConstHelper.Const.ProjectResPath))
            {
                UnityTipsHelper.ShowError($"请在路径 {UIConstHelper.Const.ProjectResPath}/xxx/{UIConstHelper.Const.Prefabs} 下右键创建");
                return;
            }

            CreateUIPanelByPath(path);
        }

        /// <summary>按路径创建并保存为预制体（重名报错），完成后选中资产</summary>
        internal static void CreateUIPanelByPath(string path)
        {
            if (!path.Contains(UIConstHelper.Const.ProjectResPath))
            {
                UnityTipsHelper.ShowError($"请在路径 {UIConstHelper.Const.ProjectResPath}/xxx/{UIConstHelper.Const.Prefabs} 下右键创建");
                return;
            }

            var saveName = $"{UIConstHelper.Const.ProjectName}{UIConstHelper.Const.PanelName}";
            var savePath = $"{path}/{saveName}.prefab";

            if (AssetDatabase.LoadAssetAtPath(savePath, typeof(Object)) != null)
            {
                UnityTipsHelper.ShowError($"已存在 请先重命名 {saveName}");
                return;
            }

            var createPanel = CreateUIPanel();
            PrefabUtility.SaveAsPrefabAsset(createPanel, savePath);
            Object.DestroyImmediate(createPanel);
            MenuItemHelper.SelectAssetAtPath(savePath);
        }

        /// <summary>
        /// 创建面板根节点（Panel 类型、非拆分、默认 TimeCache）+ 满屏阻挡图（带 UIBlock），整体设为 UI 层
        /// </summary>
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
            panelObject.name      = $"{UIConstHelper.Const.ProjectName}{UIConstHelper.Const.PanelName}";
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