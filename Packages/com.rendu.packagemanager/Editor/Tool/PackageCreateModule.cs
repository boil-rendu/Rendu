//------------------------------------------------------------
// 创建 / 更新 Rendu Package(声明式模块版)
// 对应原 PackageCreateView(UXML 自绘对话框): 必填 / 可选 / 摘要 / 强制更新 / 创建按钮,
// 界面全靠特性描述, 由 cn.rendu.panel 引擎绘制; 三个 [Flags] 枚举走引擎的多选控件。
//------------------------------------------------------------

#if UNITY_EDITOR

using RD.EditTool.Editor;
using RD.Init.Editor;
using RD.Panel;
using UnityEditor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    [UIAutoMenu("创建 Rendu Package", 100, Scope = "packagemanager")]
    public class PackageCreateModule : BaseToolModule
    {
        private PackageViewModel _viewModel;
        private PackageInputChangedHandler _inputChangedHandler;

        //---------------- 必填信息 ----------------

        [ShowInInspector, BoxGroup("必填信息"), LabelText("包名(简写)"), PropertyOrder(10)]
        [OnValueChanged(nameof(OnSimpleNameChanged))]
        public string SimpleName
        {
            get => _viewModel.SimpleName;
            set => _viewModel.SimpleName = value;
        }

        [ShowInInspector, BoxGroup("必填信息"), LabelText("正式包名"), ReadOnly, PropertyOrder(11)]
        public string FormalName
        {
            get => _viewModel.FormalName;
            set => _viewModel.FormalName = value;
        }

        [ShowInInspector, BoxGroup("必填信息"), LabelText("显示名"), ReadOnly, PropertyOrder(12)]
        public string DisplayName
        {
            get => _viewModel.DisplayName;
            set => _viewModel.DisplayName = value;
        }

        [ShowInInspector, BoxGroup("必填信息"), LabelText("版本"), PropertyOrder(13)]
        public string Version
        {
            get => _viewModel.Version;
            set => _viewModel.Version = value;
        }

        [ShowInInspector, BoxGroup("必填信息"), LabelText("Unity 版本"), PropertyOrder(14)]
        public string Unity
        {
            get => _viewModel.Unity;
            set => _viewModel.Unity = value;
        }

        [ShowInInspector, BoxGroup("必填信息"), LabelText("Assembly 定义类型"), PropertyOrder(15)]
        public AssemblyDefineType AsmDefType
        {
            get => _viewModel.AsmDefType;
            set => _viewModel.AsmDefType = value;
        }

        [ShowInInspector, BoxGroup("必填信息"), LabelText("Assembly 引用类型"), PropertyOrder(16)]
        public AssemblyReferenceType AsmRefType
        {
            get => _viewModel.AsmRefType;
            set => _viewModel.AsmRefType = value;
        }

        [ShowInInspector, BoxGroup("必填信息"), LabelText("代码模式"), PropertyOrder(17)]
        public CodeModeType CodeMode
        {
            get => _viewModel.CodeMode;
            set => _viewModel.CodeMode = value;
        }

        [ShowInInspector, BoxGroup("必填信息"), LabelText("生成 Ignore Asmdef"), PropertyOrder(18)]
        public bool GenerateIgnoreAsmdef
        {
            get => _viewModel.GenerateIgnoreAsmdef;
            set => _viewModel.GenerateIgnoreAsmdef = value;
        }

        //---------------- 可选信息 ----------------

        [ShowInInspector, BoxGroup("可选信息"), LabelText("描述"), PropertyOrder(30)]
        public string Description
        {
            get => _viewModel.Description;
            set => _viewModel.Description = value;
        }

        [ShowInInspector, BoxGroup("可选信息"), LabelText("作者"), PropertyOrder(31)]
        public string AuthorName
        {
            get => _viewModel.AuthorName;
            set => _viewModel.AuthorName = value;
        }

        [ShowInInspector, BoxGroup("可选信息"), LabelText("邮箱"), PropertyOrder(32)]
        public string AuthorEmail
        {
            get => _viewModel.AuthorEmail;
            set => _viewModel.AuthorEmail = value;
        }

        [ShowInInspector, BoxGroup("可选信息"), LabelText("主页"), PropertyOrder(33)]
        public string AuthorUrl
        {
            get => _viewModel.AuthorUrl;
            set => _viewModel.AuthorUrl = value;
        }

        [ShowInInspector, BoxGroup("可选信息"), LabelText("关键字"), PropertyOrder(34)]
        public string Keywords
        {
            get => _viewModel.Keywords;
            set => _viewModel.Keywords = value;
        }

        //---------------- 摘要 ----------------

        [ShowInInspector, DisplayAsString, BoxGroup("摘要"), LabelText("正式包名"), PropertyOrder(50)]
        private string SummaryFormalName => _viewModel.FormalName;

        [ShowInInspector, DisplayAsString, BoxGroup("摘要"), LabelText("显示名"), PropertyOrder(51)]
        private string SummaryDisplayName => _viewModel.DisplayName;

        [ShowInInspector, DisplayAsString, BoxGroup("摘要"), LabelText("版本 / Unity"), PropertyOrder(52)]
        private string SummaryVersion => $"{_viewModel.Version} / {_viewModel.Unity}";

        //---------------- 操作 ----------------

        [ShowInInspector, LabelText("强制更新(覆盖已存在的包)"), PropertyOrder(60)]
        public bool ForceUpdate;

        [Button("📦 创建 / 更新包", 61), GUIColor(0.53f, 0.95f, 0.72f), PropertyOrder(61)]
        private void CreateOrUpdate()
        {
            var validation = _viewModel.ValidateCreatePackage();
            if (validation.IsValid == false)
            {
                UnityTipsHelper.ShowError(validation.ErrorMessage);
                return;
            }

            var packagePath = $"Packages/{_viewModel.FormalName}";
            var packageExists = System.IO.Directory.Exists(packagePath);

            if (packageExists && ForceUpdate == false)
            {
                UnityTipsHelper.ShowError(
                    $"Package '{_viewModel.FormalName}' already exists.\n\n请启用'强制更新'选项来覆盖现有包。");
                return;
            }

            var shouldProceed = true;
            UnityTipsHelper.CallBack($"确定要{(ForceUpdate ? "更新" : "创建")}包 '{_viewModel.FormalName}' 吗?",
                () => { }, () => { shouldProceed = false; });
            if (shouldProceed == false) return;

            Debug.Log($"[PackageCreateModule] Creating package - AsmDefType: {_viewModel.AsmDefType}, "
                    + $"CodeMode: {_viewModel.CodeMode}, AsmRefType: {_viewModel.AsmRefType}");

            var createInfo = new PackageCreateInfo(_viewModel.Info)
            {
                PackagePath = packagePath,
                PackageName = _viewModel.SimpleName,
            };

            PackageCreateHelper.CreatePackage(createInfo, force: ForceUpdate);
        }

        /// <summary>放弃当前填写 / 建完回到初始态(替代原对话框的"取消 = 关窗口")。</summary>
        [Button("↺ 重置", 62), PropertyOrder(62)]
        private void ResetForm()
        {
            ResetState();
        }

        //---------------- 生命周期 ----------------

        public override void Initialize()
        {
            ResetState();
        }

        private void ResetState()
        {
            _viewModel = new PackageViewModel();

            // 与原来一样: 当前 Unity 版本比默认版本新时, 用当前版本
            var current = FormatUnityVersion(Application.unityVersion);
            if (CompareUnityVersions(current, _viewModel.Info.Required.Unity) > 0)
            {
                _viewModel.Unity = current;
            }

            _inputChangedHandler = new PackageInputChangedHandler(_viewModel);
            _inputChangedHandler.OnDataLoaded += OnDataLoaded;

            ForceUpdate = false;
        }

        /// <summary>包名变化: 交给原处理器(它有防抖, 并在包已存在时载入其信息)。</summary>
        private void OnSimpleNameChanged()
        {
            _inputChangedHandler?.HandleInputChanged(SimpleName);
        }

        private void OnDataLoaded()
        {
            Window?.Repaint();   // 载入已有包信息后立刻重画, 让联动字段同步显示
        }

        private static string FormatUnityVersion(string version)
        {
            if (string.IsNullOrEmpty(version)) return version;

            var tIndex = version.IndexOf('t');
            var formatted = tIndex > 0 ? version.Substring(0, tIndex) : version;

            var parts = formatted.Split('.');
            return parts.Length >= 2 ? $"{parts[0]}.{parts[1]}" : formatted;
        }

        private static int CompareUnityVersions(string v1, string v2)
        {
            if (string.IsNullOrEmpty(v1)) return -1;
            if (string.IsNullOrEmpty(v2)) return 1;

            var p1 = v1.Split('.');
            var p2 = v2.Split('.');

            var maxLength = Mathf.Max(p1.Length, p2.Length);
            for (var i = 0; i < maxLength; i++)
            {
                var n1 = i < p1.Length && int.TryParse(p1[i], out var a) ? a : 0;
                var n2 = i < p2.Length && int.TryParse(p2[i], out var b) ? b : 0;

                if (n1 > n2) return 1;
                if (n1 < n2) return -1;
            }

            return 0;
        }
    }
}

#endif
