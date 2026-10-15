//------------------------------------------------------------
// Proto 列表(声明式模块版): 统计 + 操作 + 文件表
// 对应原 ProtoViewContent 里的 "📋 Proto 列表" 子页签 —— 两个子页签在这里拆成两个模块,
// 由左侧工具树切换(比"窗口里再嵌一层子页签"更贴合 GenericToolWindow 的套路)。
//------------------------------------------------------------

#if UNITY_EDITOR

using System.Collections.Generic;
using RD.EditTool.Editor;
using RD.Draw;

namespace RD.Protocol.Editor
{
    [UIAutoMenu("Proto 列表", 100, Scope = "protocol")]
    public class ProtoListModule : BaseToolModule
    {
        private readonly ProtoViewModel _viewModel = new ProtoViewModel();

        //---------------- 统计 ----------------

        [ShowInInspector, DisplayAsString, LabelText("总计"), BoxGroup("📊 统计信息"), PropertyOrder(-100)]
        private int TotalCount;

        [ShowInInspector, DisplayAsString, LabelText("状态"), BoxGroup("📊 统计信息"), PropertyOrder(-99)]
        private string State;

        [ShowInInspector, DisplayAsString, LabelText("格式错误"), BoxGroup("📊 统计信息"), PropertyOrder(-98)]
        private int FormatErrorCount;

        [ShowInInspector, DisplayAsString, LabelText("数值重复"), BoxGroup("📊 统计信息"), PropertyOrder(-97)]
        private int DuplicateCount;

        [ShowInInspector, DisplayAsString, LabelText("解析失败"), BoxGroup("📊 统计信息"), PropertyOrder(-96)]
        private int ParseErrorCount;

        //---------------- 操作 ----------------

        [Button("🔄 刷新", 10), BoxGroup("🔧 操作")]
        [PropertyOrder(10)]
        private void Refresh()
        {
            SyncList();
        }

        /// <summary>按编号排序: 改完立刻重排(与原 Toggle 的 EndChangeCheck 行为一致)。</summary>
        [ShowInInspector, LabelText("按编号排序"), BoxGroup("🔧 操作")]
        [OnValueChanged(nameof(OnSortChanged))]
        [PropertyOrder(11)]
        private bool SortEnabled;

        [Button("⚙️ 协议导出", 12), BoxGroup("🔧 操作")]
        [PropertyOrder(12)]
        private void ExportProtocol()
        {
            _viewModel.ExecuteProtocolExport();
        }

        //---------------- 文件列表 ----------------

        [ShowInInspector]
        [TableList]
        [LabelText("📋 Proto 文件列表")]
        [ListDrawerSettings(IsReadOnly = true, HideAddButton = true, HideRemoveButton = true,
                            ShowItemCount = true, NumberOfItemsPerPage = 20, PagingHasValue = true)]
        [PropertyOrder(20)]
        private List<ProtoRow> Files = new List<ProtoRow>();

        public override void Initialize()
        {
            SortEnabled = _viewModel.SortEnabled;
            SyncList();
        }

        private void OnSortChanged()
        {
            _viewModel.SortEnabled = SortEnabled;
            SyncList();
        }

        private void SyncList()
        {
            _viewModel.RefreshProtoList();

            Files.Clear();
            TotalCount       = 0;
            FormatErrorCount = 0;
            DuplicateCount   = 0;
            ParseErrorCount  = 0;

            foreach (var item in _viewModel.ProtoItems)
            {
                Files.Add(ProtoRow.From(item));

                TotalCount++;
                if (item.IsFormatError)    FormatErrorCount++;
                if (item.IsDuplicateError) DuplicateCount++;
                if (item.IsParseError)     ParseErrorCount++;
            }

            var errors = FormatErrorCount + DuplicateCount + ParseErrorCount;
            State = errors > 0 ? "❌ 有错误" : "✅ 正常";
        }
    }
}

#endif
