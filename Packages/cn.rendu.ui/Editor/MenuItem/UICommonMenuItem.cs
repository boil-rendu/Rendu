using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    internal static partial class UICommonMenuItem
    {
        public static GameObject CreateTarget(string path, string targetName)
        {
            var activeObject = Selection.activeObject as GameObject;
            if (activeObject == null)
            {
                UnityTipsHelper.ShowError($"请选择一个对象 右键创建");
                return null;
            }

            var clonePath = $"{path}/{targetName}.prefab";
            var obj       = UIMenuItemHelper.CloneGameObjectByPath(clonePath, activeObject.transform);
            Selection.activeObject = obj;
            return obj;
        }

        internal static GameObject CreateTarget(string targetName)
        {
            var path = $"Packages/cn.rendu.ui/Editor/TemplatePrefabs/UI";
            return CreateTarget(path, targetName);
        }

        [MenuItem("GameObject/UI/UIBlockBG", false, 100000)]
        private static void CreateText_UIBlockBG()
        {
            CreateTarget("UIBlockBG");
        }

        [MenuItem("GameObject/UI/Text_NoRaycast", false, 100001)]
        private static void CreateText_NoRaycast()
        {
            CreateTarget("UIText_NoRaycast");
        }

        [MenuItem("GameObject/UI/Text (TMP)", false, 100002)]
        private static void CreateTextTMP()
        {
            CreateTarget("UIText (TMP)");
        }

        [MenuItem("GameObject/UI/Image_NoRaycast", false, 100003)]
        private static void CreateImage_NoRaycast()
        {
            CreateTarget("UIImage_NoRaycast");
        }

        [MenuItem("GameObject/UI/Button", false, 100004)]
        private static void CreateButton()
        {
            CreateTarget("UIButton");
        }

        [MenuItem("GameObject/UI/Button_NoText", false, 100005)]
        private static void CreateButton_NoText()
        {
            CreateTarget("UIButton_NoText");
        }

        [MenuItem("GameObject/UI/Click_Event", false, 110001)]
        private static void CreateClick_Event()
        {
            var obj = CreateTarget("UIBlockBG");
            obj.name = "ClickEvent";
            obj.AddComponent<UIClickEffect>();
            obj.AddComponent<UIEventBindClick>();
        }

        [MenuItem("GameObject/UI/Click_Task", false, 110002)]
        private static void CreateClick_Task()
        {
            var obj = CreateTarget("UIBlockBG");
            obj.name = "ClickTask";
            obj.AddComponent<UIClickEffect>();
            obj.AddComponent<UITaskEventBindClick>();
        }
    }
}