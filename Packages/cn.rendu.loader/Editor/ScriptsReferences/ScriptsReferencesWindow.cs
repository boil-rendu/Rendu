using RD.Init.Editor;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 脚本引用配置窗口
    /// </summary>
    public class ScriptsReferencesWindow : EditorWindow
    {
        private ScriptsReferencesViewModel _viewModel;

        public static void ShowWindow()
        {
            var window = GetWindow<ScriptsReferencesWindow>("Script References", true, WindowsDefine.DockedWindowTypes);
            window.minSize = new Vector2(600, 500);
        }

        public void CreateGUI()
        {
            try
            {
                VisualElement root = this.rootVisualElement;

                // 加载布局文件
                var visualAsset = UxmlLoader.LoadWindowUXML<ScriptsReferencesWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 初始化 ViewModel
                if (_viewModel == null)
                {
                    _viewModel = new ScriptsReferencesViewModel(new ScriptsReferencesModel());
                }

                // 获取UI元素
                var infoLabel = root.Q<Label>("InfoLabel");
                var generateButton = root.Q<Button>("GenerateButton");
                var updateButton = root.Q<Button>("UpdateButton");
                var viewConfigButton = root.Q<Button>("ViewConfigButton");

                // 设置信息文本
                if (infoLabel != null)
                {
                    infoLabel.text = "脚本引用配置用于管理项目中各个程序集之间的引用关系。\n\n" +
                        "通过正确的引用配置，可以确保代码的模块化和清晰的依赖关系。";
                    infoLabel.style.color = new Color(0.4f, 0.6f, 0.9f);
                }

                // 绑定按钮事件
                if (generateButton != null)
                {
                    generateButton.clicked += () =>
                    {
                        if (EditorUtility.DisplayDialog("确认操作", "确定要生成脚本引用吗？", "确定", "取消"))
                        {
                            _viewModel.GenerateReferences();
                        }
                    };
                }

                if (updateButton != null)
                {
                    updateButton.clicked += () => _viewModel.UpdateReferences();
                }

                if (viewConfigButton != null)
                {
                    viewConfigButton.clicked += () => _viewModel.ShowCurrentConfig();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void OnEnable()
        {
            if (_viewModel == null)
            {
                _viewModel = new ScriptsReferencesViewModel(new ScriptsReferencesModel());
            }
        }

        private void OnDisable()
        {
            _viewModel?.Dispose();
        }
    }
}
