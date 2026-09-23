using System.IO;
using System.Threading;
using RD.Core;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

namespace RD.Loader.Editor
{
    public static class AssemblyTool
    {
        /// <summary>
        /// Unity线程的同步上下文
        /// </summary>
        static SynchronizationContext unitySynchronizationContext { get; set; }

        /// <summary>
        /// 程序集名字数组
        /// </summary>
        public static readonly string[] DllNames = { "RD.Hotfix", "RD.HotfixView", "RD.Model", "RD.ModelView" };

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            unitySynchronizationContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// 执行编译代码流程
        /// </summary>
        public static void DoCompile()
        {
            Debug.Log("[AssemblyTool] ========== 开始编译 ==========");

            // 强制刷新一下，防止关闭auto refresh，编译出老代码
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            Debug.Log($"[AssemblyTool] 当前 CodeMode: {globalConfig.CodeMode}");

            CodeModeChangeHelper.ChangeToCodeMode(globalConfig.CodeMode.ToString());

            bool isCompileOk = CompileDlls();
            if (!isCompileOk)
            {
                Debug.LogError("[AssemblyTool] 编译失败");
                return;
            }

            CopyHotUpdateDlls();

            Debug.Log("[AssemblyTool] ========== 编译完成 ==========");
        }

        /// <summary>
        /// 编译成dll
        /// </summary>
        static bool CompileDlls()
        {
            Debug.Log("[AssemblyTool] 开始编译 DLLs...");

            // 运行时编译需要先设置为UnitySynchronizationContext, 编译完再还原为CurrentContext
            SynchronizationContext lastSynchronizationContext = Application.isPlaying ? SynchronizationContext.Current : null;
            SynchronizationContext.SetSynchronizationContext(unitySynchronizationContext);

            bool isCompileOk = false;

            try
            {
                Directory.CreateDirectory(Define.BuildOutputDir);
                BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

                Debug.Log($"[AssemblyTool] 编译目标: {group} / {target}");

                ScriptCompilationSettings scriptCompilationSettings = new()
                {
                    group = group,
                    target = target,
                    extraScriptingDefines = new[] { "IS_COMPILING" },
                    options = EditorUserBuildSettings.development ? ScriptCompilationOptions.DevelopmentBuild : ScriptCompilationOptions.None
                };

                ScriptCompilationResult result = PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, Define.BuildOutputDir);
                isCompileOk = result.assemblies.Count > 0;

                if (isCompileOk)
                {
                    Debug.Log($"[AssemblyTool] 编译成功，生成 {result.assemblies.Count} 个程序集");
                    foreach (var assembly in result.assemblies)
                    {
                        Debug.Log($"[AssemblyTool]   - {assembly}");
                    }

                    // 列出输出目录中的所有文件
                    if (Directory.Exists(Define.BuildOutputDir))
                    {
                        var outputFiles = Directory.GetFiles(Define.BuildOutputDir);
                        Debug.Log($"[AssemblyTool] 输出目录中的文件:");
                        foreach (var file in outputFiles)
                        {
                            Debug.Log($"[AssemblyTool]     - {Path.GetFileName(file)} ({new FileInfo(file).Length / 1024} KB)");
                        }
                    }
                }
                else
                {
                    Debug.LogError("[AssemblyTool] 编译失败，未生成任何程序集");
                }

                EditorUtility.ClearProgressBar();
            }
            finally
            {
                if (lastSynchronizationContext != null)
                {
                    SynchronizationContext.SetSynchronizationContext(lastSynchronizationContext);
                }
            }

            return isCompileOk;
        }

        /// <summary>
        /// 将dll文件复制到加载目录
        /// </summary>
        static void CopyHotUpdateDlls()
        {
            Debug.Log($"[AssemblyTool] 复制热更新 DLLs 到: {Define.CodeDir}");

            // 检查输出目录中是否存在文件
            string outputDir = Path.GetFullPath(Define.BuildOutputDir);
            Debug.Log($"[AssemblyTool] 编译输出目录: {outputDir}");

            if (!Directory.Exists(outputDir))
            {
                Debug.LogError($"[AssemblyTool] 编译输出目录不存在: {outputDir}");
                return;
            }

            var allDllFiles = Directory.GetFiles(outputDir, "*.dll");
            Debug.Log($"[AssemblyTool] 输出目录中的 DLL 文件数量: {allDllFiles.Length}");

            // 清空目标目录
            FileHelper.CleanDirectory(Define.CodeDir);

            int successCount = 0;
            int skippedCount = 0;

            // 复制所有 DLL 文件，不管是否在 DllNames 列表中
            foreach (string dllPath in allDllFiles)
            {
                string dllName = Path.GetFileNameWithoutExtension(dllPath);
                string sourcePdb = Path.ChangeExtension(dllPath, ".pdb");

                // 跳过非目标 DLL（如 Unity 自带的 DLL）
                if (!dllName.StartsWith("RD."))
                {
                    Debug.Log($"[AssemblyTool]   跳过: {dllName} (非 RD 程序集)");
                    skippedCount++;
                    continue;
                }

                try
                {
                    File.Copy(dllPath, $"{Define.CodeDir}/{dllName}.dll.bytes", true);

                    if (File.Exists(sourcePdb))
                    {
                        File.Copy(sourcePdb, $"{Define.CodeDir}/{dllName}.pdb.bytes", true);
                        Debug.Log($"[AssemblyTool]   复制成功: {dllName}.dll + pdb");
                    }
                    else
                    {
                        Debug.Log($"[AssemblyTool]   复制成功: {dllName}.dll (无 pdb)");
                    }

                    successCount++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[AssemblyTool] 复制失败 {dllName}: {ex.Message}");
                }
            }

            Debug.Log($"[AssemblyTool] 成功复制 {successCount} 个 DLL 文件，跳过 {skippedCount} 个文件");

            if (successCount == 0)
            {
                Debug.LogWarning($"[AssemblyTool] 没有复制任何 RD 开头的 DLL 文件！这可能是因为代码目录为空。");
            }

            AssetDatabase.Refresh();
        }
    }
}