#if UNITY_EDITOR

using RD.Draw;
using RD.EditTool.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 全局配置模块: 编辑 cn.rendu.loader/Resources/GlobalConfig.asset
    /// (CodeMode / SceneName / Address), 以及触发"重新生成 AssemblyReference + ProjectFiles + Refresh"链路。
    /// 替代原 GlobalConfigWindow + GlobalConfigViewModel + GlobalConfigWindow.uxml 三件套。
    /// </summary>
    [UIAutoMenu("全局配置", 200, Scope = "loader")]
    public class GlobalConfigModule : BaseToolModule
    {
        private GlobalConfig _config;

        [LabelText("代码模式")]
        [OnValueChanged("Save")]
        [ShowInInspector]
        public CodeMode CodeMode
        {
            get => _config != null ? _config.CodeMode : CodeMode.Client;
            set { EnsureAsset().CodeMode = value; Save(); }
        }

        [LabelText("场景名")]
        [OnValueChanged("Save")]
        [ShowInInspector]
        public string SceneName
        {
            get => _config != null ? _config.SceneName : "";
            set { EnsureAsset().SceneName = value; Save(); }
        }

        [LabelText("服务器地址")]
        [OnValueChanged("Save")]
        [ShowInInspector]
        public string Address
        {
            get => _config != null ? _config.Address : "";
            set { EnsureAsset().Address = value; Save(); }
        }

        [LabelText("资产路径")]
        [ReadOnly]
        [ShowInInspector]
        public string AssetPath => GlobalConfigModel.AssetPath;

        [Button("重新生成 AssemblyReference", 10)]
        [GUIColor(0.53f, 0.95f, 0.72f)]
        public void Regenerate()
        {
            Debug.Log("[GlobalConfig] ========== 重新生成 AssemblyReference + ProjectFiles ==========");
            new GenerateAssemblyReferenceCommand(CodeMode).Execute();
            new RegenerateProjectFilesCommand().Execute();
            new RefreshAssetDatabaseCommand().Execute();
            Debug.Log("[GlobalConfig] ========== 重新生成完成 ==========");
        }

        public override void Initialize()
        {
            _config = new GlobalConfigModel().Config;
        }

        public override void OnDestroy()
        {
        }

        private GlobalConfig EnsureAsset()
        {
            if (_config == null) _config = new GlobalConfigModel().Config;
            return _config;
        }

        private void Save()
        {
            if (_config == null) return;
            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssets();
        }
    }
}

#endif
