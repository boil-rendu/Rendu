using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using RD.Init.Editor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 编译符号配置视图模型
    /// </summary>
    public class DefineViewModel : ViewModelBase
    {
        private string _currentDefines;
        private List<string> _definesList;

        public string CurrentDefines
        {
            get => _currentDefines;
            set => SetProperty(ref _currentDefines, value);
        }

        public DefineViewModel()
        {
            LoadDefines();
        }

        /// <summary>
        /// 加载编译符号
        /// </summary>
        public void LoadDefines()
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                BuildTargetGroup.Standalone);

            CurrentDefines = defines;
            _definesList = new List<string>();

            if (!string.IsNullOrEmpty(defines))
            {
                string[] split = defines.Split(';');
                foreach (var s in split)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        _definesList.Add(s);
                    }
                }
            }
        }

        /// <summary>
        /// 保存编译符号
        /// </summary>
        public void SaveDefines()
        {
            string defines = string.Join(";", _definesList);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                BuildTargetGroup.Standalone, defines);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("保存成功", "编译符号已保存，建议重新编译项目。", "确定");
        }

        /// <summary>
        /// 检查编译符号是否已启用
        /// </summary>
        public bool IsDefineEnabled(string define)
        {
            return _definesList.Contains(define);
        }

        /// <summary>
        /// 添加编译符号
        /// </summary>
        public void AddDefine(string define)
        {
            if (!_definesList.Contains(define))
            {
                _definesList.Add(define);
                UpdateCurrentDefines();
            }
        }

        /// <summary>
        /// 移除编译符号
        /// </summary>
        public void RemoveDefine(string define)
        {
            if (_definesList.Contains(define))
            {
                _definesList.Remove(define);
                UpdateCurrentDefines();
            }
        }

        private void UpdateCurrentDefines()
        {
            CurrentDefines = string.Join(";", _definesList);
        }
    }
}
