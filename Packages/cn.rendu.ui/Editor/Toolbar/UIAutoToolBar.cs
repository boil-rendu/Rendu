#if UI
using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    [InitializeOnLoad]
    public static class UIAutoToolBar
    {
        private static Texture _cachedIcon;

        static UIAutoToolBar()
        {
            UIToolbarExtender.AddLeftToolbarGUI(OnUIAutoToolbarGUI, 100);
        }

        private static void OnUIAutoToolbarGUI()
        {
            _cachedIcon ??= AssetDatabase.LoadAssetAtPath<Texture>("Packages/cn.rendu.ui/Editor/Resources/UIIcon16.png");

            GUILayout.Space(5);
            GUIContent iconContent = new(string.Empty, _cachedIcon);
            iconContent.tooltip = "UIAutoTool";
            if (GUILayout.Button(iconContent))
            {
                UIAutoTool.OpenWindow();
                EditorApplication.delayCall += () =>
                {
                    var window = EditorWindow.GetWindow<UIAutoTool>();
                    if (window != null)
                    {
                        window.SelectModule("发布");
                    }
                };
            }

            GUILayout.Space(5);
        }
    }
}
#endif