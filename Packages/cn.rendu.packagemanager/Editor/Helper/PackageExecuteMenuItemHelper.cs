using System;
using UnityEditor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    //为了防止其他包的按钮可能改名字等操作 这里需要使用前深度检查
    //因为这个管理包 不可能每次其他包更新都去检查一下 万一哪天没检查到不就BUG了
    //所以这哪里必须要做好防护措施
    public static class PackageExecuteMenuItemHelper
    {
        public static void RDAll()
        {
            //有顺序的 别乱动
            RD_Init_RepairDependencies();
            RD_Loader_ReGenerateProjectFiles();
            RD_Loader_ReGenerateProjectAssemblyReference();
            RD_Loader_UpdateScriptsReferences();
            RD_Excel_ExcelExporter();
            RD_Proto_Proto2CS();
        }

        public static void RD_Init_RepairDependencies()
        {
            ExecuteMenuItem("RD/Init/RepairDependencies");
        }

        public static void RD_Loader_ReGenerateProjectFiles()
        {
            ExecuteMenuItem("RD/Loader/ReGenerateProjectFiles");
        }

        public static void RD_Loader_ReGenerateProjectAssemblyReference()
        {
            ExecuteMenuItem("RD/Loader/ReGenerateProjectAssemblyReference");
        }

        public static void RD_Loader_UpdateScriptsReferences()
        {
            ExecuteMenuItem("RD/Loader/UpdateScriptsReferences");
        }

        public static void RD_Excel_ExcelExporter()
        {
            //关闭此功能 改为手动执行
            //ExecuteMenuItem("RD/Excel/ExcelExporter");
        }

        public static void RD_Proto_Proto2CS()
        {
            ExecuteMenuItem("RD/Proto/Proto2CS");
        }

        public static void ExecuteMenuItem(string menuItem)
        {
            try
            {
                EditorApplication.ExecuteMenuItem(menuItem);
            }
            catch (Exception e)
            {
                Debug.LogError($"执行错误 {menuItem} 请检查 {e.Message}");
            }
        }
    }
}