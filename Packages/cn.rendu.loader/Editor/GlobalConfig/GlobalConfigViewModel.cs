using RD.Init.Editor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 全局配置视图模型
    /// </summary>
    public class GlobalConfigViewModel : ViewModelBase
    {
        private readonly GlobalConfigModel _model;

        public GlobalConfigViewModel(GlobalConfigModel model)
        {
            _model = model ?? throw new System.ArgumentNullException(nameof(model));
        }

        public CodeMode CodeMode
        {
            get => _model.CodeMode;
            set
            {
                if (_model.CodeMode != value)
                {
                    _model.SetCodeMode(value);
                    OnPropertyChanged();
                    OnCodeModeChanged();
                }
            }
        }

        public string SceneName
        {
            get => _model.SceneName;
            set
            {
                if (_model.SceneName != value)
                {
                    _model.SetSceneName(value);
                    OnPropertyChanged();
                    OnSceneNameChanged();
                }
            }
        }

        public string Address
        {
            get => _model.Address;
            set
            {
                if (_model.Address != value)
                {
                    _model.SetAddress(value);
                    OnPropertyChanged();
                    OnAddressChanged();
                }
            }
        }

        private void OnCodeModeChanged()
        {
            _model.SaveAsset();
        }

        private void OnSceneNameChanged()
        {
            _model.SaveAsset();
        }

        private void OnAddressChanged()
        {
            _model.SaveAsset();
        }

        /// <summary>
        /// 从资源重新加载配置
        /// </summary>
        public void LoadConfig()
        {
            _model.LoadAsset();
            OnPropertyChanged("CodeMode");
            OnPropertyChanged("SceneName");
            OnPropertyChanged("Address");
        }

        public void ReGenerateAssemblyReference()
        {
            UnityEngine.Debug.Log("[GlobalConfig] ========== 重新生成 AssemblyReference 和 ProjectFiles ==========");
            CreateGenerateAssemblyReferenceCommand(_model.CodeMode).Execute();
            new RegenerateProjectFilesCommand().Execute();
            new RefreshAssetDatabaseCommand().Execute();
            UnityEngine.Debug.Log("[GlobalConfig] ========== 重新生成完成 ==========");
        }

        private GenerateAssemblyReferenceCommand CreateGenerateAssemblyReferenceCommand(CodeMode codeMode)
        {
            return new GenerateAssemblyReferenceCommand(codeMode);
        }
    }
}
