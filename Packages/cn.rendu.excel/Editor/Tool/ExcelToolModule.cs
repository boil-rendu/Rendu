#if UNITY_EDITOR

using System.Collections.Generic;
using RD.EditorWindows.Editor;
using RD.Panel;
using UnityEngine;

namespace RD.Excel.Editor
{
    /// <summary>
    /// Excel 工具模块: 与原 ExcelViewContent(UXML 自绘)等价的界面, 但改成声明式 ——
    /// 不写 UXML、不手写 Q&lt;T&gt;() 取控件、不手绑事件, 界面全靠特性描述, 由 cn.rendu.panel 引擎绘制。
    ///
    /// 挂 [UIAutoMenu(Scope = "excel")] 后被 <see cref="ExcelTool"/> 反射收进左侧工具树,
    /// 与 LoaderTool / UITool 的模块完全同一套套路。
    /// </summary>
    [UIAutoMenu("Excel 表格", 100, Scope = "excel")]
    public class ExcelToolModule : BaseToolModule
    {
        private readonly ExcelViewModel _viewModel = new ExcelViewModel();

        //---------------- 概览 ----------------

        [ShowInInspector]
        [DisplayAsString]
        [LabelText("总计")]
        [BoxGroup("概览", centerLabel: true)]
        [PropertyOrder(-100)]
        private string Summary => $"{Files.Count} 个 Excel 文件";

        [Button("刷新列表", -99), GUIColor(0.4f, 0.8f, 1)]
        [BoxGroup("概览", centerLabel: true)]
        private void Refresh()
        {
            SyncFiles();
        }

        [Button("导出 Excel", -98), GUIColor(0.53f, 0.95f, 0.72f)]
        [BoxGroup("概览", centerLabel: true)]
        private void Export()
        {
            ExcelViewModel.ExecuteExcelExportStatic();
        }

        //---------------- 文件列表 ----------------

        [ShowInInspector]
        [TableList]
        [LabelText("Excel 文件")]
        [ListDrawerSettings(IsReadOnly = true, HideAddButton = true, HideRemoveButton = true,
                            ShowItemCount = true, NumberOfItemsPerPage = 20, PagingHasValue = true)]
        [PropertyOrder(0)]
        private List<ExcelFileRow> Files = new List<ExcelFileRow>();

        public override void Initialize()
        {
            SyncFiles();
        }

        /// <summary>ViewModel 扫盘 → 行模型(列表数据源)。</summary>
        private void SyncFiles()
        {
            _viewModel.RefreshExcelList();

            Files.Clear();
            foreach (var item in _viewModel.ExcelItems)
            {
                Files.Add(ExcelFileRow.From(item));
            }
        }
    }
}

#endif
