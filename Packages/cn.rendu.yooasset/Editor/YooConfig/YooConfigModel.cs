using RD.YooAsset;
using UnityEditor;
using UnityEngine;
using YooAsset;

namespace YooAsset.Editor
{
    /// <summary>
    /// YooConfig 模型层：负责加载 / 创建 / 保存 YooConfig.asset
    /// </summary>
    public class YooConfigModel
    {
        private const string AssetPath = "Packages/cn.rendu.yooasset/Resources/YooConfig.asset";

        private YooConfig _asset;

        public YooConfig Config
        {
            get
            {
                if (_asset == null)
                {
                    _asset = ScriptableObject.CreateInstance<YooConfig>();
                    _asset.EPlayMode = EPlayMode.EditorSimulateMode;
                    _asset.Url = "";
                }
                return _asset;
            }
        }

        public EPlayMode EPlayMode => _asset?.EPlayMode ?? EPlayMode.EditorSimulateMode;
        public bool EditorForceRedownload => _asset?.EditorForceRedownload ?? true;
        public int EditorSimulateDownloadSpeed => _asset?.EditorSimulateDownloadSpeed ?? 1024 * 1000;
        public string Url => _asset?.Url ?? "";
        public bool HostForceClearSandbox => _asset?.HostForceClearSandbox ?? false;

        public YooConfigModel()
        {
            LoadAsset();
        }

        public YooConfigModel(YooConfig asset)
        {
            _asset = asset;
            if (_asset == null)
            {
                LoadAsset();
            }
        }

        public bool LoadAsset()
        {
            _asset = AssetDatabase.LoadAssetAtPath<YooConfig>(AssetPath);
            return _asset != null;
        }

        public bool CreateAsset()
        {
            if (!AssetDatabase.IsValidFolder("Packages/cn.rendu.yooasset/Resources"))
            {
                return false;
            }
            _asset = ScriptableObject.CreateInstance<YooConfig>();
            _asset.EPlayMode = EPlayMode.EditorSimulateMode;
            _asset.Url = "";
            AssetDatabase.CreateAsset(_asset, AssetPath);
            AssetDatabase.SaveAssets();
            return true;
        }

        public void SetEPlayMode(EPlayMode value)
        {
            if (_asset != null)
                _asset.EPlayMode = value;
        }

        public void SetEditorForceRedownload(bool value)
        {
            if (_asset != null)
                _asset.EditorForceRedownload = value;
        }

        public void SetEditorSimulateDownloadSpeed(int value)
        {
            if (_asset != null)
                _asset.EditorSimulateDownloadSpeed = value;
        }

        public void SetUrl(string value)
        {
            if (_asset != null)
                _asset.Url = value;
        }

        public void SetHostForceClearSandbox(bool value)
        {
            if (_asset != null)
                _asset.HostForceClearSandbox = value;
        }

        public void SaveAsset()
        {
            if (_asset != null)
            {
                EditorUtility.SetDirty(_asset);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
