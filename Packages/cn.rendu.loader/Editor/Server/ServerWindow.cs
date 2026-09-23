using RD.Init.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 服务器管理窗口
    /// </summary>
    public class ServerWindow : EditorWindow
    {
        private ServerViewModel _viewModel;

        public static void ShowWindow()
        {
            var window = GetWindow<ServerWindow>("Server Management", true, WindowsDefine.DockedWindowTypes);
            window.minSize = new Vector2(500, 400);
        }

        public void CreateGUI()
        {
            try
            {
                VisualElement root = this.rootVisualElement;

                // 加载布局文件
                var visualAsset = UxmlLoader.LoadWindowUXML<ServerWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 初始化 ViewModel
                if (_viewModel == null)
                {
                    _viewModel = new ServerViewModel(new ServerConfigModel());
                }

                // 获取UI元素
                var gameServerStatusLabel = root.Q<Label>("GameServerStatusLabel");
                var mongoStatusLabel = root.Q<Label>("MongoStatusLabel");
                var watcherStatusLabel = root.Q<Label>("WatcherStatusLabel");
                var toggleServerButton = root.Q<Button>("ToggleServerButton");
                var restartServerButton = root.Q<Button>("RestartServerButton");
                var toggleMongoButton = root.Q<Button>("ToggleMongoButton");
                var toggleWatcherButton = root.Q<Button>("ToggleWatcherButton");

                // 更新UI
                UpdateUI(gameServerStatusLabel, mongoStatusLabel, watcherStatusLabel, toggleServerButton, toggleMongoButton, toggleWatcherButton);

                // 绑定按钮事件
                if (toggleServerButton != null)
                {
                    toggleServerButton.clicked += () =>
                    {
                        _viewModel.ToggleServer();
                        UpdateUI(gameServerStatusLabel, mongoStatusLabel, watcherStatusLabel, toggleServerButton, toggleMongoButton, toggleWatcherButton);
                    };
                }

                if (restartServerButton != null)
                {
                    restartServerButton.clicked += () =>
                    {
                        _viewModel.RestartServer();
                        UpdateUI(gameServerStatusLabel, mongoStatusLabel, watcherStatusLabel, toggleServerButton, toggleMongoButton, toggleWatcherButton);
                    };
                }

                if (toggleMongoButton != null)
                {
                    toggleMongoButton.clicked += () =>
                    {
                        _viewModel.ToggleMongo();
                        UpdateUI(gameServerStatusLabel, mongoStatusLabel, watcherStatusLabel, toggleServerButton, toggleMongoButton, toggleWatcherButton);
                    };
                }

                if (toggleWatcherButton != null)
                {
                    toggleWatcherButton.clicked += () =>
                    {
                        _viewModel.ToggleWatcher();
                        UpdateUI(gameServerStatusLabel, mongoStatusLabel, watcherStatusLabel, toggleServerButton, toggleMongoButton, toggleWatcherButton);
                    };
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void UpdateUI(Label gameServerStatusLabel, Label mongoStatusLabel, Label watcherStatusLabel, Button toggleServerButton, Button toggleMongoButton, Button toggleWatcherButton)
        {
            if (gameServerStatusLabel != null)
            {
                gameServerStatusLabel.text = _viewModel.IsGameServerRunning ? "游戏服务器：🟢 运行中" : "游戏服务器：🔴 已停止";
            }

            if (mongoStatusLabel != null)
            {
                mongoStatusLabel.text = _viewModel.IsMongoRunning ? "MongoDB: 🟢 运行中" : "MongoDB: 🔴 已停止";
            }

            if (watcherStatusLabel != null)
            {
                watcherStatusLabel.text = _viewModel.IsWatcherRunning ? "文件监听：🟢 运行中" : "文件监听：🔴 已停止";
            }

            if (toggleServerButton != null)
            {
                toggleServerButton.text = _viewModel.IsGameServerRunning ? "⏹️ 停止服务器" : "▶️ 启动服务器";
                toggleServerButton.style.backgroundColor = _viewModel.IsGameServerRunning ? new Color(0.9f, 0.4f, 0.4f) : new Color(0.2f, 0.7f, 0.3f);
            }

            if (toggleMongoButton != null)
            {
                toggleMongoButton.text = _viewModel.IsMongoRunning ? "⏹️ 停止 MongoDB" : "▶️ 启动 MongoDB";
                toggleMongoButton.style.backgroundColor = _viewModel.IsMongoRunning ? new Color(0.9f, 0.4f, 0.4f) : new Color(0.2f, 0.7f, 0.3f);
            }

            if (toggleWatcherButton != null)
            {
                toggleWatcherButton.text = _viewModel.IsWatcherRunning ? "👁️ 停止监听" : "👁️ 启动监听";
            }
        }

        private void OnEnable()
        {
            if (_viewModel == null)
            {
                _viewModel = new ServerViewModel(new ServerConfigModel());
            }
        }

        private void OnDisable()
        {
            _viewModel?.Dispose();
        }
    }
}
