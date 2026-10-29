using System.Diagnostics;
using UnityEditor;

namespace RD.Init.Editor
{
    public static class CodeModeEditor
    {
        [MenuItem("RD/Init/GlobalConfig",false, 11)]
        public static void Init()
        {
            var globalConfig = UnityEngine.Resources.Load<GlobalConfig>("GlobalConfig");

            if (globalConfig == null)
            {
                UnityEngine.Debug.LogError("[CodeModeEditor] 未找到 GlobalConfig");
                return;
            }

            // 先打开 GlobalConfig（在 Project 窗口中定位并在 Inspector 中显示）
            Selection.activeObject = globalConfig;
            EditorGUIUtility.PingObject(globalConfig);

            AssetDatabase.Refresh();
        }
    }
}