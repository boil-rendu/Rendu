#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using RD;
using RD.Inspector;
using RD.Inspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    public class UIPublishModule : BaseUIToolModule
    {
        [ButtonGroup("文档")]
        [Button("UI文档", 50)]
        [PropertyOrder(-99999)]
        public void OpenDocument1()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/ES7Gwz4EAiVGKSkotY5cRbTznuh");
        }

        internal const string m_PublishName = "发布";
        internal const string m_PublishCommonName = "通用";
        internal const string m_PublishPackageName = "ET包";

        [FolderPath]
        [LabelText("所有模块资源路径")]
        [ReadOnly]
        [ShowInInspector]
        private string m_AllPkgPath = UIConstHelper.Const.UIProjectResPath;

        //所有ET模块资源路径
        private const string m_AllETPkgPath = "Packages/";

        [BoxGroup("创建模块", centerLabel: true)]
        [ShowInInspector]
        internal UICreateResModule CreateResModule = new UICreateResModule();

        private readonly Dictionary<string, List<string>> m_AllInfo = new();

        private int m_AllCount;

        private void RefreshAllPkg()
        {
            EditorHelper.CreateExistsDirectory(m_AllPkgPath);
            m_AllInfo.Clear();
            m_AllCount = 0;
            try
            {
                foreach (var uiPkg in Directory.GetDirectories(EditorHelper.GetProjPath(m_AllPkgPath)))
                {
                    if (!m_AllInfo.ContainsKey(m_AllPkgPath))
                    {
                        m_AllInfo.Add(m_AllPkgPath, new List<string>());
                    }

                    var list = m_AllInfo[m_AllPkgPath];
                    list.Add(uiPkg);
                }

                foreach (var packagePath in Directory.GetDirectories(EditorHelper.GetProjPath(m_AllETPkgPath)))
                {
                    var packageRes = $"{packagePath}/{m_AllPkgPath}";
                    if (Directory.Exists(packageRes))
                    {
                        var rdPackagesName = UIOperationHelper.GetRDPackagesName(packageRes, false);
                        if (string.IsNullOrEmpty(rdPackagesName))
                        {
                            Debug.LogError($"错误这里没有找到ET包名 请检查 {packageRes}");
                            continue;
                        }

                        var resPath = $"{m_AllETPkgPath}{UIConstHelper.Const.UIETPackagesFormat}{rdPackagesName}/{m_AllPkgPath}";
                        if (!m_AllInfo.ContainsKey(resPath))
                        {
                            m_AllInfo.Add(resPath, new List<string>());
                        }

                        var list = m_AllInfo[resPath];
                        foreach (var uiPkg in Directory.GetDirectories(packageRes))
                        {
                            list.Add(uiPkg);
                            m_AllCount++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"获取所有模块错误 请检查 err={e}{e.StackTrace}");
                return;
            }

            var commonPublish = $"{m_PublishName}/{m_PublishCommonName}";
            Tree.AddMenuItemAtPath(m_PublishName, new InspectorMenuItem(Tree, m_PublishCommonName, null)).AddIcon(InspectorIcons.Folder);
            var packagePublish = $"{m_PublishName}/{m_PublishPackageName}";
            Tree.AddMenuItemAtPath(m_PublishName, new InspectorMenuItem(Tree, m_PublishPackageName, null)).AddIcon(InspectorIcons.Folder);

            foreach ((var resPath, var listInfo) in m_AllInfo)
            {
                var rdPackagesName = UIOperationHelper.GetRDPackagesName(resPath, false);

                foreach (var folder in listInfo)
                {
                    var pkgName = Path.GetFileName(folder);
                    var upperName = NameUtility.ToFirstUpper(pkgName);
                    if (upperName != pkgName)
                    {
                        Debug.LogError($"这是一个非法的模块[ {pkgName} ]请使用统一方法创建模块 或者满足指定要求");
                        continue;
                    }

                    var showPkgName = string.IsNullOrEmpty(rdPackagesName) ? $"{pkgName}" : $"[{rdPackagesName}]{pkgName}";

                    var publishName = string.IsNullOrEmpty(rdPackagesName) ? $"{commonPublish}" : $"{packagePublish}";

                    var PublishPath = $"{publishName}/{showPkgName}";
                    var pkgMenu = new TreeMenuItem<UIPublishPackageModule>(this.AutoTool, this.Tree, PublishPath,
                        InspectorIcons.Folder);
                    pkgMenu.UserData = new UIPublishPackageModuleData
                    {
                        PublishModule = this,
                        PublishPath = PublishPath,
                        ResPath = resPath,
                        PkgName = pkgName
                    };
                }
            }
        }

        [GUIColor(0.4f, 0.8f, 1)]
        [Button("全部发布", 50)]
        [PropertyOrder(-99)]
        public void PublishAll()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;

            UnityTipsHelper.CallBack("确定发布全部UI?", () =>
            {
                var index = 0;
                var total = 0;
                try
                {
                    foreach ((var resPath, var listInfo) in m_AllInfo)
                    {
                        total += listInfo.Count;
                        foreach (var folder in listInfo)
                        {
                            index++;
                            var pkgName = Path.GetFileName(folder);
                            var upperName = NameUtility.ToFirstUpper(pkgName);
                            if (upperName != pkgName)
                            {
                                Debug.LogError($"这是一个非法的模块[ {pkgName} ]请使用统一方法创建模块 或者满足指定要求");
                                continue;
                            }

                            var module = new UIPublishPackageModule(this, resPath, pkgName);
                            module.PublishCurrent(false); //不要默认重置所有图集设置 有的图集真的会有独立设置
                            EditorHelper.DisplayProgressBar("发布", $"正在发布 {pkgName} ...", index, total);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"发布全部时发生错误: {e}");
                }
                finally
                {
                    EditorHelper.ClearProgressBar();
                    UnityTipsHelper.CallBackOk("UI全部 发布完毕", UIAutoTool.CloseWindowRefresh);
                }
            });
        }

        [ButtonGroup("Atlas")]
        [GUIColor(0f, 1f, 0.8f)]
        [Button("更新图集数据", 40)]
        [PropertyOrder(-98)]
        public void RefreshAtlasData()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;

            try
            {
                // TODO(RD): UIYooAssetsAtlasModule 所属的图集模块尚未迁入, 迁入后打开下方宏
#if RD_UI_ATLAS_MODULE
                UIYooAssetsAtlasModule.RefreshAtlasData();
#endif
            }
            catch (Exception e)
            {
                Debug.LogError($"更新图集数据时发生错误: {e}");
            }
            finally
            {
                UIAutoTool.CloseWindow();
            }
        }

        [ButtonGroup("Atlas")]
        [GUIColor(1f, 1f, 0.5f)]
        [Button("全部发布图集", 40)]
        [PropertyOrder(-98)]
        public void PublishAllAtlas()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;

            UnityTipsHelper.CallBack("确定发布全部UI图集?", () =>
            {
                var index = 0;
                var total = 0;
                try
                {
                    foreach ((var resPath, var listInfo) in m_AllInfo)
                    {
                        total += listInfo.Count;
                        foreach (var folder in listInfo)
                        {
                            index++;
                            var pkgName = Path.GetFileName(folder);
                            var upperName = NameUtility.ToFirstUpper(pkgName);
                            if (upperName != pkgName)
                            {
                                Debug.LogError($"这是一个非法的模块[ {pkgName} ]请使用统一方法创建模块 或者满足指定要求");
                                continue;
                            }

                            var module = new UIPublishPackageModule(this, resPath, pkgName);
                            module.CreateOrResetAtlas(true);
                            EditorHelper.DisplayProgressBar("发布图集", $"正在发布图集 {pkgName} ...", index, total);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"发布全部图集时发生错误: {e}");
                }
                finally
                {
                    EditorHelper.ClearProgressBar();
                    UnityTipsHelper.CallBackOk("UI全部图集 发布完毕", UIAutoTool.CloseWindowRefresh);
                }
            });
        }

        public override void Initialize()
        {
            this.RefreshAllPkg();
            CreateResModule?.Initialize();
        }

        public override void OnDestroy()
        {
            CreateResModule?.OnDestroy();
        }
    }
}
#endif