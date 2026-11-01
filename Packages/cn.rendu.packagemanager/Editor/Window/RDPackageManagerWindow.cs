using RD.EditTool.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    /// <summary>
    /// RD包管理 工具窗口。
    /// 左树 + 右面板 + 模块扫描 + 选中调度 + 上次选中持久化全部由
    /// [cn.rendu.edittool](../cn.rendu.edittool/README.md) 的 <see cref="GenericToolWindow"/> 提供
    /// (模块类挂 [ToolMenu] 即入树), 本类只提供窗口入口和包管理自身的静态工具方法。
    /// </summary>
    public class RDPackageManagerWindow : GenericToolWindow
    {
        [MenuItem("RD/PackageManager/包管理集成工具/打开", false, 150)]
        public static void OpenWindow()
        {
            GetWindow<RDPackageManagerWindow>("RD包管理").Show();
        }

        [MenuItem("RD/PackageManager/包管理集成工具/关闭", false, 151)]
        public static void CloseWindow()
        {
            GetWindow<RDPackageManagerWindow>()?.Close();
        }

        //关闭后刷新资源
        public static void CloseWindowRefresh()
        {
            CloseWindow();
            AssetDatabase.SaveAssets();
            EditorApplication.ExecuteMenuItem("Assets/Refresh");
        }

        public static void UnloadAllAssets()
        {
            PackageHelper.Unload();
            PackageVersionHelper.Unload();
        }

        public static void ReLoadAllAssets()
        {
            PackageHelper.LoadAsset();
            PackageVersionHelper.LoadAsset();
        }
    }
}
