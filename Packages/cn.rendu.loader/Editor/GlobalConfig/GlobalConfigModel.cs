using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    public class GlobalConfigModel
    {
        /// <summary>GlobalConfig 资产的唯一路径，避免各处硬编码</summary>
        public const string ResourceFolder = "Packages/cn.rendu.loader/Resources";
        public const string AssetPath      = ResourceFolder + "/GlobalConfig.asset";

        private GlobalConfig _asset;

        public GlobalConfig Config
        {
            get
            {
                if (_asset == null)
                {
                    // 先尝试读取磁盘资产，读不到就创建，都失败才退化为内存实例（保证窗口可编辑、不静默失效）
                    if (!LoadAsset() && !CreateAsset())
                    {
                        _asset = ScriptableObject.CreateInstance<GlobalConfig>();
                        _asset.CodeMode = CodeMode.Client;
                        _asset.SceneName = "Main";
                        _asset.Address = "";
                    }
                }
                return _asset!;
            }
        }
        public CodeMode CodeMode => Config.CodeMode;
        public string SceneName => Config.SceneName;
        public string Address => Config.Address;

    public GlobalConfigModel()
    {
        LoadAsset();
    }

    public GlobalConfigModel(GlobalConfig asset)
    {
        _asset = asset;
        if (_asset == null)
        {
            LoadAsset();
        }
    }

        public bool LoadAsset()
        {
            _asset = AssetDatabase.LoadAssetAtPath<GlobalConfig>(AssetPath);
            return _asset != null;
        }

        public bool CreateAsset()
        {
            if (!AssetDatabase.IsValidFolder(ResourceFolder))
            {
                return false;
            }
            _asset = ScriptableObject.CreateInstance<GlobalConfig>();
            _asset.CodeMode = CodeMode.Client;
            _asset.SceneName = "Main";
            _asset.Address = "";
            AssetDatabase.CreateAsset(_asset, AssetPath);
            AssetDatabase.SaveAssets();
            return true;
        }

        public void SetCodeMode(CodeMode codeMode)
        {
            Config.CodeMode = codeMode;
        }

        public void SetSceneName(string sceneName)
        {
            Config.SceneName = sceneName;
        }

        public void SetAddress(string address)
        {
            Config.Address = address;
        }

        public void SaveAsset()
        {
            EditorUtility.SetDirty(Config);
            AssetDatabase.SaveAssets();
        }
    }
}
