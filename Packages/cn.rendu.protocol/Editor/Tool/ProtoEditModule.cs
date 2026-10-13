//------------------------------------------------------------
// Proto 编辑(声明式模块版): VSCode 路径配置 + 合并编辑 / 保存拆分
// 对应原 ProtoViewContent 里的 "✏️ Proto 编辑" 子页签。
//------------------------------------------------------------

#if UNITY_EDITOR

using RD.EditorWindows.Editor;
using RD.Panel;
using UnityEditor;
using UnityEngine;

namespace RD.Protocol.Editor
{
    [UIAutoMenu("Proto 编辑", 200, Scope = "protocol")]
    public class ProtoEditModule : BaseToolModule
    {
        private readonly ProtoViewModel _viewModel = new ProtoViewModel();

        //---------------- VSCode 路径 ----------------

        /// <summary>候选路径由 ScanVSCodePaths 扫出来, 用 [ValueDropdown] 直接挂成下拉。</summary>
        [ShowInInspector]
        [LabelText("VSCode 路径")]
        [ValueDropdown(nameof(AvailablePaths))]
        [BoxGroup("VSCode 配置")]
        [PropertyOrder(10)]
        private string VSCodePath = "";

        /// <summary>[ValueDropdown] 的数据源(成员名被上面的特性引用)。</summary>
        private string[] AvailablePaths = new string[0];

        [Button("浏览...", 11), BoxGroup("VSCode 配置")]
        [PropertyOrder(11)]
        private void BrowseVSCode()
        {
            var selected = EditorUtility.OpenFilePanel("选择 VSCode 可执行文件", "", "exe");
            if (string.IsNullOrEmpty(selected) == false)
            {
                VSCodePath = selected;
                _viewModel.VSCodePath = selected;
            }
        }

        [ShowInInspector, DisplayAsString, LabelText("当前路径"), BoxGroup("VSCode 配置")]
        [PropertyOrder(12)]
        private string CurrentPath => string.IsNullOrEmpty(_viewModel.VSCodePath) ? "未设置" : _viewModel.VSCodePath;

        //---------------- 编辑操作 ----------------

        [Button("📝 在 VSCode 中编辑", 20), GUIColor(0.4f, 0.8f, 1), BoxGroup("编辑")]
        [PropertyOrder(20)]
        private void EditInVSCode()
        {
            ApplyPath();
            try
            {
                _viewModel.CombineAndOpenInVSCode();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"在 VSCode 中编辑失败：{e.Message}");
            }
        }

        [Button("💾 保存更改", 21), GUIColor(0.53f, 0.95f, 0.72f), BoxGroup("编辑")]
        [PropertyOrder(21)]
        private void SaveChanges()
        {
            ApplyPath();
            _viewModel.SeparateAndSave();
        }

        [ShowInInspector, LabelText("同时打开文件夹"), BoxGroup("编辑")]
        [OnValueChanged(nameof(OnOpenFolderChanged))]
        [PropertyOrder(22)]
        private bool OpenFolderEnabled;

        public override void Initialize()
        {
            AvailablePaths = _viewModel.ScanVSCodePaths();

            // 与原实现一致: 没设过时自动取扫描到的第一条
            VSCodePath = string.IsNullOrEmpty(_viewModel.VSCodePath) == false
                ? _viewModel.VSCodePath
                : (AvailablePaths.Length > 0 ? AvailablePaths[0] : "");

            if (string.IsNullOrEmpty(VSCodePath) == false)
            {
                _viewModel.VSCodePath = VSCodePath;
            }

            OpenFolderEnabled = _viewModel.OpenFolderEnabled;
        }

        private void OnOpenFolderChanged()
        {
            _viewModel.OpenFolderEnabled = OpenFolderEnabled;
        }

        private void ApplyPath()
        {
            if (string.IsNullOrEmpty(VSCodePath) == false)
            {
                _viewModel.VSCodePath = VSCodePath;
            }
        }
    }
}

#endif
