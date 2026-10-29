using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    /// <summary>
    /// 包创建窗口(IMGUI 版) - 与 PackageCreateModule 同构, 仅渲染层不同:
    /// 数据/逻辑全部走 PackageViewModel, 输入变更交给 PackageInputChangedHandler(带防抖)。
    /// </summary>
    public class RDPackageCreateWindow : EditorWindow
    {

        
        [MenuItem("RD/PackageManager/包创建", false, 100)]
        private static void OpenWindow()
        {
            var window = GetWindow<RDPackageCreateWindow>();
            if (window != null)
                window.Show();
        }

        public static void CloseWindow()
        {
            GetWindow<RDPackageCreateWindow>()?.Close();
        }

        private PackageViewModel            _viewModel;
        private PackageInputChangedHandler _inputChangedHandler;
        private bool                       _forceUpdate;

        private void OnEnable()
        {
            ResetState();
        }

        private void OnDisable()
        {
            _inputChangedHandler?.Unregister();
            _inputChangedHandler = null;
        }

        private void ResetState()
        {
            _inputChangedHandler?.Unregister();
            _inputChangedHandler = null;

            _viewModel = new PackageViewModel();

            // 当前 Unity 版本比默认版本新时, 用当前版本(与模块一致)
            var current = FormatUnityVersion(Application.unityVersion);
            if (CompareUnityVersions(current, _viewModel.Info.Required.Unity) > 0)
            {
                _viewModel.Unity = current;
            }

            _inputChangedHandler = new PackageInputChangedHandler(_viewModel);
            _inputChangedHandler.OnDataLoaded += Repaint;

            _forceUpdate = false;
        }

        private void OnGUI()
        {
            if (_viewModel == null) return;

            EditorGUILayout.LabelField("提示: 输入包名(简写)后, 若已存在同名包会自动载入其信息; 需要成体系浏览请用 RD 自绘检视器");
            GUILayout.Space(8);

            // 包名(简写) - 变更交给处理器(防抖, 包已存在时载入信息)
            var newSimple = EditorGUILayout.TextField("包名(简写)", _viewModel.SimpleName);
            if (newSimple != _viewModel.SimpleName)
            {
                _inputChangedHandler.HandleInputChanged(newSimple);
            }

            // 正式包名 / 显示名 - 只读(由包名或已载入信息派生)
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("正式包名", _viewModel.FormalName);
            EditorGUILayout.TextField("显示名", _viewModel.DisplayName);
            EditorGUI.EndDisabledGroup();

            _viewModel.Version              = EditorGUILayout.TextField("版本", _viewModel.Version);
            _viewModel.Unity                = EditorGUILayout.TextField("Unity 版本", _viewModel.Unity);
            _viewModel.AsmDefType           = (EAssemblyDefineType)EditorGUILayout.EnumFlagsField("Assembly 定义类型", _viewModel.AsmDefType);
            _viewModel.AsmRefType           = (EAssemblyReferenceType)EditorGUILayout.EnumFlagsField("Assembly 引用类型", _viewModel.AsmRefType);
            _viewModel.CodeMode             = (ECodeModeType)EditorGUILayout.EnumFlagsField("代码模式", _viewModel.CodeMode);
            _viewModel.GenerateIgnoreAsmdef = EditorGUILayout.Toggle("生成 Ignore Asmdef", _viewModel.GenerateIgnoreAsmdef);

            _viewModel.Description = EditorGUILayout.TextField("描述", _viewModel.Description);
            _viewModel.AuthorName  = EditorGUILayout.TextField("作者", _viewModel.AuthorName);
            _viewModel.AuthorEmail = EditorGUILayout.TextField("邮箱", _viewModel.AuthorEmail);
            _viewModel.AuthorUrl   = EditorGUILayout.TextField("主页", _viewModel.AuthorUrl);
            _viewModel.Keywords    = EditorGUILayout.TextField("关键字", _viewModel.Keywords);

            _forceUpdate = EditorGUILayout.Toggle("强制更新(覆盖已存在的包)", _forceUpdate);

            GUILayout.Space(10);
            if (GUILayout.Button("创建 / 更新包", GUILayout.Height(40)))
            {
                CreateOrUpdate();
            }
        }

        private void CreateOrUpdate()
        {
            var validation = _viewModel.ValidateCreatePackage();
            if (!validation.IsValid)
            {
                UnityTipsHelper.ShowError(validation.ErrorMessage);
                return;
            }

            var packagePath = $"Packages/{_viewModel.FormalName}";
            if (Directory.Exists(packagePath) && !_forceUpdate)
            {
                UnityTipsHelper.ShowError(
                    $"Package '{_viewModel.FormalName}' 已存在.\n\n请启用'强制更新'选项来覆盖现有包。");
                return;
            }

            UnityTipsHelper.CallBack(
                $"确定要{(_forceUpdate ? "更新" : "创建")}包 '{_viewModel.FormalName}' 吗?",
                () =>
                {
                    var createInfo = new PackageCreateInfo(_viewModel.Info)
                    {
                        PackagePath = packagePath,
                        PackageName = _viewModel.SimpleName,
                    };
                    PackageCreateHelper.CreatePackage(createInfo, force: _forceUpdate);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    UnityTipsHelper.Show($"创建成功 [ {_viewModel.FormalName} ]");
                });
        }

        // 当前 Unity 版本比默认版本新时, 用当前版本(复制自 PackageCreateModule)
        private static string FormatUnityVersion(string version)
        {
            if (string.IsNullOrEmpty(version)) return version;
            var tIndex = version.IndexOf('t');
            var formatted = tIndex > 0 ? version.Substring(0, tIndex) : version;
            var parts = formatted.Split('.');
            return parts.Length >= 2 ? $"{parts[0]}.{parts[1]}" : formatted;
        }

        private static int CompareUnityVersions(string v1, string v2)
        {
            if (string.IsNullOrEmpty(v1)) return -1;
            if (string.IsNullOrEmpty(v2)) return 1;

            var p1 = v1.Split('.');
            var p2 = v2.Split('.');
            var maxLength = Mathf.Max(p1.Length, p2.Length);
            for (var i = 0; i < maxLength; i++)
            {
                var n1 = i < p1.Length && int.TryParse(p1[i], out var a) ? a : 0;
                var n2 = i < p2.Length && int.TryParse(p2[i], out var b) ? b : 0;
                if (n1 > n2) return 1;
                if (n1 < n2) return -1;
            }
            return 0;
        }
    }
}
