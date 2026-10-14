using RD.EditTool.Editor;

namespace YooAsset.Editor
{
    /// <summary>
    /// YooConfig 视图模型层：封装模型属性，变更时自动保存
    /// </summary>
    public class YooConfigViewModel : ViewModelBase
    {
        private readonly YooConfigModel _model;

        public YooConfigViewModel(YooConfigModel model)
        {
            _model = model ?? throw new System.ArgumentNullException(nameof(model));
        }

        public EPlayMode EPlayMode
        {
            get => _model.EPlayMode;
            set
            {
                if (_model.EPlayMode != value)
                {
                    _model.SetEPlayMode(value);
                    OnPropertyChanged();
                    _model.SaveAsset();
                }
            }
        }

        public bool EditorForceRedownload
        {
            get => _model.EditorForceRedownload;
            set
            {
                if (_model.EditorForceRedownload != value)
                {
                    _model.SetEditorForceRedownload(value);
                    OnPropertyChanged();
                    _model.SaveAsset();
                }
            }
        }

        public int EditorSimulateDownloadSpeed
        {
            get => _model.EditorSimulateDownloadSpeed;
            set
            {
                if (_model.EditorSimulateDownloadSpeed != value)
                {
                    _model.SetEditorSimulateDownloadSpeed(value);
                    OnPropertyChanged();
                    _model.SaveAsset();
                }
            }
        }

        public string Url
        {
            get => _model.Url;
            set
            {
                if (_model.Url != value)
                {
                    _model.SetUrl(value);
                    OnPropertyChanged();
                    _model.SaveAsset();
                }
            }
        }

        public bool HostForceClearSandbox
        {
            get => _model.HostForceClearSandbox;
            set
            {
                if (_model.HostForceClearSandbox != value)
                {
                    _model.SetHostForceClearSandbox(value);
                    OnPropertyChanged();
                    _model.SaveAsset();
                }
            }
        }

        /// <summary>
        /// 从资源重新加载配置
        /// </summary>
        public void LoadConfig()
        {
            _model.LoadAsset();
            OnPropertyChanged("EPlayMode");
            OnPropertyChanged("EditorForceRedownload");
            OnPropertyChanged("EditorSimulateDownloadSpeed");
            OnPropertyChanged("Url");
            OnPropertyChanged("HostForceClearSandbox");
        }
    }
}
