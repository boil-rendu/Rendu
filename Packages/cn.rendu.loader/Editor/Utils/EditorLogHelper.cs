using System;
using RD.Core;
using UnityEditor;

namespace RD.Loader.Editor
{
    [InitializeOnLoad]
    public class EditorLogHelper
    {
        static EditorLogHelper()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += CheckCompolingFinish;
        }

        private static void CheckCompolingFinish()
        {
            if (!EditorApplication.isCompiling)
            {
                CreateLog();
                EditorApplication.update -= CheckCompolingFinish;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.ExitingPlayMode:
                    CreateLog();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    DestroyLog();
                    break;
                default:
                    break;
            }
        }

        private static void CreateLog()
        {
            if (Logger.Instance == null)
            {
                // 通过反射创建 UnityLogger，避免直接引用 Client 程序集
                var unityLoggerType = Type.GetType("RD.UnityLogger, RD.Loader");
                if (unityLoggerType != null)
                {
                    var loggerInstance = Activator.CreateInstance(unityLoggerType);
                    World.Instance.AddSingleton<Logger>().Log = (ILog)loggerInstance;
                }
            }

            if (Options.Instance == null)
            {
                World.Instance.AddSingleton(new Options());
            }
        }

        private static void DestroyLog()
        {
            World.Instance.Dispose();
        }
    }
}