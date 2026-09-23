using System.IO;
using System.Threading;
using RD.Core;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 程序集编译服务 - 封装 AssemblyTool 的功能
    /// </summary>
    public class AssemblyCompilerService : IAssemblyCompilerService
    {
        private static readonly string[] DllNames =
        {
            "RD.Hotfix",
            "RD.HotfixView",
            "RD.Model",
            "RD.ModelView"
        };

        private SynchronizationContext _unitySynchronizationContext;

        public AssemblyCompilerService()
        {
            _unitySynchronizationContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// 执行完整编译流程
        /// </summary>
        public void Compile()
        {
            Log.Info("[AssemblyCompiler] ========== 开始编译 ==========");

            // 强制刷新，防止编译出旧代码
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            Log.Info($"[AssemblyCompiler] 当前 CodeMode: {globalConfig.CodeMode}");

            CodeModeChangeHelper.ChangeToCodeMode(globalConfig.CodeMode.ToString());

            if (!CompileDlls())
            {
                Log.Error("[AssemblyCompiler] 编译失败");
                return;
            }

            CopyHotUpdateDlls();

            Log.Info("[AssemblyCompiler] ========== 编译完成 ==========");
        }

        /// <summary>
        /// 编译 DLL
        /// </summary>
        public bool CompileDlls()
        {
            Log.Info("[AssemblyCompiler] 开始编译 DLLs...");

            // 运行时编译需要先设置为 UnitySynchronizationContext，编译完再还原
            SynchronizationContext lastSynchronizationContext = Application.isPlaying ? SynchronizationContext.Current : null;
            SynchronizationContext.SetSynchronizationContext(_unitySynchronizationContext);

            bool isCompileOk = false;

            try
            {
                Directory.CreateDirectory(Define.BuildOutputDir);
                BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

                Log.Info($"[AssemblyCompiler] 编译目标: {group} / {target}");

                ScriptCompilationSettings settings = new()
                {
                    group = group,
                    target = target,
                    extraScriptingDefines = new[] { "IS_COMPILING" },
                    options = EditorUserBuildSettings.development
                        ? ScriptCompilationOptions.DevelopmentBuild
                        : ScriptCompilationOptions.None
                };

                ScriptCompilationResult result = PlayerBuildInterface.CompilePlayerScripts(settings, Define.BuildOutputDir);
                isCompileOk = result.assemblies.Count > 0;

                if (isCompileOk)
                {
                    Log.Info($"[AssemblyCompiler] 编译成功，生成 {result.assemblies.Count} 个程序集");
                    foreach (var assembly in result.assemblies)
                    {
                        Log.Info($"[AssemblyCompiler]   - {assembly}");
                    }
                }
                else
                {
                    Log.Error("[AssemblyCompiler] 编译失败，未生成任何程序集");
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
        /// 复制热更新 DLLs
        /// </summary>
        public void CopyHotUpdateDlls()
        {
            Log.Info($"[AssemblyCompiler] 复制热更新 DLLs 到: {Define.CodeDir}");

            FileHelper.CleanDirectory(Define.CodeDir);

            foreach (string dllName in DllNames)
            {
                string sourceDll = $"{Define.BuildOutputDir}/{dllName}.dll";
                string sourcePdb = $"{Define.BuildOutputDir}/{dllName}.pdb";
                File.Copy(sourceDll, $"{Define.CodeDir}/{dllName}.dll.bytes", true);
                File.Copy(sourcePdb, $"{Define.CodeDir}/{dllName}.pdb.bytes", true);
                Log.Info($"[AssemblyCompiler]   复制: {dllName}.dll, {dllName}.pdb");
            }

            AssetDatabase.Refresh();
        }
    }
}
