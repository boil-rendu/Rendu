using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using RD.Inspector;
using RD.Inspector.Editor;
using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    public class CheckPrefabModule: BaseToolModule
    {
        [GUIColor(0.4f, 0.8f, 1)]
        [Button("检查", 50)]
        [PropertyOrder(-99)]
        public void CheckAll()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;
            InitGetAll();
        }

        [BoxGroup("检查筛选", centerLabel: true)]
        [HideLabel]
        [EnumToggleButtons]
        [OnValueChanged("OnValueChangedFiltrate")]
        [PropertyOrder(-98)]
        public ECheckPrefabFiltrate Filtrate = ECheckPrefabFiltrate.CDE;

        private void OnValueChangedFiltrate()
        {
            UpdateFiltrate();
        }

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

        [TableList(DrawScrollView = true, IsReadOnly = true)]
        [BoxGroup("检查结果", centerLabel: true)]
        [HideLabel]
        [ShowInInspector]
        [HideReferenceObjectPicker]
        private List<CheckPrefabData> m_FiltrateDatas = new();

        private List<CheckPrefabData> m_CheckPrefabs = new();

        public override void Initialize()
        {
        }

        public override void OnDestroy()
        {
        }

        private readonly string m_Pattern = @"UI\\[^\\]*\\Prefabs";

        private void InitGetAll()
        {
            m_CheckPrefabs.Clear();
            var allPrefabs = AssetDatabase.FindAssets("t:Prefab", null);
            var allCount   = allPrefabs.Length;
            for (int index = 0; index < allPrefabs.Length; index++)
            {
                var guid = allPrefabs[index];
                var path = AssetDatabase.GUIDToAssetPath(guid);

                var normalizedPath = Path.GetFullPath(path);

                var match = Regex.Match(normalizedPath, m_Pattern, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    var pkgName     = match.Value.Split('\\')[1];
                    var fileNameAll = Path.GetFileName(path);
                    var fileName    = fileNameAll.Split('.')[0];
                    m_CheckPrefabs.Add(new CheckPrefabData(path, pkgName, fileName));
                    EditorUtility.DisplayProgressBar("同步信息", $"检查 {pkgName},{fileName}", index * 1.0f / allCount);
                }
            }

            UpdateFiltrate();
            EditorUtility.ClearProgressBar();
        }

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