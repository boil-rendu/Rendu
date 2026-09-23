using System;
using UnityEditor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    /// <summary>
    /// 包输入字段变更处理器 - 处理用户输入变更并触发相应的更新逻辑
    /// 优化：使用防抖机制避免频繁调用耗时操作
    /// </summary>
    public class PackageInputChangedHandler
    {
        private readonly PackageViewModel _viewModel;
        private string _lastInput;
        private string _lastFormalName;

        // 防抖计时器
        private string _pendingInput;
        private string _pendingFormalName;
        private double _lastUpdateTime;
        private const double DebounceDelay = 0.3; // 防抖延迟（秒）

        /// <summary>
        /// 当数据加载完成时触发的事件
        /// </summary>
        public event Action OnDataLoaded;

        public PackageInputChangedHandler(PackageViewModel viewModel)
        {
            _viewModel = viewModel;
            _lastInput = viewModel.SimpleName;
            _lastFormalName = viewModel.FormalName;

            // 注册编辑器更新回调用于防抖
            EditorApplication.update += Update;
        }

        ~PackageInputChangedHandler()
        {
            EditorApplication.update -= Update;
        }

        /// <summary>
        /// 编辑器更新回调（用于防抖）
        /// </summary>
        private void Update()
        {
            if (_lastUpdateTime > 0 && EditorApplication.timeSinceStartup - _lastUpdateTime >= DebounceDelay)
            {
                bool dataLoaded = false;

                // 处理待处理的输入变更
                if (!string.IsNullOrEmpty(_pendingInput))
                {
                    Debug.Log($"[PackageInputChangedHandler] Processing input change: {_pendingInput}");

                    // 尝试加载包信息,如果包不存在则重置为默认值
                    if (!_viewModel.LoadPackageInfoFromSimpleName())
                    {
                        Debug.Log($"[PackageInputChangedHandler] Package not found, resetting to defaults");
                        _viewModel.ResetToDefaults();
                    }
                    _lastInput = _pendingInput;
                    _pendingInput = null;
                    dataLoaded = true;
                }

                // 处理待处理的 FormalName 变更
                if (!string.IsNullOrEmpty(_pendingFormalName))
                {
                    Debug.Log($"[PackageInputChangedHandler] Processing formal name change: {_pendingFormalName}");

                    // 尝试加载包信息,如果包不存在则重置为默认值
                    if (!_viewModel.LoadPackageInfoFromFormalName())
                    {
                        Debug.Log($"[PackageInputChangedHandler] Package not found, resetting to defaults");
                        _viewModel.ResetToDefaults();
                    }
                    _lastFormalName = _pendingFormalName;
                    _pendingFormalName = null;
                    dataLoaded = true;
                }

                _lastUpdateTime = 0;

                // 触发 UI 更新事件
                if (dataLoaded)
                {
                    OnDataLoaded?.Invoke();
                }
            }
        }

        /// <summary>
        /// 处理 Package Name 输入变更（带防抖）
        /// </summary>
        public void HandleInputChanged(string newValue)
        {
            // 立即更新值，延迟执行耗时操作
            if (newValue != _lastInput)
            {
                _viewModel.SimpleName = newValue;
                _viewModel.UpdateFromSimpleName();
                _pendingInput = newValue;
                _lastUpdateTime = EditorApplication.timeSinceStartup;
                _lastInput = newValue;
            }
        }

        /// <summary>
        /// 处理 Formal Name 输入变更（带防抖）
        /// </summary>
        public void HandleFormalNameChanged(string newValue)
        {
            if (newValue != _lastFormalName)
            {
                _pendingFormalName = newValue;
                _lastUpdateTime = EditorApplication.timeSinceStartup;
                _lastFormalName = newValue;
            }
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset()
        {
            _lastInput = _viewModel.SimpleName;
            _lastFormalName = _viewModel.FormalName;
            _pendingInput = null;
            _pendingFormalName = null;
            _lastUpdateTime = 0;
        }
    }
}
