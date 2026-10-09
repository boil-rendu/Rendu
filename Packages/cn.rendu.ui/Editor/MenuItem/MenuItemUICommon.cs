using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    using Object = UnityEngine.Object;
    /// <summary>创建 UICommon 预制体（Common 类型的 UIBindCDETable）</summary>
    public static class MenuItemUICommon
    {
        /// <summary>在资源目录右键创建：保存为预制体并选中（重名会报错）</summary>
        [MenuItem("Assets/UI/Create UICommon", false, 3)]
        static void CreateUICommonByFolder()
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

            var saveName = $"{UIConstHelper.Const.ProjectName}{UIConstHelper.Const.CommonName}";
            var savePath = $"{path}/{saveName}.prefab";

            if (AssetDatabase.LoadAssetAtPath(savePath, typeof(Object)) != null)
            {
                UnityTipsHelper.ShowError($"已存在 请先重命名 {saveName}");
                return;
            }

            var createCommon = CreateUICommon();
            PrefabUtility.SaveAsPrefabAsset(createCommon, savePath);
            Object.DestroyImmediate(createCommon);
            MenuItemHelper.SelectAssetAtPath(savePath);
        }

        /// <summary>在层级里右键创建：直接挂在选中对象之下</summary>
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

        /// <summary>创建 Common 根节点（RectTransform + CanvasRenderer + UIBindCDETable，UI 层）</summary>
        public static GameObject CreateUICommon(GameObject activeObject = null)
        {
            var commonObject = new GameObject();
            commonObject.name = $"{UIConstHelper.Const.ProjectName}{UIConstHelper.Const.CommonName}";
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