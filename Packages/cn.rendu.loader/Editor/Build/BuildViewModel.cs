using RD.Init.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 构建视图模型
    /// </summary>
    public class BuildViewModel : ViewModelBase
    {
        private PlatformType _platformType;
        private BuildOptions _buildOptions;
        private readonly GlobalConfig _globalConfig;

        public BuildViewModel(BuildSettings settings)
        {
            _globalConfig = settings.GlobalConfig;
            ActivePlatform = settings.ActivePlatform;
            _platformType = ActivePlatform;
        }

        public PlatformType PlatformType
        {
            get => _platformType;
            set => SetProperty(ref _platformType, value);
        }

        public BuildOptions BuildOptions
        {
            get => _buildOptions;
            set => SetProperty(ref _buildOptions, value);
        }

        public PlatformType ActivePlatform { get; private set; }

        public GlobalConfig GlobalConfig => _globalConfig;

        /// <summary>
        /// 构建包
        /// </summary>
        public void BuildPackage()
        {
            if (_platformType == PlatformType.None)
            {
                Debug.LogError("[Build] 请选择平台!");
                return;
            }

            if (_globalConfig.CodeMode != CodeMode.Client)
            {
                Debug.LogError("[Build] 构建 CodeMode 必须是 CodeMode.Client，请选择 Client");
                return;
            }

            Debug.Log($"[Build] ========== 开始构建: 平台={_platformType}, 选项={_buildOptions} ==========");

            if (_platformType != ActivePlatform)
            {
                switch (EditorDialogHelper.DisplayDialogComplex("警告!",
                    $"当前平台是 {ActivePlatform}，切换到 {_platformType} 可能需要较长时间",
                    "切换", "取消", "不切换"))
                {
                    case 0:
                        ActivePlatform = _platformType;
                        break;
                    case 1:
                        return;
                    case 2:
                        _platformType = ActivePlatform;
                        OnPropertyChanged(nameof(PlatformType));
                        break;
                }
            }

            new BuildApplicationCommand(_platformType, _buildOptions).Execute();
            Debug.Log($"[Build] ========== 构建完成: {_platformType} ==========");
        }
    }
}
