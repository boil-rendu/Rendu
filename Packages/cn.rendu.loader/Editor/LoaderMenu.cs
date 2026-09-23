using RD.Init.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// Loader 菜单管理器
    /// 提供统一的菜单入口和快捷键管理
    ///
    /// </summary>
    ///
    
    
    
    internal static class LoaderMenu
    {
        const string RootMenu = MenuDefine.ProjectRootMenu + "/Loader";
        const  int RootMenuPriority = 1040;
        
        const string GlobalConfigMenu = RootMenu +"/Global Config";
        const string ProjectInitMenu = RootMenu +"/Project Init";
        const string BuildPackageMenu = RootMenu +"/Build Package";
        const string ServerManagerMenu = RootMenu +"/Server Manager";
        const string ProjectFilesManagerMenu = RootMenu +"/Project Files Manager";
        const string ScriptReferenceManagerMenu = RootMenu +"/Script Reference Manager";
        const string MacroDefinitionManagerMenu = RootMenu +"/Macro Definition Manager";
        
        const string CompileMenu = RootMenu +"/Compile _F6";
        const string HotReloadMenu = RootMenu +"/Hot Reload _F7";
        
        
        const string BuildApplicationMenu = RootMenu +"/Build Application";
        const string RegenerateProjectFilesMenu = RootMenu +"/Regenerate Project Files";
        const string GenerateAssemblyReferenceMenu = RootMenu +"/Generate Assembly Reference";
        const string InitScriptsMenu = RootMenu +"/Init Scripts";
        const string UpdateScriptReferenceMenu = RootMenu +"/Update Script Reference";
        
        
        #region 项目配置管理窗口 

        [MenuItem(GlobalConfigMenu, false, RootMenuPriority)]
        public static void OpenGlobalConfig()
        {
            GlobalConfigWindow.ShowWindow();
        }

        [MenuItem(ProjectInitMenu, false, RootMenuPriority+1)]
        public static void OpenRootMenuPriority()
        {
            ProjectInitWindow.ShowWindow();
        }

        [MenuItem(BuildPackageMenu, false, RootMenuPriority+2)]
        public static void OpenBuildPackage()
        {
            BuildWindow.ShowWindow();
        }

        [MenuItem(ServerManagerMenu, false, RootMenuPriority+3)]
        public static void OpenServerManager()
        {
            ServerWindow.ShowWindow();
        }

        [MenuItem(ProjectFilesManagerMenu, false, RootMenuPriority+4)]
        public static void OpenProjectFilesManager()
        {
            ProjectFilesWindow.ShowWindow();
        }

        [MenuItem(ScriptReferenceManagerMenu, false, RootMenuPriority+5)]
        public static void OpenScriptsReferencesManager()
        {
            ScriptsReferencesWindow.ShowWindow();
        }

        [MenuItem(MacroDefinitionManagerMenu, false, RootMenuPriority+6)]
        public static void OpenMacroDefineManager()
        {
            DefineWindow.ShowWindow();
        }

        #endregion

        #region 快速操作 

        [MenuItem(CompileMenu, false)]
        public static void Compile()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssemblyTool.DoCompile();
        }

        [MenuItem(HotReloadMenu, false)]
        public static void Reload()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[热重载] 热重载功能只能在游戏运行时使用！");
                EditorUtility.DisplayDialog("提示", "热重载功能只能在游戏运行时使用！", "确定");
                return;
            }

            try
            {
                var codeLoaderType = System.Type.GetType("RD.CodeLoader, RD.Loader");
                if (codeLoaderType == null)
                {
                    Debug.LogError("[热重载] 未找到 RD.CodeLoader 类型");
                    EditorUtility.DisplayDialog("错误", "未找到 CodeLoader 类！", "确定");
                    return;
                }

                var instanceProperty = codeLoaderType.GetProperty("Instance");
                var codeLoader = instanceProperty?.GetValue(null);
                var reloadMethod = codeLoaderType?.GetMethod("Reload");

                Debug.Log("[热重载] 开始执行热重载...");
                reloadMethod?.Invoke(codeLoader, null);
                Debug.Log("[热重载] 热重载执行完成");
                EditorUtility.DisplayDialog("成功", "热重载执行成功！", "确定");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[热重载] 热重载失败：{ex.Message}");
                EditorUtility.DisplayDialog("错误", $"热重载失败！\n{ex.Message}", "确定");
            }
        }

        #endregion

        #region 构建工具 

        [MenuItem(BuildApplicationMenu, false,RootMenuPriority+6)]
        public static void BuildApplication()
        {
            // 根据当前构建平台确定 PlatformType
            PlatformType platformType = GetPlatformType(EditorUserBuildSettings.activeBuildTarget);

            // 使用默认构建选项
            new BuildApplicationCommand(platformType, BuildOptions.None).Execute();
        }

        private static PlatformType GetPlatformType(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return PlatformType.Windows;
                case BuildTarget.StandaloneOSX:
                    return PlatformType.MacOS;
                case BuildTarget.StandaloneLinux64:
                    return PlatformType.Linux;
                case BuildTarget.Android:
                    return PlatformType.Android;
                case BuildTarget.iOS:
                    return PlatformType.IOS;
                case BuildTarget.WebGL:
                    return PlatformType.WebGL;
                default:
                    return PlatformType.None;
            }
        }

        [MenuItem(RegenerateProjectFilesMenu, false, RootMenuPriority+7)]
        public static void RegenerateProjectFiles()
        {
            new RegenerateProjectFilesCommand().Execute();
        }

        #endregion

        #region 开发工具

        [MenuItem(GenerateAssemblyReferenceMenu, false, RootMenuPriority+8)]
        public static void GenerateAssemblyReference()
        {
            // 使用默认的 Client 模式生成程序集引用
            new GenerateAssemblyReferenceCommand(CodeMode.Client).Execute();
        }

        [MenuItem(InitScriptsMenu, false)]
        public static void RunInitScripts()
        {
            new RunInitScriptsCommand().Execute();
        }

        [MenuItem(UpdateScriptReferenceMenu, false, RootMenuPriority+9)]
        public static void UpdateScriptsReferences()
        {
            new UpdateScriptsReferencesCommand().Execute();
        }

        #endregion
    }
}
