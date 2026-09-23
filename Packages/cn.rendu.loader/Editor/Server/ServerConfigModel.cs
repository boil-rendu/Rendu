using System.IO;
using System.Linq;

namespace RD.Loader.Editor
{
    public class ServerConfigModel
    {
        public string[] StartConfigs { get; private set; }
        public int SelectedStartConfigIndex { get; set; }
        public string StartConfig { get; set; }
        public DevelopMode DevelopMode { get; set; }
        public string SceneName { get; set; }

        private string _startConfigPath;

        public void LoadStartConfigs()
        {
            // 根据代码模式选择正确的配置路径
            // 服务器默认使用 Server 配置
            _startConfigPath = "Packages/cn.rendu.excel/Config/Bytes/Server/StartConfig";

            if (!Directory.Exists(_startConfigPath))
            {
                UnityEngine.Debug.LogWarning($"启动配置路径不存在: {_startConfigPath}");
                StartConfigs = new string[0];
                StartConfig = "";
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(_startConfigPath);
            StartConfigs = directoryInfo.GetDirectories().Select(x => x.Name).ToArray();

            UnityEngine.Debug.Log($"找到 {StartConfigs.Length} 个启动配置: {string.Join(", ", StartConfigs)}");

            if (SelectedStartConfigIndex >= 0 && SelectedStartConfigIndex < StartConfigs.Length)
            {
                StartConfig = StartConfigs[SelectedStartConfigIndex];
            }
            else if (StartConfigs.Length > 0)
            {
                StartConfig = StartConfigs[0];
                SelectedStartConfigIndex = 0;
            }
        }
    }
}