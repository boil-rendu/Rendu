using RD.Init.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// YooConfig 配置窗口（视图层）
    /// </summary>
    public class YooConfigWindow : EditorWindow
    {
        private YooConfigViewModel _viewModel;

        private EnumField _playModeField;
        private TextField _urlField;
        private Toggle _editorForceRedownloadField;
        private IntegerField _editorSimulateDownloadSpeedField;
        private Toggle _hostForceClearSandboxField;

        [MenuItem("YooAsset/Rendu/Yoo Config", false, 1000)]
        public static void ShowWindow()
        {
            var window = GetWindow<YooConfigWindow>("Yoo Config", true, EditorWindowDefine.GetDockedWindowTypes());
            window.minSize = new Vector2(500, 400);
        }

        public void CreateGUI()
        {
            try
            {
                VisualElement root = this.rootVisualElement;

                // 加载布局文件
                var visualAsset = UxmlLoader.LoadWindowUxml<YooConfigWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 获取 UI 元素
                _playModeField = root.Q<EnumField>("PlayModeField");
                _urlField = root.Q<TextField>("UrlField");
                _editorForceRedownloadField = root.Q<Toggle>("EditorForceRedownloadField");
                _editorSimulateDownloadSpeedField = root.Q<IntegerField>("EditorSimulateDownloadSpeedField");
                _hostForceClearSandboxField = root.Q<Toggle>("HostForceClearSandboxField");

                // 初始化 ViewModel
                if (_viewModel == null)
                {
                    _viewModel = new YooConfigViewModel(new YooConfigModel());
                }
                _viewModel.LoadConfig();

                // 绑定 EPlayMode
                if (_playModeField != null)
                {
                    _playModeField.Init(_viewModel.EPlayMode);
                    _playModeField.RegisterValueChangedCallback(evt =>
                    {
                        if (evt.newValue is EPlayMode newMode)
                        {
                            _viewModel.EPlayMode = newMode;
                            UpdateFieldsVisibility(newMode);
                        }
                    });
                }

                // 绑定 Url
                if (_urlField != null)
                {
                    _urlField.value = _viewModel.Url;
                    _urlField.RegisterValueChangedCallback(evt =>
                    {
                        _viewModel.Url = evt.newValue;
                    });
                }

                // 绑定 EditorForceRedownload
                if (_editorForceRedownloadField != null)
                {
                    _editorForceRedownloadField.value = _viewModel.EditorForceRedownload;
                    _editorForceRedownloadField.RegisterValueChangedCallback(evt =>
                    {
                        _viewModel.EditorForceRedownload = evt.newValue;
                    });
                }

                // 绑定 EditorSimulateDownloadSpeed
                if (_editorSimulateDownloadSpeedField != null)
                {
                    _editorSimulateDownloadSpeedField.value = _viewModel.EditorSimulateDownloadSpeed;
                    _editorSimulateDownloadSpeedField.RegisterValueChangedCallback(evt =>
                    {
                        _viewModel.EditorSimulateDownloadSpeed = evt.newValue;
                    });
                }

                // 绑定 HostForceClearSandbox
                if (_hostForceClearSandboxField != null)
                {
                    _hostForceClearSandboxField.value = _viewModel.HostForceClearSandbox;
                    _hostForceClearSandboxField.RegisterValueChangedCallback(evt =>
                    {
                        _viewModel.HostForceClearSandbox = evt.newValue;
                    });
                }

                // 按当前模式显示对应字段
                UpdateFieldsVisibility(_viewModel.EPlayMode);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"YooConfigWindow 初始化失败：{e}");
            }
        }

        /// <summary>
        /// 根据运行模式切换可见字段（与 YooConfig 自定义 Inspector 保持一致）
        /// </summary>
        private void UpdateFieldsVisibility(EPlayMode mode)
        {
            if (_urlField != null)
                _urlField.style.display = mode == EPlayMode.HostPlayMode ? DisplayStyle.Flex : DisplayStyle.None;
            if (_editorForceRedownloadField != null)
                _editorForceRedownloadField.style.display = mode == EPlayMode.EditorSimulateMode ? DisplayStyle.Flex : DisplayStyle.None;
            if (_editorSimulateDownloadSpeedField != null)
                _editorSimulateDownloadSpeedField.style.display = mode == EPlayMode.EditorSimulateMode ? DisplayStyle.Flex : DisplayStyle.None;
            if (_hostForceClearSandboxField != null)
                _hostForceClearSandboxField.style.display = mode == EPlayMode.HostPlayMode ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnDisable()
        {
            _viewModel?.Dispose();
        }
    }
}
