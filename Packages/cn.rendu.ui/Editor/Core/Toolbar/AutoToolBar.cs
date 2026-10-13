#if UI
using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>在编辑器工具栏左侧添加 UITool 入口按钮（点击后打开窗口并定位到「发布」页签）</summary>
    [InitializeOnLoad]
    public static class AutoToolBar
    {
        private static Texture _cachedIcon;

        static AutoToolBar()
        {
            ToolbarExtender.AddLeftToolbarGUI(OnUIAutoToolbarGUI, 100);
        }

        private static void OnUIAutoToolbarGUI()
        {
            _cachedIcon ??= AssetDatabase.LoadAssetAtPath<Texture>("Packages/cn.rendu.ui/Editor/Resources/UIIcon16.png");

            GUILayout.Space(5);
            GUIContent iconContent = new(string.Empty, _cachedIcon);
            iconContent.tooltip = "UITool";
            if (GUILayout.Button(iconContent))
            {
                UITool.OpenWindow();
                EditorApplication.delayCall += () =>
                {
                    var window = EditorWindow.GetWindow<UITool>();
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