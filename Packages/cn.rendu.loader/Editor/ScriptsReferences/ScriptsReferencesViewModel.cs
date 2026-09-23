using RD.Init.Editor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 脚本引用视图模型
    /// </summary>
    public class ScriptsReferencesViewModel : ViewModelBase
    {
        private bool _isProcessing;

        public ScriptsReferencesViewModel()
        {
            _isProcessing = false;
        }

        public ScriptsReferencesViewModel(object model) : this()
        {
            // Model 参数保留用于扩展，当前版本未使用
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        /// <summary>
        /// 更新脚本引用
        /// </summary>
        public void UpdateScriptsReferences()
        {
            if (_isProcessing)
            {
                Debug.LogWarning("UpdateScriptsReferences is already running");
                return;
            }

            IsProcessing = true;

            try
            {
                new UpdateScriptsReferencesCommand().Execute();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"UpdateScriptsReferences failed: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// 生成脚本引用
        /// </summary>
        public void GenerateReferences()
        {
            if (_isProcessing)
            {
                Debug.LogWarning("GenerateReferences is already running");
                return;
            }

            IsProcessing = true;

            try
            {
                // ScriptsReferencesCommand 是静态类，直接调用静态方法
                ScriptsReferencesCommand.UpdateScriptsReferences();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"GenerateReferences failed: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// 更新引用（别名）
        /// </summary>
        public void UpdateReferences()
        {
            UpdateScriptsReferences();
        }

        /// <summary>
        /// 显示当前配置
        /// </summary>
        public void ShowCurrentConfig()
        {
            Debug.Log("当前脚本引用配置已显示在控制台");
            // 可以在这里添加显示具体配置的逻辑
        }
    }
}
