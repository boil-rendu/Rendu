using System.Collections.Generic;
using RD.Utilities;
using UnityEditor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    /// <summary>
    /// RD包管理 更新检测窗口
    /// </summary>
    public class RDPackageUpdateWindow : EditorWindow
    {
        [MenuItem("RD/PackageManager/更新检查", false, 110)]
        private static void OpenWindow()
        {
            var window = GetWindow<RDPackageUpdateWindow>();
            if (window != null)
                window.Show();
        }

        [MenuItem("RD/PackageManager/关闭更新检查", false, 111)]
        //错误时使用的 面板出现了错误 会导致如何都打不开 就需要先关闭
        public static void CloseWindow()
        {
            GetWindow<RDPackageUpdateWindow>()?.Close();
        }

        private string m_UpdatePackageInfo;

        private GUIStyle m_CustomStyle;

        private void OnEnable()
        {
            m_CustomStyle       = new(EditorStyles.wordWrappedMiniLabel) { fontSize = 30, richText = true };
            m_UpdatePackageInfo = "更新信息请求中...";
            PackageHelper.CheckUpdateAll(CheckUpdateAll);
        }

        private void CheckUpdateAll(bool result)
        {
            if (!result)
            {
                m_UpdatePackageInfo = "更新失败...";
                return;
            }

            ShowUpdateInfo();
        }

        private void ShowUpdateInfo()
        {
            var sb = SbPool.Get();

            sb.Append("可更新包信息:");
            sb.AppendLine();
            sb.AppendLine();

            var currentRDPackageInfo = new Dictionary<string, string>();

            foreach (var packageInfo in UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages())
            {
                var name = packageInfo.name;

                if (name.Contains("cn.rendu."))
                {
                    var version = packageInfo.version;
                    currentRDPackageInfo[name] = version;
                }
            }

            var newRDPackageInfo = new Dictionary<string, string>();

            foreach (var info in currentRDPackageInfo)
            {
                var name       = info.Key;
                var newVersion = PackageHelper.GetPackageLastVersion(name);
                if (string.IsNullOrEmpty(newVersion))
                {
                    continue;
                }

                var version = info.Value;
                if (version != newVersion)
                {
                    newRDPackageInfo[name] = newVersion;
                }
            }

            if (newRDPackageInfo.Count <= 0)
            {
                sb.Append($"无");
            }
            else
            {
                foreach (var info in newRDPackageInfo)
                {
                    var name       = info.Key;
                    var newVersion = info.Value;
                    var version    = currentRDPackageInfo[name];

                    sb.Append($"[{name} {version}] >> [<color=#00FF00>{newVersion}</color>]");
                    sb.AppendLine();
                }
            }

            m_UpdatePackageInfo = SbPool.PutAndToStr(sb);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("提示:需要成体系地浏览包与依赖资产请用 RD 自绘检视器");

            GUILayout.Space(10);
            if (GUILayout.Button("RDPackageManager 包管理文档"))
            {
                RDPackageDocumentModule.OpenLocalDocSection("Packages/cn.rendu.packagemanager/Docs/Tutorial.md", "4.2 更新检查窗口");
            }
            GUILayout.Space(10);
            
            EditorGUILayout.LabelField(m_UpdatePackageInfo, m_CustomStyle);
        }
    }
}
