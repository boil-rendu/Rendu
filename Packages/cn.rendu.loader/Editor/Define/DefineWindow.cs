using RD.Init.Editor;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 编译符号配置窗口
    /// </summary>
    public class DefineWindow : EditorWindow
    {
        private DefineViewModel _viewModel;

        public static void ShowWindow()
        {
            var window = GetWindow<DefineWindow>("Scripting Defines", true, WindowsDefine.DockedWindowTypes);
            window.minSize = new Vector2(500, 400);
        }

        public void CreateGUI()
        {
            try
            {
                VisualElement root = this.rootVisualElement;

                // 加载布局文件
                var visualAsset = UxmlLoader.LoadWindowUXML<DefineWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 初始化 ViewModel
                if (_viewModel == null)
                {
                    _viewModel = new DefineViewModel();
                }
                _viewModel.LoadDefines();

                // 获取UI元素
                var definesScrollView = root.Q<ScrollView>("DefinesScrollView");
                var initedToggle = root.Q<Toggle>("INITEDToggle");
                var clientServerToggle = root.Q<Toggle>("CLIENTSERVERToggle");
                var serverToggle = root.Q<Toggle>("SERVERToggle");
                var clientToggle = root.Q<Toggle>("CLIENTToggle");
                var enableHotfixToggle = root.Q<Toggle>("ENABLE_HOTFIXToggle");
                var enableHybridClrToggle = root.Q<Toggle>("ENABLE_HYBRIDCLRToggle");
                var saveButton = root.Q<Button>("SaveButton");

                // 显示当前编译符号
                if (definesScrollView != null)
                {
                    string currentDefines = _viewModel.CurrentDefines;
                    if (string.IsNullOrEmpty(currentDefines))
                    {
                        definesScrollView.Add(new Label("(无编译符号)"));
                    }
                    else
                    {
                        definesScrollView.Add(new Label(currentDefines));
                    }
                }

                // 绑定编译符号开关
                if (initedToggle != null)
                {
                    initedToggle.value = _viewModel.IsDefineEnabled("INITED");
                    initedToggle.RegisterValueChangedCallback(evt => ToggleDefine("INITED", evt.newValue));
                }

                if (clientServerToggle != null)
                {
                    clientServerToggle.value = _viewModel.IsDefineEnabled("CLIENTSERVER");
                    clientServerToggle.RegisterValueChangedCallback(evt => ToggleDefine("CLIENTSERVER", evt.newValue));
                }

                if (serverToggle != null)
                {
                    serverToggle.value = _viewModel.IsDefineEnabled("SERVER");
                    serverToggle.RegisterValueChangedCallback(evt => ToggleDefine("SERVER", evt.newValue));
                }

                if (clientToggle != null)
                {
                    clientToggle.value = _viewModel.IsDefineEnabled("CLIENT");
                    clientToggle.RegisterValueChangedCallback(evt => ToggleDefine("CLIENT", evt.newValue));
                }

                if (enableHotfixToggle != null)
                {
                    enableHotfixToggle.value = _viewModel.IsDefineEnabled("ENABLE_HOTFIX");
                    enableHotfixToggle.RegisterValueChangedCallback(evt => ToggleDefine("ENABLE_HOTFIX", evt.newValue));
                }

                if (enableHybridClrToggle != null)
                {
                    enableHybridClrToggle.value = _viewModel.IsDefineEnabled("ENABLE_HYBRIDCLR");
                    enableHybridClrToggle.RegisterValueChangedCallback(evt => ToggleDefine("ENABLE_HYBRIDCLR", evt.newValue));
                }

                // 绑定保存按钮
                if (saveButton != null)
                {
                    saveButton.clicked += () => _viewModel.SaveDefines();
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

        private void ToggleDefine(string define, bool enable)
        {
            if (enable)
            {
                _viewModel.AddDefine(define);
            }
            else
            {
                _viewModel.RemoveDefine(define);
            }
        }
    }
}
