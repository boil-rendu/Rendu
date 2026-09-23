using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    using Object = UnityEngine.Object;
    public static class MenuItemUICommon
    {
        [MenuItem("Assets/UI/Create UICommon", false, 3)]
        static void CreateUICommonByFolder()
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

            var saveName = $"{UIConstHelper.Const.UIProjectName}{UIConstHelper.Const.UICommonName}";
            var savePath = $"{path}/{saveName}.prefab";

            if (AssetDatabase.LoadAssetAtPath(savePath, typeof(Object)) != null)
            {
                UnityTipsHelper.ShowError($"已存在 请先重命名 {saveName}");
                return;
            }

            var createCommon = CreateUICommon();
            PrefabUtility.SaveAsPrefabAsset(createCommon, savePath);
            Object.DestroyImmediate(createCommon);
            UIMenuItemHelper.SelectAssetAtPath(savePath);
        }

        [MenuItem("GameObject/UI/Create UICommon", false, 3)]
        static void CreateUICommonByGameObject()
        {
            var activeObject = Selection.activeObject as GameObject;
            if (activeObject == null)
            {
                UnityTipsHelper.ShowError($"请选择一个目标");
                return;
            }

            Selection.activeObject = CreateUICommon(activeObject);
        }

        public static GameObject CreateUICommon(GameObject activeObject = null)
        {
            var commonObject = new GameObject();
            commonObject.name = $"{UIConstHelper.Const.UIProjectName}{UIConstHelper.Const.UICommonName}";
            var viewRect = commonObject.GetOrAddComponent<RectTransform>();
            commonObject.GetOrAddComponent<CanvasRenderer>();
            var cdeTable = commonObject.GetOrAddComponent<UIBindCDETable>();
            cdeTable.UICodeType = EUICodeType.Common;
            if (activeObject != null)
            {
                viewRect.SetParent(activeObject.transform, false);
                EditorUtility.SetDirty(activeObject);
            }

            commonObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));
            return commonObject;
        }
    }
}