using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace RD.Init.Editor
{
    [CustomEditor(typeof(GlobalConfig))]
    public class GlobalConfigEditor : UnityEditor.Editor
    {
        private const string SolutionFileName = "RD.sln";
        private const string TempLinkSuffix = ".mainpackage_link_tmp";

        private CodeMode codeMode;
        
        private void OnEnable()
        {
            GlobalConfig globalConfig = (GlobalConfig)this.target;
            this.codeMode = globalConfig.CodeMode;
            //globalConfig.BuildType = EditorUserBuildSettings.development ? BuildType.Debug : BuildType.Release;
            
            EditorUtility.SetDirty(globalConfig);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            GlobalConfig globalConfig = (GlobalConfig)this.target;

            SerializedProperty iterator = this.serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (iterator.propertyPath == "m_Script")
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }

                    continue;
                }

                if (iterator.name == nameof(GlobalConfig.SceneName))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Set Main Package"))
                    {
                        SetMainPackageBySceneName(globalConfig);
                    }
                    if (GUILayout.Button("Set Code Mode"))
                    {
                        RefreshAssemblyReferenceByCodeMode(globalConfig.CodeMode);
                    }
                    EditorGUILayout.EndHorizontal();
                    continue;
                }

                EditorGUILayout.PropertyField(iterator, true);
            }

            this.serializedObject.ApplyModifiedProperties();

            if (globalConfig.CodeMode != this.codeMode)
            {
                this.codeMode = globalConfig.CodeMode;
                RefreshAssemblyReferenceByCodeMode(codeMode);
                AssetDatabase.Refresh();
            }

            EditorUtility.SetDirty(globalConfig);
        }

        private static void RefreshAssemblyReferenceByCodeMode(CodeMode codeMode)
        {
            // 以工程方式直接运行 CodeMode 工具(dotnet run --project), 避免依赖预先构建并拷贝到 Bin/RD.Init.dll。
            // 旧写法依赖 Bin/RD.Init.dll, 但该文件实际不存在(工具工程名为 RD.CodeMode 且从未部署到该路径);
            // 文件缺失时 dotnet 会退回扫描工程根目录, 而工程根含多个 .csproj, 于是报"包含多个项目文件"错误。
            Process process = ProcessHelper.DotNet($"run --project Packages/cn.rendu.init/DotNet~/RD.CodeMode/RD.CodeMode.csproj -- --CodeMode={codeMode}", ".", true);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                UnityEngine.Debug.LogError($"[GlobalConfigEditor] 刷新 AssemblyReference 失败，退出码: {process.ExitCode}");
            }
        }

        private static void Set(GlobalConfig globalConfig)
                {
                    string sceneName = globalConfig.SceneName?.Trim();
                    if (string.IsNullOrWhiteSpace(sceneName))
                    {
                        UnityEngine.Debug.LogError("[GlobalConfigEditor] SceneName 为空，无法设置主包");
                        return;
                    }
        
                    string packageName = $"cn.rendu.{sceneName.ToLowerInvariant()}";
                    string packageDirectory = Path.Combine("Packages", packageName);
                    if (!Directory.Exists(packageDirectory))
                    {
                        UnityEngine.Debug.LogError($"[GlobalConfigEditor] 包不存在: {packageName}");
                        return;
                    }
        
                    bool result = MainPackageSelector.SetAsMainPackage(packageName);
                    if (!result)
                    {
                        UnityEngine.Debug.LogError($"[GlobalConfigEditor] 设置主包失败: {packageName}");
                        return;
                    }
        
                    TryLinkMainPackageSolution(packageName);
                }
        
        
        private static void SetMainPackageBySceneName(GlobalConfig globalConfig)
        {
            string sceneName = globalConfig.SceneName?.Trim();
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                UnityEngine.Debug.LogError("[GlobalConfigEditor] SceneName 为空，无法设置主包");
                return;
            }

            string packageName = $"cn.rendu.{sceneName.ToLowerInvariant()}";
            string packageDirectory = Path.Combine("Packages", packageName);
            if (!Directory.Exists(packageDirectory))
            {
                UnityEngine.Debug.LogError($"[GlobalConfigEditor] 包不存在: {packageName}");
                return;
            }

            bool result = MainPackageSelector.SetAsMainPackage(packageName);
            if (!result)
            {
                UnityEngine.Debug.LogError($"[GlobalConfigEditor] 设置主包失败: {packageName}");
                return;
            }

            TryLinkMainPackageSolution(packageName);
        }

        private static void TryLinkMainPackageSolution(string packageName)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string sourceSolutionPath = Path.GetFullPath(Path.Combine(projectRoot, "Packages", packageName, SolutionFileName));
            string rootSolutionPath = Path.GetFullPath(Path.Combine(projectRoot, SolutionFileName));
            string tempLinkPath = Path.GetFullPath(Path.Combine(projectRoot, $"{SolutionFileName}{TempLinkSuffix}"));

            if (!File.Exists(sourceSolutionPath))
            {
                UnityEngine.Debug.LogError($"[GlobalConfigEditor] 主包中未找到 {SolutionFileName}: {sourceSolutionPath}");
                return;
            }

            try
            {
                if (!TryDeleteFileIfExists(tempLinkPath, "临时链接文件"))
                {
                    return;
                }

                if (!TryCreateHardLink(tempLinkPath, sourceSolutionPath, projectRoot))
                {
                    UnityEngine.Debug.LogError($"[GlobalConfigEditor] 创建硬连接失败: {sourceSolutionPath} -> {rootSolutionPath}");
                    return;
                }

                if (!TryDeleteFileIfExists(rootSolutionPath, $"项目根目录旧 {SolutionFileName}"))
                {
                    TryDeleteFileIfExists(tempLinkPath, "临时链接文件");
                    return;
                }

                File.Move(tempLinkPath, rootSolutionPath);
                AssetDatabase.Refresh();

                UnityEngine.Debug.Log($"[GlobalConfigEditor] 已将主包 {SolutionFileName} 硬连接到项目根目录: {rootSolutionPath}");
            }
            catch (System.Exception ex)
            {
                TryDeleteFileIfExists(tempLinkPath, "临时链接文件");

                UnityEngine.Debug.LogError($"[GlobalConfigEditor] 链接主包 {SolutionFileName} 失败: {ex.Message}");
            }
        }

        private static bool TryCreateHardLink(string linkPath, string targetPath, string workingDirectory)
        {
            Process process;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                process = ProcessHelper.Run("cmd.exe", $"/c mklink /H \"{linkPath}\" \"{targetPath}\"", workingDirectory, true);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string lnExecutable = File.Exists("/bin/ln") ? "/bin/ln" : "/usr/bin/ln";
                process = ProcessHelper.Run(lnExecutable, $"\"{targetPath}\" \"{linkPath}\"", workingDirectory, true);
            }
            else
            {
                UnityEngine.Debug.LogError($"[GlobalConfigEditor] 不支持的操作系统: {RuntimeInformation.OSDescription}");
                return false;
            }

            process.WaitForExit();
            return process.ExitCode == 0;
        }

        private static bool TryDeleteFileIfExists(string filePath, string description)
        {
            if (!File.Exists(filePath))
            {
                return true;
            }

            try
            {
                FileAttributes attributes = File.GetAttributes(filePath);
                if ((attributes & FileAttributes.ReadOnly) != 0)
                {
                    File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
                }

                File.Delete(filePath);
                UnityEngine.Debug.Log($"[GlobalConfigEditor] 已删除{description}: {filePath}");
                return true;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[GlobalConfigEditor] 删除{description}失败: {filePath}, {ex.Message}");
                return false;
            }
        }
    }
}
