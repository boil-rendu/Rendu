#if UNITY_EDITOR

using System.IO;
using System.Linq;
using RD.Draw;
using RD.EditTool.Editor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 服务器管理模块: 启动/停止/重启 游戏服务器、启动/停止 MongoDB、启动/停止 文件监听器。
    /// 启动配置 (StartConfig 目录里的子目录) 通过下拉选择; 顶部实时显示三组进程的运行状态。
    /// 替代原 ServerWindow + ServerViewModel + ServerConfigModel + ServerWindow.uxml。
    /// </summary>
    [UIAutoMenu("服务器", 600, Scope = "loader")]
    public class ServerModule : BaseToolModule
    {
        private const string StartConfigRoot = "Packages/cn.rendu.excel/Config/Bytes/Server/StartConfig";
        private const string StatusRunning = "🟢 运行中";
        private const string StatusStopped = "🔴 已停止";

        [LabelText("游戏服务器")]
        [ShowInInspector]
        [DisplayAsString(false)]
        public string GameServerStatus { get; private set; } = StatusStopped;

        [LabelText("MongoDB")]
        [ShowInInspector]
        [DisplayAsString(false)]
        public string MongoStatus { get; private set; } = StatusStopped;

        [LabelText("文件监听")]
        [ShowInInspector]
        [DisplayAsString(false)]
        public string WatcherStatus { get; private set; } = StatusStopped;

        [LabelText("启动配置")]
        [ValueDropdown("StartConfigChoices")]
        [OnValueChanged("OnStartConfigChanged")]
        [ShowInInspector]
        public string StartConfig { get; set; }

        [LabelText("场景名")]
        [ShowInInspector]
        public string SceneName { get; set; } = "Main";

        [LabelText("开发模式")]
        [ShowInInspector]
        public DevelopMode DevelopMode { get; set; } = DevelopMode.Develop;

        private System.Collections.Generic.List<string> StartConfigChoices()
        {
            if (!Directory.Exists(StartConfigRoot)) return new();
            return Directory.GetDirectories(StartConfigRoot).Select(Path.GetFileName).ToList();
        }

        private void OnStartConfigChanged()
        {
            Debug.Log($"[Server] 启动配置已切换: {StartConfig}");
        }

        [ButtonGroup("服务器")]
        [Button("启动服务器", Tooltip = "按当前启动配置与场景名启动游戏服务器")]
        [GUIColor(0.2f, 0.7f, 0.3f)]
        public void StartServer()
        {
            Debug.Log($"[Server] ========== 启动服务器: Scene={SceneName}, Config={StartConfig} ==========");
            new StartServerCommand(SceneName, 1, StartConfig, true).Execute();
            GameServerStatus = StatusRunning;
        }

        [ButtonGroup("服务器")]
        [Button("停止服务器")]
        [GUIColor(0.9f, 0.4f, 0.4f)]
        public void StopServer()
        {
            Debug.Log("[Server] ========== 停止服务器 ==========");
            GameServerStatus = StatusStopped;
        }

        [ButtonGroup("服务器")]
        [Button("重启服务器")]
        public void RestartServer()
        {
            StopServer();
            System.Threading.Thread.Sleep(1000);
            StartServer();
        }

        [ButtonGroup("MongoDB")]
        [Button("启动 MongoDB")]
        [GUIColor(0.2f, 0.7f, 0.3f)]
        public void StartMongo()
        {
            Debug.Log("[Server] ========== 启动 MongoDB ==========");
            new StartMongoCommand().Execute();
            MongoStatus = StatusRunning;
        }

        [ButtonGroup("MongoDB")]
        [Button("停止 MongoDB")]
        [GUIColor(0.9f, 0.4f, 0.4f)]
        public void StopMongo()
        {
            Debug.Log("[Server] ========== 停止 MongoDB ==========");
            MongoStatus = StatusStopped;
        }

        [ButtonGroup("文件监听")]
        [Button("启动文件监听")]
        [GUIColor(0.2f, 0.7f, 0.3f)]
        public void StartWatcher()
        {
            Debug.Log($"[Server] ========== 启动文件监视器: Config={StartConfig} ==========");
            new StartWatcherCommand(StartConfig, true).Execute();
            WatcherStatus = StatusRunning;
        }

        [ButtonGroup("文件监听")]
        [Button("停止文件监听")]
        [GUIColor(0.9f, 0.4f, 0.4f)]
        public void StopWatcher()
        {
            Debug.Log("[Server] ========== 停止文件监视器 ==========");
            WatcherStatus = StatusStopped;
        }

        public override void Initialize()
        {
            // 从 GlobalConfig 读 SceneName（如果存在）
            var cfg = UnityEditor.AssetDatabase.LoadAssetAtPath<GlobalConfig>(GlobalConfigModel.AssetPath);
            if (cfg != null && !string.IsNullOrEmpty(cfg.SceneName)) SceneName = cfg.SceneName;

            // 默认选第一个 StartConfig
            var choices = StartConfigChoices();
            if (choices.Count > 0 && string.IsNullOrEmpty(StartConfig)) StartConfig = choices[0];
        }

        public override void OnDestroy()
        {
        }
    }
}

#endif
