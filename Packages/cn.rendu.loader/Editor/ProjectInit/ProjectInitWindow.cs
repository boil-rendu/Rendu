using RD.Init.Editor;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 项目初始化窗口
    /// </summary>
    public class ProjectInitWindow : EditorWindow
    {
        private ProjectInitViewModel _viewModel;

        public static void ShowWindow()
        {
            var window = GetWindow<ProjectInitWindow>("Project Init", true, WindowsDefine.DockedWindowTypes);
            window.minSize = new Vector2(600, 500);
        }

        public void CreateGUI()
        {
            try
            {
                VisualElement root = this.rootVisualElement;

                // 加载布局文件
                var visualAsset = UxmlLoader.LoadWindowUXML<ProjectInitWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 初始化 ViewModel
                if (_viewModel == null)
                {
                    _viewModel = new ProjectInitViewModel(new ProjectInitModel());
                }
                _viewModel.LoadPackageInfo();

                // 获取UI元素
                var infoLabel = root.Q<Label>("InfoLabel");
                var initButton = root.Q<Button>("InitButton");
                var packageNameLabel = root.Q<Label>("PackageNameLabel");
                var sceneNameLabel = root.Q<Label>("SceneNameLabel");
                var codeModeLabel = root.Q<Label>("CodeModeLabel");
                var addressLabel = root.Q<Label>("AddressLabel");
                var statusLabel = root.Q<Label>("StatusLabel");

                // 更新UI
                UpdateUI(infoLabel, initButton, packageNameLabel, sceneNameLabel, codeModeLabel, addressLabel, statusLabel);

                // 绑定按钮事件
                if (initButton != null)
                {
                    initButton.clicked += () =>
                    {
                        _viewModel.InitProject();
                        UpdateUI(infoLabel, initButton, packageNameLabel, sceneNameLabel, codeModeLabel, addressLabel, statusLabel);
                    };
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void UpdateUI(Label infoLabel, Button initButton, Label packageNameLabel, Label sceneNameLabel, Label codeModeLabel, Label addressLabel, Label statusLabel)
        {
            if (_viewModel.IsProcessing)
            {
                if (infoLabel != null)
                {
                    infoLabel.text = "⏳ 正在初始化项目...";
                    infoLabel.style.color = new Color(0.95f, 0.6f, 0.1f);
                }
                if (initButton != null) initButton.style.display = DisplayStyle.None;
                return;
            }

            bool isInited = _viewModel.CheckIsInited();

            if (infoLabel != null)
            {
                if (isInited)
                {
                    infoLabel.text = "✅ 您的项目已经初始化完成！\n\n如需重新初始化，请在 Unity Player Settings 中移除 'INITED' 宏定义。";
                    infoLabel.style.color = new Color(0.95f, 0.7f, 0.1f);
                }
                else
                {
                    infoLabel.text = "初始化项目将执行以下步骤：\n1. 在 GlobalConfig 中设置 SceneName\n2. 链接 SLN 文件用于 Rider/VS\n3. 更新脚本程序集引用\n4. 切换到 ClientServer 代码模式\n5. 运行初始化脚本\n6. 启用 INITED 宏定义";
                    infoLabel.style.color = new Color(0.4f, 0.6f, 0.9f);
                }
            }

            if (initButton != null)
            {
                initButton.style.display = isInited ? DisplayStyle.None : DisplayStyle.Flex;
            }

            if (packageNameLabel != null)
            {
                packageNameLabel.text = $"包名称：{_viewModel?.Model?.PackageName ?? "(未设置)"}";
            }

            if (sceneNameLabel != null)
            {
                sceneNameLabel.text = $"场景名称：{_viewModel?.Model?.SceneName ?? "(未设置)"}";
            }

            if (codeModeLabel != null)
            {
                codeModeLabel.text = $"代码模式：{_viewModel?.Model?.CodeMode.ToString() ?? "(未设置)"}";
            }

            if (addressLabel != null)
            {
                addressLabel.text = $"服务器地址：{_viewModel?.Model?.Address ?? "(未设置)"}";
            }

            if (statusLabel != null)
            {
                bool modelIsInited = _viewModel?.Model?.IsInited ?? false;
                statusLabel.text = $"状态：{(modelIsInited ? "✅ 已初始化" : "❌ 未初始化")}";
            }
        }

        private void OnDisable()
        {
            _viewModel?.Dispose();
        }
    }
}
