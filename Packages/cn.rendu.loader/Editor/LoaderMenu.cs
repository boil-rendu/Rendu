#if UNITY_EDITOR

using RD.Init.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// Loader 顶层菜单 (Rendu/Loader/) — 只保留两个全局热键入口:
    ///   - Compile _F6     整工程编译 (AssetDatabase.Refresh + AssemblyTool.DoCompile)
    ///   - Hot Reload _F7  运行时热重载 (走 RD.CodeLoader.Instance.Reload)
    ///
    /// 7 个工具窗口的入口 (GlobalConfig / ProjectInit / Build / Server / ProjectFiles / ScriptsReferences / Define)
    /// 已全部迁移到 LoaderTool.cs:23 的 [MenuItem("Rendu/Loader Tool")], 不再平铺在 Loader 子菜单里;
    /// 5 个平铺动作 (Build Application / Regenerate / GenerateAssemblyRef / UpdateScriptsRef / InitScripts)
    /// 也都收编到对应模块的 [Button] 按钮, 不再做菜单项。
    /// </summary>
    internal static class LoaderMenu
    {
        const string RootMenu = MenuDefine.ProjectRootMenu + "/Loader";
        // [MenuItem] 的 priority 参数是 attribute 实参, 必须是常量表达式;
        // const int + 字面量在 Roslyn (.NET 8 verifier) 不算常量, 用字面量更稳。

        const string CompileMenu  = RootMenu + "/Compile _F6";
        const string HotReloadMenu = RootMenu + "/Hot Reload _F7";

        [MenuItem(CompileMenu, false, 1040)]
        public static void Compile()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssemblyTool.DoCompile();
        }

        [MenuItem(HotReloadMenu, false, 1041)]
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
    }
}

#endif
