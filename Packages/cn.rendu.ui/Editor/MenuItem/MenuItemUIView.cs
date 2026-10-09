using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    using Object = UnityEngine.Object;
    /// <summary>创建 UIView 预制体</summary>
    public static class MenuItemUIView
    {
        /// <summary>在资源目录右键创建</summary>
        [MenuItem("Assets/UI/Create UIView", false, 2)]
        static void CreateUIViewByFolder()
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

            var saveName = UIConstHelper.Const.GetDefaultName(UIConstHelper.Const.ViewName);
            var savePath = $"{path}/{saveName}.prefab";

            if (AssetDatabase.LoadAssetAtPath(savePath, typeof(Object)) != null)
            {
                UnityTipsHelper.ShowError($"已存在 请先重命名 {saveName}");
                return;
            }

            var createView = CreateUIView();
            PrefabUtility.SaveAsPrefabAsset(createView, savePath);
            Object.DestroyImmediate(createView);
            MenuItemHelper.SelectAssetAtPath(savePath);
        }

        /// <summary>
        /// 在层级里右键创建：只能在 AllViewParent / AllPopupViewParent 下使用
        /// 会自动新建一层 ViewParent 承载 View，并按父节点分别登记到 AllCreateView（View）或 AllPopupView（Popup）
        /// </summary>
        [MenuItem("GameObject/UI/Create UIView", false, 2)]
        static void CreateUIViewByGameObject()
        {
            var activeObject = Selection.activeObject as GameObject;
            if (activeObject == null)
            {
                UnityTipsHelper.ShowError($"请选择ViewParent 右键创建");
                return;
            }

            var panelCdeTable = activeObject.transform.parent.GetComponentInParent<UIBindCDETable>();
            if (panelCdeTable == null)
            {
                UnityTipsHelper.ShowError($"只能在AllViewParent / AllPopupViewParent 下使用 快捷创建View");
                return;
            }

            if (panelCdeTable.UICodeType != EUICodeType.Panel)
            {
                UnityTipsHelper.ShowError($"必须是Panel 下使用 快捷创建View");
                return;
            }

            var panelEditorData = panelCdeTable.PanelSplitData;

            if (activeObject != panelEditorData.AllViewParent?.gameObject &&
                activeObject != panelEditorData.AllPopupViewParent?.gameObject)
            {
                UnityTipsHelper.ShowError($"只能在AllViewParent / AllPopupViewParent 下使用 快捷创建View");
                return;
            }

            //ViewParent
            var viewParentObject = new GameObject();
            var viewParentRect = viewParentObject.GetOrAddComponent<RectTransform>();
            viewParentObject.name = UIConstHelper.Const.GetDefaultName(UIConstHelper.Const.ViewParentName);
            viewParentRect.SetParent(activeObject.transform, false);
            viewParentRect.ResetToFullScreen();

            //View
            var viewObject = CreateUIView(viewParentObject);
            var cdeTable = viewObject.GetOrAddComponent<UIBindCDETable>();
            if (activeObject == panelEditorData.AllViewParent.gameObject)
            {
                panelEditorData.AllCreateView.Add(viewParentRect);
                cdeTable.ViewWindowType = EViewWindowType.View;
            }
            else if (activeObject == panelEditorData.AllPopupViewParent.gameObject)
            {
                panelEditorData.AllPopupView.Add(viewParentRect);
                cdeTable.ViewWindowType = EViewWindowType.Popup;
            }

            viewParentObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));
            Selection.activeObject = viewParentObject;
        }

        /// <summary>创建 View 根节点（View 类型、满屏、UI 层）</summary>
        public static GameObject CreateUIView(GameObject activeObject = null)
        {
            var viewObject = new GameObject();
            viewObject.name = UIConstHelper.Const.GetDefaultName(UIConstHelper.Const.ViewName);
            var viewRect = viewObject.GetOrAddComponent<RectTransform>();
            viewObject.GetOrAddComponent<CanvasRenderer>();
            var cdeTable = viewObject.GetOrAddComponent<UIBindCDETable>();
            cdeTable.UICodeType = EUICodeType.View;
            if (activeObject != null)
            {
                viewRect.SetParent(activeObject.transform, false);
                EditorUtility.SetDirty(activeObject);
            }
            viewRect.ResetToFullScreen();
            viewObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));
            return viewObject;
        }
    }
}