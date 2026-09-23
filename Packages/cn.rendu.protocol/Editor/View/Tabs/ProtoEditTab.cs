using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.Protocol.Editor
{
    /// <summary>
    /// Proto 编辑页签
    /// </summary>
    public class ProtoEditTab : TabBase
    {
        public override string Label => "✏️ Proto 编辑";

        private ProtoViewModel _viewModel;
        private string _newVSCodePath = "";
        private string[] _availableVSCodePaths = new string[0];
        private int _selectedVSCodeIndex = -1;

        public ProtoEditTab(ProtoViewModel viewModel)
        {
            _viewModel = viewModel;
            _availableVSCodePaths = _viewModel.ScanVSCodePaths();

            if (string.IsNullOrEmpty(_viewModel.VSCodePath) && _availableVSCodePaths.Length > 0)
            {
                _viewModel.VSCodePath = _availableVSCodePaths[0];
                _selectedVSCodeIndex = 0;
            }
            else if (_availableVSCodePaths.Length > 0)
            {
                for (int i = 0; i < _availableVSCodePaths.Length; i++)
                {
                    if (_availableVSCodePaths[i] == _viewModel.VSCodePath)
                    {
                        _selectedVSCodeIndex = i;
                        break;
                    }
                }
            }
        }

        public override VisualElement CreateVisualElement()
        {
            var container = new VisualElement();
            container.style.paddingTop = 10;
            container.style.paddingBottom = 10;

            container.Add(CreateSectionHeader(Label));
            container.Add(DrawVSCodePathConfigUI());
            container.Add(DrawEditOptionsUI());
            container.Add(DrawVSCodeInfoUI());

            return container;
        }

        public override void OnGUI()
        {
            DrawSectionHeader(Label);
            GUILayout.Space(15);

            // VSCode 路径配置
            DrawVSCodePathConfig();
            GUILayout.Space(15);

            // 编辑选项
            DrawEditOptions();
            GUILayout.Space(15);

            // VSCode 信息
            DrawVSCodeInfo();
        }

        private void DrawVSCodePathConfig()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📁 VSCode 路径", EditorStyles.boldLabel);
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("VSCode 路径:", GUILayout.Width(80));

            if (_availableVSCodePaths.Length > 0)
            {
                int newIndex = EditorGUILayout.Popup(_selectedVSCodeIndex, _availableVSCodePaths);
                if (newIndex != _selectedVSCodeIndex && newIndex >= 0)
                {
                    _selectedVSCodeIndex = newIndex;
                    _viewModel.VSCodePath = _availableVSCodePaths[newIndex];
                }
            }
            else
            {
                GUILayout.Label("(未找到 VSCode)", EditorStyles.helpBox);
            }

            if (GUILayout.Button("浏览...", GUILayout.Width(60)))
            {
                _newVSCodePath = EditorUtility.OpenFilePanel("选择 VSCode 可执行文件", _newVSCodePath, "");
                if (!string.IsNullOrEmpty(_newVSCodePath))
                {
                    _viewModel.VSCodePath = _newVSCodePath;
                    var list = _availableVSCodePaths.ToList();
                    if (!list.Contains(_newVSCodePath))
                    {
                        list.Insert(0, _newVSCodePath);
                        _availableVSCodePaths = list.ToArray();
                        _selectedVSCodeIndex = 0;
                    }
                    else
                    {
                        _selectedVSCodeIndex = list.IndexOf(_newVSCodePath);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_viewModel.VSCodePath))
            {
                GUILayout.Space(5);
                DrawInfoRow("当前路径", _viewModel.VSCodePath);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawEditOptions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🔧 编辑选项", EditorStyles.boldLabel);
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("📝 在 VSCode 中编辑", GUILayout.Height(38)))
            {
                try { _viewModel.CombineAndOpenInVSCode(); }
                catch { Debug.LogError("打开VSCode失败"); }
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("💾 保存更改", GUILayout.Height(38)))
            {
                _viewModel.SeparateAndSave();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _viewModel.OpenFolderEnabled = GUILayout.Toggle(_viewModel.OpenFolderEnabled, "同时打开文件夹", GUILayout.Width(110));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawVSCodeInfo()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("ℹ️ VSCode 信息", EditorStyles.boldLabel);
            GUILayout.Space(5);

            DrawInfoRow("扫描到的 VSCode", _availableVSCodePaths.Length > 0 ? $"{_availableVSCodePaths.Length} 个" : "未找到");

            if (_availableVSCodePaths.Length > 0)
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField("可用的 VSCode 路径:", EditorStyles.boldLabel);
                foreach (var path in _availableVSCodePaths)
                {
                    DrawInfoBox(path);
                }
            }
            else
            {
                DrawInfoBox("未检测到 VSCode 安装，请手动指定 VSCode 可执行文件路径");
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawInfoRow(string label, string value)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, EditorStyles.boldLabel, GUILayout.Width(120));
            GUILayout.Label(value, EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private VisualElement DrawVSCodePathConfigUI()
        {
            var container = new VisualElement();

            var label = new Label("📁 VSCode 路径");
            label.style.fontSize = 16;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 5;
            container.Add(label);

            var pathRow = new VisualElement();
            pathRow.style.flexDirection = FlexDirection.Row;
            pathRow.style.alignItems = Align.Center;
            pathRow.style.marginBottom = 15;

            var pathLabel = new Label("当前路径:");
            pathLabel.style.width = 80;
            pathRow.Add(pathLabel);

            var pathValue = new Label(_viewModel.VSCodePath ?? "未设置");
            pathValue.style.flexGrow = 1;
            pathRow.Add(pathValue);

            container.Add(pathRow);

            var browseBtn = new Button(BrowseVSCodePath);
            browseBtn.text = "浏览 VSCode 可执行文件...";
            container.Add(browseBtn);

            return container;
        }

        private VisualElement DrawEditOptionsUI()
        {
            var container = new VisualElement();

            var label = new Label("🔧 编辑选项");
            label.style.fontSize = 16;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 5;
            container.Add(label);

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 15;

            var editBtn = new Button(OpenInVSCode);
            editBtn.text = "📝 在 VSCode 中编辑";
            editBtn.style.height = 38;
            editBtn.style.marginRight = 5;
            row.Add(editBtn);

            var flexibleSpace = new VisualElement();
            flexibleSpace.style.flexGrow = 1;
            row.Add(flexibleSpace);

            var saveBtn = new Button(() => _viewModel.SeparateAndSave());
            saveBtn.text = "💾 保存更改";
            saveBtn.style.height = 38;
            row.Add(saveBtn);

            container.Add(row);

            var toggleRow = new VisualElement();
            toggleRow.style.flexDirection = FlexDirection.Row;
            var openFolderToggle = new Toggle("同时打开文件夹");
            openFolderToggle.value = _viewModel.OpenFolderEnabled;
            openFolderToggle.RegisterValueChangedCallback(evt => _viewModel.OpenFolderEnabled = evt.newValue);
            toggleRow.Add(openFolderToggle);
            container.Add(toggleRow);

            return container;
        }

        private VisualElement DrawVSCodeInfoUI()
        {
            var container = new VisualElement();

            var label = new Label("ℹ️ VSCode 信息");
            label.style.fontSize = 16;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 5;
            container.Add(label);

            var countLabel = new Label($"扫描到的 VSCode: {(_availableVSCodePaths.Length > 0 ? $"{_availableVSCodePaths.Length} 个" : "未找到")}");
            countLabel.style.marginBottom = 15;
            container.Add(countLabel);

            if (_availableVSCodePaths.Length > 0)
            {
                var pathsLabel = new Label("可用的 VSCode 路径:");
                pathsLabel.style.fontSize = 14;
                pathsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                container.Add(pathsLabel);

                foreach (var path in _availableVSCodePaths)
                {
                    var helpBox = new HelpBox(path, HelpBoxMessageType.Info);
                    helpBox.style.marginBottom = 2;
                    container.Add(helpBox);
                }
            }
            else
            {
                container.Add(new HelpBox("未检测到 VSCode 安装，请手动指定 VSCode 可执行文件路径", HelpBoxMessageType.Warning));
            }

            return container;
        }

        private void BrowseVSCodePath()
        {
            _newVSCodePath = EditorUtility.OpenFilePanel("选择 VSCode 可执行文件", _newVSCodePath, "");
            if (!string.IsNullOrEmpty(_newVSCodePath))
            {
                _viewModel.VSCodePath = _newVSCodePath;
                var list = _availableVSCodePaths.ToList();
                if (!list.Contains(_newVSCodePath))
                {
                    list.Insert(0, _newVSCodePath);
                    _availableVSCodePaths = list.ToArray();
                    _selectedVSCodeIndex = 0;
                }
                else
                {
                    _selectedVSCodeIndex = list.IndexOf(_newVSCodePath);
                }
            }
        }

        private void OpenInVSCode()
        {
            try { _viewModel.CombineAndOpenInVSCode(); }
            catch { Debug.LogError("打开VSCode失败"); }
        }
    }
}
