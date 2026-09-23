using System;
using RD.Init.Editor;
using UnityEngine;

namespace RD.Loader.Editor
{
    public class ServerViewModel : ViewModelBase
    {
        private ServerConfigModel _model;
        private bool _isGameServerRunning;
        private bool _isMongoRunning;
        private bool _isWatcherRunning;

        public ServerViewModel(ServerConfigModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));

            _model.LoadStartConfigs();
            OnPropertyChanged(nameof(StartConfigs));

            // 初始化状态（简化处理，实际应从外部获取真实状态）
            _isGameServerRunning = false;
            _isMongoRunning = false;
            _isWatcherRunning = false;
        }

        public bool IsGameServerRunning
        {
            get => _isGameServerRunning;
            private set
            {
                if (_isGameServerRunning != value)
                {
                    _isGameServerRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsMongoRunning
        {
            get => _isMongoRunning;
            private set
            {
                if (_isMongoRunning != value)
                {
                    _isMongoRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsWatcherRunning
        {
            get => _isWatcherRunning;
            private set
            {
                if (_isWatcherRunning != value)
                {
                    _isWatcherRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        public string[] StartConfigs => _model.StartConfigs;

        public int SelectedStartConfigIndex
        {
            get => _model.SelectedStartConfigIndex;
            set
            {
                if (_model.SelectedStartConfigIndex != value)
                {
                    _model.SelectedStartConfigIndex = value;
                    _model.StartConfig = _model.StartConfigs[value];
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StartConfig));
                }
            }
        }

        public string StartConfig
        {
            get => _model.StartConfig;
            set
            {
                if (_model.StartConfig != value)
                {
                    _model.StartConfig = value;
                    OnPropertyChanged();
                }
            }
        }

        public DevelopMode DevelopMode
        {
            get => _model.DevelopMode;
            set
            {
                if (_model.DevelopMode != value)
                {
                    _model.DevelopMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SceneName => _model.SceneName;

        /// <summary>
        /// 切换服务器状态
        /// </summary>
        public void ToggleServer()
        {
            if (IsGameServerRunning)
            {
                StopServer();
            }
            else
            {
                StartServerSingleProcess();
            }
        }

        /// <summary>
        /// 重启服务器
        /// </summary>
        public void RestartServer()
        {
            if (IsGameServerRunning)
            {
                StopServer();
                System.Threading.Thread.Sleep(1000);
            }
            StartServerSingleProcess();
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        public void StartServerSingleProcess()
        {
            Debug.Log($"[Server] ========== 启动服务器: Scene={_model.SceneName}, Config={_model.StartConfig}, Process=1 ==========");
            new StartServerCommand(_model.SceneName, 1, _model.StartConfig, true).Execute();
            IsGameServerRunning = true;
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public void StopServer()
        {
            Debug.Log("[Server] ========== 停止服务器 ==========");
            // 实际应调用停止命令，这里简化处理
            IsGameServerRunning = false;
        }

        /// <summary>
        /// 切换MongoDB状态
        /// </summary>
        public void ToggleMongo()
        {
            if (IsMongoRunning)
            {
                StopMongo();
            }
            else
            {
                StartMongo();
            }
        }

        /// <summary>
        /// 启动MongoDB
        /// </summary>
        public void StartMongo()
        {
            Debug.Log("[Server] ========== 启动 MongoDB ==========");
            new StartMongoCommand().Execute();
            IsMongoRunning = true;
        }

        /// <summary>
        /// 停止MongoDB
        /// </summary>
        public void StopMongo()
        {
            Debug.Log("[Server] ========== 停止 MongoDB ==========");
            IsMongoRunning = false;
        }

        /// <summary>
        /// 切换文件监听状态
        /// </summary>
        public void ToggleWatcher()
        {
            if (IsWatcherRunning)
            {
                StopWatcher();
            }
            else
            {
                StartWatcher();
            }
        }

        /// <summary>
        /// 启动文件监听
        /// </summary>
        public void StartWatcher()
        {
            Debug.Log($"[Server] ========== 启动文件监视器: Config={_model.StartConfig} ==========");
            new StartWatcherCommand(_model.StartConfig, true).Execute();
            IsWatcherRunning = true;
        }

        /// <summary>
        /// 停止文件监听
        /// </summary>
        public void StopWatcher()
        {
            Debug.Log("[Server] ========== 停止文件监视器 ==========");
            IsWatcherRunning = false;
        }
    }
}
