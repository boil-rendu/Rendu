using RD.Init.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// Loader 菜单: 7 个工具窗口已迁移到 LoaderTool 的 [UIAutoMenu(Scope="loader")] 模块后,
    /// Loader 菜单只保留两项 — 编译/热重载快捷键 (F6/F7) 与 Open Loader Tool 入口。
    /// 原来的"Build Application / Regenerate / Generate / Init Scripts / Update Script Reference"
    /// 等动作全部收编到对应模块, 不再平铺。
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
