using RD.Init.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 全局配置窗口
    /// </summary>
    public class GlobalConfigWindow : EditorWindow
    {
        private GlobalConfigViewModel _viewModel;

        public static void ShowWindow()
        {
            var window = GetWindow<GlobalConfigWindow>("Global Config", true, WindowsDefine.DockedWindowTypes);
            window.minSize = new Vector2(500, 400);
        }

        public void CreateGUI()
        {
            try
            {
                VisualElement root = this.rootVisualElement;

                // 加载布局文件
                var visualAsset = UxmlLoader.LoadWindowUXML<GlobalConfigWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 获取UI元素
                var codeModeField = root.Q<EnumField>("CodeModeField");
                var sceneNameField = root.Q<TextField>("SceneNameField");
                var addressField = root.Q<TextField>("AddressField");
                var regenerateButton = root.Q<Button>("RegenerateButton");

                // 初始化 ViewModel
                if (_viewModel == null)
                {
                    _viewModel = new GlobalConfigViewModel(new GlobalConfigModel());
                }
                _viewModel.LoadConfig();

                // 设置初始值
                if (codeModeField != null)
                {
                    codeModeField.Init(_viewModel.CodeMode);
                    codeModeField.RegisterValueChangedCallback(evt =>
                    {
                        if (evt.newValue is CodeMode newMode)
                        {
                            _viewModel.CodeMode = newMode;
                        }
                    });
                }

                if (sceneNameField != null)
                {
                    sceneNameField.value = _viewModel.SceneName;
                    sceneNameField.RegisterValueChangedCallback(evt =>
                    {
                        _viewModel.SceneName = evt.newValue;
                    });
                }

                if (addressField != null)
                {
                    addressField.value = _viewModel.Address;
                    addressField.RegisterValueChangedCallback(evt =>
                    {
                        _viewModel.Address = evt.newValue;
                    });
                }

                // 绑定按钮事件
                if (regenerateButton != null)
                {
                    regenerateButton.clicked += () =>
                    {
                        if (EditorUtility.DisplayDialog("确认操作", "确定要重新生成 AssemblyReference 吗？", "确定", "取消"))
                        {
                            _viewModel.ReGenerateAssemblyReference();
                        }
                    };
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void OnDisable()
        {
            _viewModel?.Dispose();
        }
    }
}
