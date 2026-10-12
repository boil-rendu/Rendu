using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RD.Panel;
using RD.Panel.Editor;
using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>
    /// AutoTool 的「预制体检查」面板
    /// 用正则从全工程 Prefab 路径里筛出 UI 分包下的资源（UI/模块名/Prefabs），
    /// 按筛选条件列出结果并支持一键删除
    /// </summary>
    public class CheckPrefabModule: BaseToolModule
    {
        /// <summary>全量检查：扫描所有 UI 分包下的 Prefab 并刷新表格</summary>
        [GUIColor(0.4f, 0.8f, 1)]
        [Button("检查", 50)]
        [PropertyOrder(-99)]
        public void CheckAll()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;
            InitGetAll();
        }

        /// <summary>检查结果的筛选方式（默认只显示可删除的 CDE）</summary>
        [BoxGroup("检查筛选", centerLabel: true)]
        [HideLabel]
        [EnumToggleButtons]
        [OnValueChanged("OnValueChangedFiltrate")]
        [PropertyOrder(-98)]
        public ECheckPrefabFiltrate Filtrate = ECheckPrefabFiltrate.CDE;

        /// <summary>筛选方式变化时立即刷新表格</summary>
        private void OnValueChangedFiltrate()
        {
            UpdateFiltrate();
        }

        /// <summary>删除当前筛选结果中所有「可删除」的预制体（带进度条与结果提示）</summary>
        [GUIColor(1, 0, 0)]
        [Button("删除所有", 50)]
        [PropertyOrder(-88)]
        [ShowIf("ShowIfDeleteAll")]
        public void DeleteAll()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;

            List<CheckPrefabData> deleteVo = new();
            foreach (var bind in m_FiltrateDatas)
            {
                var result = bind.ShowIfIgonre();
                if (result)
                {
                    deleteVo.Add(bind);
                }
            }

            var count = deleteVo.Count;
            for (int index = 0; index < deleteVo.Count; index++)
            {
                var bind = deleteVo[index];
                bind.Delete(false, false);
                EditorUtility.DisplayProgressBar("同步信息", $"删除资源信息 {bind.ResName}", index * 1.0f / count);
            }

            EditorUtility.ClearProgressBar();

            UnityTipsHelper.Show($"删除成功: {count}个相关文件");

            AutoTool.CloseWindowRefresh();
        }

        /// <summary>「删除所有」按钮的显示条件：检查结果非空且其中存在可删除项</summary>
        private bool ShowIfDeleteAll()
        {
            if (m_CheckPrefabs is not { Count: > 0 }) return false;

            foreach (var bind in m_CheckPrefabs)
            {
                var result = bind.ShowIfDelete();
                if (result)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>按当前筛选条件过滤后的检查结果（只读表格数据源）</summary>
        [TableList(DrawScrollView = true, IsReadOnly = true)]
        [BoxGroup("检查结果", centerLabel: true)]
        [HideLabel]
        [ShowInInspector]
        [HideReferenceObjectPicker]
        private List<CheckPrefabData> m_FiltrateDatas = new();

        /// <summary>全量检查结果</summary>
        private List<CheckPrefabData> m_CheckPrefabs = new();

        /// <summary>模块加载（无需初始化）</summary>
        public override void Initialize()
        {
        }

        /// <summary>模块销毁（无需清理）</summary>
        public override void OnDestroy()
        {
        }

        /// <summary>
        /// 路径命中「UI/模块名/Prefabs」时给出模块名, 否则返回 null。
        /// 分隔符 <c>/</c> 与 <c>\</c> 都认: AssetDatabase 给的是 <c>/</c>, 而 Path.GetFullPath 在 Windows 上给的是 <c>\</c> ——
        /// 只认其中一种的话, 同一个工程在 Windows 上"有结果"、在 macOS/Linux 上永远零条。
        /// 段名按整段比较(不再用正则的子串匹配), 于是 MyUI/Lobby/Prefabs 这类不会被误收。
        /// </summary>
        internal static string UIPrefabPkgName(string path)
        {
            const string rootFolderName   = "UI";
            const string prefabFolderName = "Prefabs";

            if (string.IsNullOrEmpty(path)) return null;

            var parts = path.Replace('\\', '/').Split('/');
            for (var i = 0; i + 2 < parts.Length; i++)
            {
                if (parts[i].Equals(rootFolderName, StringComparison.OrdinalIgnoreCase) == false) continue;
                if (parts[i + 2].Equals(prefabFolderName, StringComparison.OrdinalIgnoreCase) == false) continue;

                return parts[i + 1];
            }

            return null;
        }

        /// <summary>
        /// 扫描全工程 Prefab，路径落在「UI/模块名/Prefabs」下的才纳入检查结果
        /// （模块名取自路径、资源名取自文件名），最后刷新表格
        /// </summary>
        private void InitGetAll()
        {
            m_CheckPrefabs.Clear();
            var allPrefabs = AssetDatabase.FindAssets("t:Prefab", null);
            var allCount   = allPrefabs.Length;
            for (int index = 0; index < allPrefabs.Length; index++)
            {
                var guid = allPrefabs[index];
                var path = AssetDatabase.GUIDToAssetPath(guid);

                var pkgName = UIPrefabPkgName(path);
                if (pkgName != null)
                {
                    var fileName = Path.GetFileNameWithoutExtension(path);
                    m_CheckPrefabs.Add(new CheckPrefabData(path, pkgName, fileName));
                    EditorUtility.DisplayProgressBar("同步信息", $"检查 {pkgName},{fileName}", index * 1.0f / allCount);
                }
            }

            UpdateFiltrate();
            EditorUtility.ClearProgressBar();
        }

        /// <summary>按 Filtrate 把全量结果过滤进表格：全部 / 可删除 / 可删除的 CDE / 可删除的 Prefab / 忽略</summary>
        public void UpdateFiltrate()
        {
            m_FiltrateDatas.Clear();

            switch (Filtrate)
            {
                case ECheckPrefabFiltrate.All:
                    m_FiltrateDatas.AddRange(m_CheckPrefabs);
                    break;
                case ECheckPrefabFiltrate.Delete:
                    foreach (var bind in m_CheckPrefabs)
                    {
                        var result = bind.ShowIfIgonre();
                        if (result)
                        {
                            m_FiltrateDatas.Add(bind);
                        }
                    }

                    break;

                case ECheckPrefabFiltrate.CDE:

                    foreach (var bind in m_CheckPrefabs)
                    {
                        var result = bind.ShowIfIgonre() && bind.IsCDETable;
                        if (result)
                        {
                            m_FiltrateDatas.Add(bind);
                        }
                    }

                    break;

                case ECheckPrefabFiltrate.Prefab:

                    foreach (var bind in m_CheckPrefabs)
                    {
                        var result = bind.ShowIfIgonre() && !bind.IsCDETable;
                        if (result)
                        {
                            m_FiltrateDatas.Add(bind);
                        }
                    }

                    break;

                case ECheckPrefabFiltrate.Igonre:
                    foreach (var bind in m_CheckPrefabs)
                    {
                        var result = bind.ShowIfReIgonre();
                        if (result)
                        {
                            m_FiltrateDatas.Add(bind);
                        }
                    }

                    break;
                default:
                    Debug.LogError($"未知的Filtrate类型: {Filtrate}");
                    break;
            }
        }
    }
}