using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    using Object = UnityEngine.Object;
    /// <summary>
    /// 创建 UIPanelSource 预制体（拆分式面板：IsSplitData = true）
    /// 相比普通面板额外预置 AllView / AllPopupView 两个容器节点
    /// </summary>
    public static class MenuItemUIPanelSource
    {
        /// <summary>在 Source 目录右键创建（目录名必须正好是 Source）</summary>
        [MenuItem("Assets/UI/Create UIPanelSource", false, 0)]
        static void CreateUIPanelByFolder()
        {
            var activeObject = Selection.activeObject as DefaultAsset;
            if (activeObject == null)
            {
                UnityTipsHelper.ShowError($"请在路径 {UIConstHelper.Const.ProjectResPath}/xxx/{UIConstHelper.Const.Source} 下右键创建");
                return;
            }

            var path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (activeObject.name != UIConstHelper.Const.Source ||
                !path.Contains(UIConstHelper.Const.ProjectResPath))
            {
                UnityTipsHelper.ShowError($"请在路径 {UIConstHelper.Const.ProjectResPath}/xxx/{UIConstHelper.Const.Source} 下右键创建");
                return;
            }

            CreateUIPanelByPath(path);
        }

        /// <summary>按路径创建并保存为预制体；name 为空时用默认名，否则拼上 PanelSourceName 后缀</summary>
        internal static void CreateUIPanelByPath(string path, string name = null)
        {
            if (!path.Contains(UIConstHelper.Const.ProjectResPath))
            {
                UnityTipsHelper.ShowError($"请在路径 {UIConstHelper.Const.ProjectResPath}/xxx/{UIConstHelper.Const.Source} 下右键创建");
                return;
            }

            var saveName = string.IsNullOrEmpty(name)
                    ? UIConstHelper.Const.GetDefaultName(UIConstHelper.Const.PanelSourceName)
                    : $"{name}{UIConstHelper.Const.PanelSourceName}";
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
        /// 创建拆分式面板根节点（Panel 类型、IsSplitData = true、默认 TimeCache）
        /// 依次搭建：面板本体 → 满屏阻挡图（UIBlock）→ AllView 节点 → AllPopupView 节点，并登记到 PanelSplitData
        /// </summary>
        public static GameObject CreateUIPanel(GameObject activeObject = null)
        {
            //panel
            var panelObject = new GameObject();
            var panelRect   = panelObject.GetOrAddComponent<RectTransform>();
            panelObject.GetOrAddComponent<CanvasRenderer>();
            var cdeTable = panelObject.GetOrAddComponent<UIBindCDETable>();
            cdeTable.UICodeType  =  EUICodeType.Panel;
            cdeTable.IsSplitData =  true;
            cdeTable.PanelOption |= EPanelOption.TimeCache;

            var panelEditorData = cdeTable.PanelSplitData;
            panelEditorData.Panel = panelObject;
            panelObject.name      = UIConstHelper.Const.GetDefaultName(UIConstHelper.Const.PanelSourceName);
            if (activeObject != null)
                panelRect.SetParent(activeObject.transform, false);
            panelRect.ResetToFullScreen();

            //阻挡图
            var backgroundObject = new GameObject();
            var backgroundRect   = backgroundObject.GetOrAddComponent<RectTransform>();
            backgroundObject.GetOrAddComponent<CanvasRenderer>();
            backgroundObject.GetOrAddComponent<UIBlock>();
            backgroundObject.name = "UIBlockBG";
            backgroundRect.SetParent(panelRect, false);
            backgroundRect.ResetToFullScreen();

            // 添加allView节点
            var allViewObject = new GameObject();
            var allViewRect   = allViewObject.GetOrAddComponent<RectTransform>();
            allViewObject.name = UIConstHelper.Const.AllViewParentName;
            allViewRect.SetParent(panelRect, false);
            allViewRect.ResetToFullScreen();
            panelEditorData.AllViewParent = allViewRect;

            // 添加AllPopupView节点
            var allPopupViewObject = new GameObject();
            var allPopupViewRect   = allPopupViewObject.GetOrAddComponent<RectTransform>();
            allPopupViewObject.name = UIConstHelper.Const.AllPopupViewParentName;
            allPopupViewRect.SetParent(panelRect, false);
            allPopupViewRect.ResetToFullScreen();
            panelEditorData.AllPopupViewParent = allPopupViewRect;

            panelObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));

            return panelObject;
        }
    }
}