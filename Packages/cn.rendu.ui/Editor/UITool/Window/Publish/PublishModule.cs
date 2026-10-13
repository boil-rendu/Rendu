#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using RD;
using RD.Panel;
using RD.Panel.Editor;
using RD.EditorWindows.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>
    /// UITool 的「发布」模块：扫描工程内所有 UI 模块，按「通用 / 模块包」两棵子树挂出发布菜单
    /// 支持整体发布、整体发布图集，并内嵌「创建模块」子面板
    /// </summary>
    public class PublishModule : BaseToolModule
    {
        /// <summary>打开 UI 发布说明文档</summary>
        [ButtonGroup("文档")]
        [Button("UI文档", 50)]
        [PropertyOrder(-99999)]
        public void OpenDocument1()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/ES7Gwz4EAiVGKSkotY5cRbTznuh");
        }

        /// <summary>发布菜单根节点名</summary>
        internal const string m_PublishName = "发布";
        /// <summary>「通用」发布子节点名（非分包 UI）</summary>
        internal const string m_PublishCommonName = "通用";
        /// <summary>「模块包」发布子节点名（分包 UI）</summary>
        internal const string m_PublishPackageName = "模块包";

        /// <summary>所有模块资源路径（只读展示）</summary>
        [FolderPath]
        [LabelText("所有模块资源路径")]
        [ReadOnly]
        [ShowInInspector]
        private string m_AllPkgPath = UIConstHelper.Const.ProjectResPath;

        //所有分包资源根路径
        /// <summary>所有分包资源根路径（Packages/）</summary>
        private const string m_AllPackageRootPath = "Packages/";

        /// <summary>内嵌的「创建模块」子面板</summary>
        [BoxGroup("创建模块", centerLabel: true)]
        [ShowInInspector]
        internal CreateResModule CreateResModule = new CreateResModule();

        /// <summary>扫描结果：资源根路径 → 该路径下的全部模块目录</summary>
        private readonly Dictionary<string, List<string>> m_AllInfo = new();

        /// <summary>分包内模块总数</summary>
        private int m_AllCount;

        /// <summary>
        /// 扫描两处来源：Assets 下的模块目录，以及每个分包内的同名目录（跳过取不到包名的分包）
        /// 随后在工具树上挂出「通用 / 模块包」两棵子树，为每个模块创建 PublishPackageModule 菜单项
        /// 目录名必须首字母大写，否则视为非法模块
        /// </summary>
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

                foreach (var packagePath in Directory.GetDirectories(EditorHelper.GetProjPath(m_AllPackageRootPath)))
                {
                    var packageRes = $"{packagePath}/{m_AllPkgPath}";
                    if (Directory.Exists(packageRes))
                    {
                        var rdPackagesName = UIOperationHelper.GetRDPackagesName(packageRes, false);
                        if (string.IsNullOrEmpty(rdPackagesName))
                        {
                            Debug.LogError($"错误这里没有找到分包包名 请检查 {packageRes}");
                            continue;
                        }

                        var resPath = $"{m_AllPackageRootPath}{UIConstHelper.Const.PackagesFormat}{rdPackagesName}/{m_AllPkgPath}";
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
            Tree.AddMenuItemAtPath(m_PublishName, new MenuTreeItem(Tree, m_PublishCommonName, null)).AddIcon(Icons.Folder);
            var packagePublish = $"{m_PublishName}/{m_PublishPackageName}";
            Tree.AddMenuItemAtPath(m_PublishName, new MenuTreeItem(Tree, m_PublishPackageName, null)).AddIcon(Icons.Folder);

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
                    var pkgMenu = new TreeMenuItem<PublishPackageModule>(this.Window, this.Tree, PublishPath,
                        Icons.Folder);
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

        /// <summary>
        /// 二次确认后发布全部模块：逐个调用 PublishCurrent(false)
        /// （false 表示不重置图集设置，避免覆盖图集的独立配置），全程带进度条，结束弹提示并刷新窗口
        /// </summary>
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

                            var module = new PublishPackageModule(this, resPath, pkgName);
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
                    UnityTipsHelper.CallBackOk("UI全部 发布完毕", UITool.CloseWindowRefresh);
                }
            });
        }

        /// <summary>刷新图集数据（图集模块未迁入时为空实现，由 RD_UI_ATLAS_MODULE 宏控制），完成后关闭窗口</summary>
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
                UITool.CloseWindow();
            }
        }

        /// <summary>
        /// 二次确认后发布全部模块的图集：逐个调用 CreateOrResetAtlas(true)
        /// （与「全部发布」的差别在于这里会重置图集设置），全程带进度条
        /// </summary>
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

                            var module = new PublishPackageModule(this, resPath, pkgName);
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
                    UnityTipsHelper.CallBackOk("UI全部图集 发布完毕", UITool.CloseWindowRefresh);
                }
            });
        }

        /// <summary>模块加载：扫描全部模块并初始化内嵌的创建模块面板</summary>
        public override void Initialize()
        {
            this.RefreshAllPkg();
            CreateResModule?.Initialize();
        }

        /// <summary>模块销毁：同步销毁内嵌的创建模块面板</summary>
        public override void OnDestroy()
        {
            CreateResModule?.OnDestroy();
        }
    }
}
#endif