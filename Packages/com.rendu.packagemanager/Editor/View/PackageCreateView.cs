using System;
using System.Collections.Generic;
using System.Linq;
using RD.Init.Editor;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace RD.PackageManager.Editor
{
    /// <summary>
    /// 创建/更新包对话框视图
    /// </summary>
    public class PackageCreateView : EditorWindow
    {
        private VisualElement _root;
        private PackageViewModel _viewModel;
        private PackageInputChangedHandler _inputChangedHandler;

        // View 状态
        private bool _requiredFoldout = true;
        private bool _optionalFoldout = true;
        private bool _forceUpdate = false;

        public static void OpenWindow()
        {
            var window = GetWindow<PackageCreateView>("创建 Rendu Package", true, WindowsDefine.DockedWindowTypes);
            if (window != null)
            {
                window.Initialize();
                window.Show();
            }
        }

        private void Initialize()
        {
            // 创建新的 ViewModel,会自动从 JSON 加载默认值
            _viewModel = new PackageViewModel();

            // 使用当前 Unity 版本(仅当比默认版本新时)
            string currentUnityVersion = FormatUnityVersion(Application.unityVersion);
            string defaultUnityVersion = _viewModel.Info.Required.Unity;
            if (CompareUnityVersions(currentUnityVersion, defaultUnityVersion) > 0)
            {
                _viewModel.Unity = currentUnityVersion;
            }

            _inputChangedHandler = new PackageInputChangedHandler(_viewModel);
            _inputChangedHandler.OnDataLoaded += OnDataLoaded;
        }

        /// <summary>
        /// 格式化 Unity 版本(去掉 't' 后缀,只保留主版本号)
        /// </summary>
        private string FormatUnityVersion(string version)
        {
            if (string.IsNullOrEmpty(version)) return version;

            // 去掉 't' 开头的后缀
            int tIndex = version.IndexOf('t');
            string formatted = tIndex > 0 ? version.Substring(0, tIndex) : version;

            // 只保留主版本号 (如 "2022.3")
            string[] parts = formatted.Split('.');
            if (parts.Length >= 2)
            {
                return $"{parts[0]}.{parts[1]}";
            }
            return formatted;
        }

        /// <summary>
        /// 比较两个 Unity 版本
        /// 返回: 1 (v1 > v2), 0 (v1 == v2), -1 (v1 < v2)
        /// </summary>
        private int CompareUnityVersions(string v1, string v2)
        {
            if (string.IsNullOrEmpty(v1)) return -1;
            if (string.IsNullOrEmpty(v2)) return 1;

            string[] p1 = v1.Split('.');
            string[] p2 = v2.Split('.');

            int maxLength = Mathf.Max(p1.Length, p2.Length);
            for (int i = 0; i < maxLength; i++)
            {
                int num1 = i < p1.Length && int.TryParse(p1[i], out int n1) ? n1 : 0;
                int num2 = i < p2.Length && int.TryParse(p2[i], out int n2) ? n2 : 0;

                if (num1 > num2) return 1;
                if (num1 < num2) return -1;
            }
            return 0;
        }

        private VisualElement CreateFlagsEnumField<T>(string label, T currentValue, System.Action<System.Enum> onChanged) where T : System.Enum
        {
            // 使用 RD.Init 包中的 FlagsEnumField 控件
            var field = new RD.Init.Editor.FlagsEnumField(label, typeof(T));
            field.Value = currentValue;
            field.RegisterValueChangedCallback(onChanged);
            return field;
        }

        private void UpdateFlagsEnumField<T>(string fieldName, T currentValue) where T : System.Enum
        {
            var field = _root.Q<RD.Init.Editor.FlagsEnumField>(fieldName);
            if (field == null)
            {
                Debug.LogWarning($"[UpdateFlagsEnumField] Field not found: {fieldName}");
                return;
            }

            // 使用 FlagsEnumField 的 SetValueWithoutNotify 方法更新显示
            field.SetValueWithoutNotify(currentValue);
        }

        private void OnEnable()
        {
            minSize = new Vector2(600, 900);
        }

        private void CreateGUI()
        {
            var visualTree = UxmlLoader.LoadWindowUXML<PackageCreateView>();
            if (visualTree == null)
            {
                Debug.LogError($"无法加载 UXML 文件: PackageCreateView");
                return;
            }

            _root = visualTree.CloneTree();
            rootVisualElement.Add(_root);

            // 每次打开窗口时都重新初始化数据（在 SetupVisualElements 之前）
            Initialize();

            SetupVisualElements();
            BindEvents();

            UpdateUIFromViewModel();
        }

        private void SetupVisualElements()
        {
            var simpleNameField = _root.Q<TextField>("simpleNameField");
            if (simpleNameField != null)
            {
                simpleNameField.RegisterValueChangedCallback(evt =>
                {
                    _inputChangedHandler.HandleInputChanged(evt.newValue);
                    // 更新相关字段的 UI 显示
                    UpdateRelatedFieldsUI();
                    UpdateSummary();
                    UpdateForceUpdateInfo();
                });
            }

            var formalNameField = _root.Q<TextField>("formalNameField");
            if (formalNameField != null)
            {
                formalNameField.RegisterValueChangedCallback(evt =>
                {
                    _inputChangedHandler.HandleFormalNameChanged(evt.newValue);
                    UpdateSummary();
                    UpdateForceUpdateInfo();
                });
            }

            var displayNameField = _root.Q<TextField>("displayNameField");
            if (displayNameField != null)
            {
                displayNameField.RegisterValueChangedCallback(evt =>
                {
                    _viewModel.DisplayName = evt.newValue;
                    UpdateSummary();
                });
            }

            var versionField = _root.Q<TextField>("versionField");
            if (versionField != null)
            {
                versionField.RegisterValueChangedCallback(evt =>
                {
                    _viewModel.Version = evt.newValue;
                    UpdateSummary();
                });
            }

            var unityVersionField = _root.Q<TextField>("unityVersionField");
            if (unityVersionField != null)
            {
                unityVersionField.RegisterValueChangedCallback(evt =>
                {
                    _viewModel.Unity = evt.newValue;
                    UpdateSummary();
                });
            }

            // 创建多选枚举字段（因为 UXML 不支持在 XML 中直接引用自定义枚举类型）
            var asmDefTypeContainer = _root.Q<VisualElement>("asmDefTypeContainer");
            if (asmDefTypeContainer != null)
            {
                var asmDefTypeField = CreateFlagsEnumField("Assembly定义类型", _viewModel.AsmDefType,
                    newValue =>
                    {
                        _viewModel.AsmDefType = (AssemblyDefineType)newValue;
                        UpdateSummary();
                    });
                asmDefTypeField.name = "asmDefTypeField";
                asmDefTypeContainer.Add(asmDefTypeField);
            }

            var asmRefTypeContainer = _root.Q<VisualElement>("asmRefTypeContainer");
            if (asmRefTypeContainer != null)
            {
                var asmRefTypeField = CreateFlagsEnumField("Assembly引用类型", _viewModel.AsmRefType,
                    newValue =>
                    {
                        _viewModel.AsmRefType = (AssemblyReferenceType)newValue;
                        UpdateSummary();
                    });
                asmRefTypeField.name = "asmRefTypeField";
                asmRefTypeContainer.Add(asmRefTypeField);
            }

            var codeModeContainer = _root.Q<VisualElement>("codeModeContainer");
            if (codeModeContainer != null)
            {
                var codeModeField = CreateFlagsEnumField("CodeMode类型", _viewModel.CodeMode,
                    newValue =>
                    {
                        _viewModel.CodeMode = (CodeModeType)newValue;
                        UpdateSummary();
                    });
                codeModeField.name = "codeModeField";
                codeModeContainer.Add(codeModeField);
            }

            var ignoreAsmdefToggle = _root.Q<Toggle>("ignoreAsmdefToggle");
            if (ignoreAsmdefToggle != null)
            {
                ignoreAsmdefToggle.RegisterValueChangedCallback(evt =>
                {
                    _viewModel.GenerateIgnoreAsmdef = evt.newValue;
                    UpdateSummary();
                });
            }

            var descriptionField = _root.Q<TextField>("descriptionField");
            if (descriptionField != null)
            {
                descriptionField.RegisterValueChangedCallback(evt =>
                {
                    _viewModel.Description = evt.newValue;
                    UpdateSummary();
                });
            }

            var authorNameField = _root.Q<TextField>("authorNameField");
            if (authorNameField != null)
            {
                authorNameField.RegisterValueChangedCallback(evt =>
                {
                    _viewModel.AuthorName = evt.newValue;
                    UpdateSummary();
                });
            }

            var authorEmailField = _root.Q<TextField>("authorEmailField");
            if (authorEmailField != null)
            {
                authorEmailField.RegisterValueChangedCallback(evt =>
                {
                    _viewModel.AuthorEmail = evt.newValue;
                    UpdateSummary();
                });
            }

            var authorUrlField = _root.Q<TextField>("authorUrlField");
            if (authorUrlField != null)
            {
                authorUrlField.RegisterValueChangedCallback(evt =>
                {
                    _viewModel.AuthorUrl = evt.newValue;
                    UpdateSummary();
                });
            }

            var keywordsField = _root.Q<TextField>("keywordsField");
            if (keywordsField != null)
            {
                keywordsField.RegisterValueChangedCallback(evt =>
                {
                    _viewModel.Keywords = evt.newValue;
                    UpdateSummary();
                });
            }

            UpdateFoldoutVisual("requiredFoldoutButton", "requiredContent", _requiredFoldout);
            UpdateFoldoutVisual("optionalFoldoutButton", "optionalContent", _optionalFoldout);
        }

        private void BindEvents()
        {
            var forceUpdateToggle = _root.Q<Toggle>("forceUpdateToggle");
            if (forceUpdateToggle != null)
            {
                forceUpdateToggle.RegisterValueChangedCallback(evt =>
                {
                    _forceUpdate = evt.newValue;
                    UpdateForceUpdateInfo();
                });
            }

            var requiredFoldoutButton = _root.Q<Button>("requiredFoldoutButton");
            if (requiredFoldoutButton != null)
            {
                requiredFoldoutButton.clicked += () =>
                {
                    _requiredFoldout = !_requiredFoldout;
                    UpdateFoldoutVisual("requiredFoldoutButton", "requiredContent", _requiredFoldout);
                };
            }

            var optionalFoldoutButton = _root.Q<Button>("optionalFoldoutButton");
            if (optionalFoldoutButton != null)
            {
                optionalFoldoutButton.clicked += () =>
                {
                    _optionalFoldout = !_optionalFoldout;
                    UpdateFoldoutVisual("optionalFoldoutButton", "optionalContent", _optionalFoldout);
                };
            }

            var cancelButton = _root.Q<Button>("cancelButton");
            if (cancelButton != null)
            {
                cancelButton.clicked += Close;
            }

            var createButton = _root.Q<Button>("createButton");
            if (createButton != null)
            {
                createButton.clicked += () =>
                {
                    if (TryCreateOrUpdatePackage())
                    {
                        Close();
                    }
                };
            }
        }

        private void OnDataLoaded()
        {
            // 数据加载完成后，更新所有相关字段的 UI 显示
            UpdateRelatedFieldsUI();
            UpdateSummary();
            UpdateForceUpdateInfo();
        }

        private void UpdateRelatedFieldsUI()
        {
            Debug.Log($"[UpdateRelatedFieldsUI] Updating UI - AsmDefType: {_viewModel.AsmDefType}, AsmRefType: {_viewModel.AsmRefType}, CodeMode: {_viewModel.CodeMode}");
            
            // 更新 SimpleName、FormalName 和 DisplayName 字段的 UI 显示
            var simpleNameField = _root.Q<TextField>("simpleNameField");
            if (simpleNameField != null) simpleNameField.value = _viewModel.SimpleName;

            var formalNameField = _root.Q<TextField>("formalNameField");
            if (formalNameField != null) formalNameField.value = _viewModel.FormalName;

            var displayNameField = _root.Q<TextField>("displayNameField");
            if (displayNameField != null) displayNameField.value = _viewModel.DisplayName;

            // 更新枚举字段
            UpdateFlagsEnumField("asmDefTypeField", _viewModel.AsmDefType);
            UpdateFlagsEnumField("asmRefTypeField", _viewModel.AsmRefType);
            UpdateFlagsEnumField("codeModeField", _viewModel.CodeMode);

            var ignoreAsmdefToggle = _root.Q<Toggle>("ignoreAsmdefToggle");
            if (ignoreAsmdefToggle != null) ignoreAsmdefToggle.value = _viewModel.GenerateIgnoreAsmdef;

            // 更新其他字段
            var versionField = _root.Q<TextField>("versionField");
            if (versionField != null) versionField.value = _viewModel.Version;

            var unityVersionField = _root.Q<TextField>("unityVersionField");
            if (unityVersionField != null) unityVersionField.value = _viewModel.Unity;

            var descriptionField = _root.Q<TextField>("descriptionField");
            if (descriptionField != null) descriptionField.value = _viewModel.Description;

            var authorNameField = _root.Q<TextField>("authorNameField");
            if (authorNameField != null) authorNameField.value = _viewModel.AuthorName;

            var authorEmailField = _root.Q<TextField>("authorEmailField");
            if (authorEmailField != null) authorEmailField.value = _viewModel.AuthorEmail;

            var authorUrlField = _root.Q<TextField>("authorUrlField");
            if (authorUrlField != null) authorUrlField.value = _viewModel.AuthorUrl;

            var keywordsField = _root.Q<TextField>("keywordsField");
            if (keywordsField != null) keywordsField.value = _viewModel.Keywords;
        }

        private void UpdateUIFromViewModel()
        {
            // 使用 UpdateRelatedFieldsUI 来统一更新所有字段
            UpdateRelatedFieldsUI();
            UpdateSummary();
            UpdateForceUpdateInfo();
        }

        private void UpdateFoldoutVisual(string buttonName, string contentName, bool isExpanded)
        {
            var button = _root.Q<Button>(buttonName);
            if (button != null) button.text = isExpanded ? "▼" : "▶";

            var content = _root.Q<VisualElement>(contentName);
            if (content != null) content.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void UpdateSummary()
        {
            var summaryFormalName = _root.Q<Label>("summaryFormalName");
            var summaryDisplayName = _root.Q<Label>("summaryDisplayName");
            var summaryVersion = _root.Q<Label>("summaryVersion");
            var summaryUnity = _root.Q<Label>("summaryUnity");
            var summaryDescription = _root.Q<Label>("summaryDescription");
            var summaryAsmDef = _root.Q<Label>("summaryAsmDef");
            var summaryAsmRef = _root.Q<Label>("summaryAsmRef");
            var summaryCodeMode = _root.Q<Label>("summaryCodeMode");
            var summaryIgnoreAsmdef = _root.Q<Label>("summaryIgnoreAsmdef");
            var summaryAuthor = _root.Q<Label>("summaryAuthor");
            var summaryAuthorEmail = _root.Q<Label>("summaryAuthorEmail");
            var summaryAuthorUrl = _root.Q<Label>("summaryAuthorUrl");
            var summaryKeywords = _root.Q<Label>("summaryKeywords");

            if (summaryFormalName != null) summaryFormalName.text = $"正式名称: {_viewModel.FormalName}";
            if (summaryDisplayName != null) summaryDisplayName.text = $"显示名称: {_viewModel.DisplayName}";
            if (summaryVersion != null) summaryVersion.text = $"版本: {_viewModel.Version}";
            if (summaryUnity != null) summaryUnity.text = $"Unity版本: {_viewModel.Unity}";
            if (summaryDescription != null) summaryDescription.text = $"描述: {_viewModel.Description}";
            if (summaryAsmDef != null) summaryAsmDef.text = $"Assembly定义类型: {_viewModel.AsmDefType}";
            if (summaryAsmRef != null) summaryAsmRef.text = $"Assembly引用类型: {_viewModel.AsmRefType}";
            if (summaryCodeMode != null) summaryCodeMode.text = $"CodeMode类型: {_viewModel.CodeMode}";
            if (summaryIgnoreAsmdef != null) summaryIgnoreAsmdef.text = $"生成 Ignore Asmdef: {_viewModel.GenerateIgnoreAsmdef}";
            if (summaryAuthor != null) summaryAuthor.text = $"作者: {_viewModel.AuthorName}";
            if (summaryAuthorEmail != null) summaryAuthorEmail.text = $"作者邮箱: {_viewModel.AuthorEmail}";
            if (summaryAuthorUrl != null) summaryAuthorUrl.text = $"作者网址: {_viewModel.AuthorUrl}";
            if (summaryKeywords != null) summaryKeywords.text = $"关键词: {_viewModel.Keywords}";
        }

        private void UpdateForceUpdateInfo()
        {
            var infoBox = _root.Q<VisualElement>("forceUpdateInfoBox");
            if (infoBox == null) return;

            infoBox.Clear();

            string packagePath = $"Packages/{_viewModel.FormalName}";
            bool packageExists = !string.IsNullOrEmpty(_viewModel.FormalName) && System.IO.Directory.Exists(packagePath);

            HelpBox helpBox;
            if (_forceUpdate)
            {
                string updateMessage = "更新模式: 将保留已有配置，仅更新 package.json 中的基本信息\n\n" +
                                       "注意事项:\n" +
                                       "- package.json: 增量更新(保留已有配置)\n" +
                                       "- .asmdef 文件: 跳过更新(保留引用关系)\n" +
                                       "- packagegit.json: 跳过更新(保留已有配置)\n" +
                                       "- 目录结构: 跳过已有目录(不删除文件)";
                helpBox = new HelpBox(updateMessage, HelpBoxMessageType.Info);
            }
            else if (packageExists)
            {
                helpBox = new HelpBox($"包 '{_viewModel.FormalName}' 已存在，启用强制更新以进行增量更新", HelpBoxMessageType.Warning);
            }
            else
            {
                helpBox = new HelpBox("创建模式: 将创建新的包结构", HelpBoxMessageType.Info);
            }

            infoBox.Add(helpBox);
        }

        private bool TryCreateOrUpdatePackage()
        {
            // 验证输入
            var validationResult = _viewModel.ValidateCreatePackage();
            if (!validationResult.IsValid)
            {
                UnityTipsHelper.ShowError(validationResult.ErrorMessage);
                return false;
            }

            string packagePath = $"Packages/{_viewModel.FormalName}";
            bool packageExists = System.IO.Directory.Exists(packagePath);

            // 检查是否需要强制更新
            if (packageExists && !_forceUpdate)
            {
                UnityTipsHelper.ShowError($"Package '{_viewModel.FormalName}' already exists.\n\n请启用'强制更新'选项来覆盖现有包。");
                return false;
            }

            // 确认操作
            string actionText = _forceUpdate ? "更新" : "创建";
            bool shouldProceed = true;
            UnityTipsHelper.CallBack($"确定要{actionText}包 '{_viewModel.FormalName}' 吗?",
                () => { },
                () => { shouldProceed = false; });
            if (!shouldProceed) return false;

            PackageCreateInfo createPackageInfo = new PackageCreateInfo(_viewModel.Info);
            createPackageInfo.PackagePath = packagePath;
            createPackageInfo.PackageName = _viewModel.SimpleName;

            Debug.Log($"[PackageCreateView] Creating package - CreateType: {_viewModel.AsmDefType}, CreateFolderType: {_viewModel.CodeMode}, RuntimeRefType: {_viewModel.AsmRefType}");

            // 创建或更新包
            PackageCreateHelper.CreatePackage(createPackageInfo, force: _forceUpdate);
            return true;
        }
    }
}