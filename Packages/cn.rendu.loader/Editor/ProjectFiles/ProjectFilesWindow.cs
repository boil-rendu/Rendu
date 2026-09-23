#if UNITY_2019_4_OR_NEWER
using RD.Init.Editor;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 项目文件管理窗口
    /// </summary>
    public class ProjectFilesWindow : EditorWindow
    {
        private ProjectFilesViewModel _viewModel;

        public static void ShowWindow()
        {
            var window = GetWindow<ProjectFilesWindow>("Project Files", true, WindowsDefine.DockedWindowTypes);
            window.minSize = new Vector2(600, 500);
        }

        public void CreateGUI()
        {
            try
            {
                VisualElement root = this.rootVisualElement;

                // 加载布局文件
                var visualAsset = UxmlLoader.LoadWindowUXML<ProjectFilesWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 初始化 ViewModel
                if (_viewModel == null)
                {
                    _viewModel = new ProjectFilesViewModel(new ProjectFilesModel());
                }

                // 获取UI元素
                var slnStatusLabel = root.Q<Label>("SlnStatusLabel");
                var linkSlnButton = root.Q<Button>("LinkSlnButton");
                var regenerateProjectButton = root.Q<Button>("RegenerateProjectButton");
                var refreshDatabaseButton = root.Q<Button>("RefreshDatabaseButton");
                var generateReferencesButton = root.Q<Button>("GenerateReferencesButton");
                var updateReferencesButton = root.Q<Button>("UpdateReferencesButton");
                var runInitScriptsButton = root.Q<Button>("RunInitScriptsButton");

                // 更新 Sln 状态
                UpdateSlnStatus(slnStatusLabel);

                // 绑定按钮事件
                if (linkSlnButton != null)
                {
                    linkSlnButton.clicked += () =>
                    {
                        _viewModel.LinkSln();
                        UpdateSlnStatus(slnStatusLabel);
                    };
                }

                if (regenerateProjectButton != null)
                {
                    regenerateProjectButton.clicked += () => _viewModel.RegenerateProjectFiles();
                }

                if (refreshDatabaseButton != null)
                {
                    refreshDatabaseButton.clicked += () => _viewModel.RefreshAssetDatabase();
                }

                if (generateReferencesButton != null)
                {
                    generateReferencesButton.clicked += () => _viewModel.GenerateAssemblyReferences();
                }

                if (updateReferencesButton != null)
                {
                    updateReferencesButton.clicked += () => _viewModel.UpdateScriptsReferences();
                }

                if (runInitScriptsButton != null)
                {
                    runInitScriptsButton.clicked += () => _viewModel.RunInitScripts();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void UpdateSlnStatus(Label slnStatusLabel)
        {
            if (slnStatusLabel != null)
            {
                bool isLinked = _viewModel.IsSlnLinked;
                slnStatusLabel.text = isLinked ? "Sln 状态：✅ 已链接" : "Sln 状态：❌ 未链接";
                slnStatusLabel.style.color = isLinked ? new Color(0.2f, 0.7f, 0.3f) : new Color(0.9f, 0.4f, 0.4f);
            }
        }

        private void OnEnable()
        {
            if (_viewModel == null)
            {
                _viewModel = new ProjectFilesViewModel(new ProjectFilesModel());
            }
        }

        private void OnDisable()
        {
            _viewModel?.Dispose();
        }
    }
}
#endif
