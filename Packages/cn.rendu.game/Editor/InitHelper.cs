using RD.Excel.Editor;
using RD.Loader.Editor;
using RD.Protocol.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.Game.Editor
{
    public static class InitHelper
    {
        public static void Init()
        {
            Debug.Log("========== Game Init Start ==========");
            
            Debug.Log("[1/5] 初始化 HybridCLR...");
            HybridCLREditor.Init();
            Debug.Log("[1/5] HybridCLR 初始化完成");
            
            Debug.Log("[2/5] 导出 Excel...");
            ExcelViewModel.ExecuteExcelExportStatic();
            Debug.Log("[2/5] Excel 导出完成");
            
            Debug.Log("[3/5] 导出 Protocol...");
            ProtoViewModel.ExecuteProtocolExportStatic();
            Debug.Log("[3/5] Protocol 导出完成");
            
            Debug.Log("[4/5] 初始化项目...");
            new SetSceneNameCommand("StateSync").Execute();
            new LinkSlnCommand().Execute();
            new UpdateScriptsReferencesCommand().Execute();
            new ChangeCodeModeCommand("ClientServer").Execute();
            new RunInitScriptsCommand().Execute();
            new EnableDefineCommand("INITED").Execute();
            Debug.Log("[4/5] 项目初始化完成");
            
            Debug.Log("[5/5] 刷新资源数据库...");
            AssetDatabase.Refresh();
            Debug.Log("[5/5] 资源数据库刷新完成");
            
            Debug.Log("========== Game Init Finish ==========");
        }
    }
}