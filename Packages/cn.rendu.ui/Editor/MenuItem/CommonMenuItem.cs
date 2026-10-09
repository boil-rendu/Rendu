using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>常用 UI 控件的 GameObject 菜单（GameObject/UI/RD/*）</summary>
    internal static partial class CommonMenuItem
    {
        /// <summary>以模板预制体克隆一个节点到当前选中对象之下，并选中克隆结果</summary>
        public static GameObject CreateTarget(string path, string targetName)
        {
            var activeObject = Selection.activeObject as GameObject;
            if (activeObject == null)
            {
                UnityTipsHelper.ShowError($"请选择一个对象 右键创建");
                return null;
            }

            var clonePath = $"{path}/{targetName}.prefab";
            var obj       = MenuItemHelper.CloneGameObjectByPath(clonePath, activeObject.transform);
            Selection.activeObject = obj;
            return obj;
        }

        /// <summary>以内置模板目录下的同名预制体创建</summary>
        internal static GameObject CreateTarget(string targetName)
        {
            var path = $"Packages/cn.rendu.ui/Editor/TemplatePrefabs";
            return CreateTarget(path, targetName);
        }

        [MenuItem("GameObject/UI/RD/UIBlockBG", false, 1)]
        private static void CreateText_UIBlockBG()
        {
            CreateTarget("UIBlockBG");
        }

        [MenuItem("GameObject/UI/RD/Text_NoRaycast", false, 2)]
        private static void CreateText_NoRaycast()
        {
            CreateTarget("UIText_NoRaycast");
        }

        [MenuItem("GameObject/UI/RD/Text (TMP)", false, 3)]
        private static void CreateTextTMP()
        {
            CreateTarget("UIText (TMP)");
        }

        [MenuItem("GameObject/UI/RD/Image_NoRaycast", false, 4)]
        private static void CreateImage_NoRaycast()
        {
            CreateTarget("UIImage_NoRaycast");
        }

        [MenuItem("GameObject/UI/RD/Button", false, 5)]
        private static void CreateButton()
        {
            CreateTarget("UIButton");
        }

        [MenuItem("GameObject/UI/RD/Button_NoText", false, 6)]
        private static void CreateButton_NoText()
        {
            CreateTarget("UIButton_NoText");
        }

        [MenuItem("GameObject/UI/RD/InputField", false, 7)]
        private static void CreateInputField()
        {
            CreateTarget("UIInputField");
        }

        [MenuItem("GameObject/UI/RD/Toggle", false, 8)]
        private static void CreateToggle()
        {
            CreateTarget("UIToggle");
        }

        [MenuItem("GameObject/UI/RD/Slider", false, 9)]
        private static void CreateSlider()
        {
            CreateTarget("UISlider");
        }

        [MenuItem("GameObject/UI/RD/ScrollView", false, 11)]
        private static void CreateScrollView()
        {
            CreateTarget("UIScrollView");
        }

        [MenuItem("GameObject/UI/RD/Empty", false, 12)]
        private static void CreateEmpty()
        {
            CreateTarget("UIEmpty");
        }

        /// <summary>创建带 UIClickEffect + UIEventBindClick 的点击事件节点</summary>
        [MenuItem("GameObject/UI/RD/Click_Event", false, 21)]
        private static void CreateClick_Event()
        {
            var obj = CreateTarget("UIBlockBG");
            obj.name = "ClickEvent";
            obj.AddComponent<UIClickEffect>();
            obj.AddComponent<UIEventBindClick>();
        }

        /// <summary>创建带 UIClickEffect + UITaskEventBindClick 的点击事件节点</summary>
        [MenuItem("GameObject/UI/RD/Click_Task", false, 22)]
        private static void CreateClick_Task()
        {
            var obj = CreateTarget("UIBlockBG");
            obj.name = "ClickTask";
            obj.AddComponent<UIClickEffect>();
            obj.AddComponent<UITaskEventBindClick>();
        }
    }
}