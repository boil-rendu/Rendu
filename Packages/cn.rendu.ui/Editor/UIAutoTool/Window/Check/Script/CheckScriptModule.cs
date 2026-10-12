using System;
using System.Collections.Generic;
using System.Reflection;
using RD.Panel;
using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    using Object = UnityEngine.Object;
    /// <summary>
    /// AutoTool 的「脚本检查」面板
    /// 扫描全部带 UIAttribute 的 UI 类型，反射读出 PkgName/ResName 等字段生成检查项，
    /// 再按筛选条件把结果列到表格里，并支持一键删除多余文件
    /// </summary>
    public class CheckScriptModule: BaseToolModule
    {
        /// <summary>全量检查：刷新资源信息后逐个检查，并把结果按当前筛选条件刷新到表格</summary>
        [GUIColor(0.4f, 0.8f, 1)]
        [Button("检查", 50)]
        [PropertyOrder(-99)]
        public void CheckAll()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;

            EditorUtility.DisplayProgressBar("同步信息", $"刷新资源信息", 0);

            InitGetAll();

            var count = m_CheckScriptDatas.Count;
            for (int index = 0; index < m_CheckScriptDatas.Count; index++)
            {
                var bind = m_CheckScriptDatas[index];
                bind.UpdatCheck();
                EditorUtility.DisplayProgressBar("同步信息", $"检查 {bind.ComponentType.Name}", index * 1.0f / count);
            }

            UpdateFiltrate();
            EditorUtility.ClearProgressBar();
        }

        /// <summary>检查结果的筛选方式（默认只显示可删除项）</summary>
        [BoxGroup("检查筛选", centerLabel: true)]
        [HideLabel]
        [EnumToggleButtons]
        [OnValueChanged("OnValueChangedFiltrate")]
        [PropertyOrder(-98)]
        public ECheckScriptFiltrate Filtrate = ECheckScriptFiltrate.Delete;

        /// <summary>筛选方式变化时立即刷新表格</summary>
        private void OnValueChangedFiltrate()
        {
            UpdateFiltrate();
        }

        /// <summary>删除当前筛选结果中所有「可删除」的脚本文件（带进度条与二次确认提示）</summary>
        [GUIColor(1, 0, 0)]
        [Button("删除所有", 50)]
        [PropertyOrder(-88)]
        [ShowIf("ShowIfDeleteAll")]
        public void DeleteAll()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;

            List<CheckScriptData> deleteVo = new();
            foreach (var bind in m_FiltrateScriptDatas)
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
                bind.DeleteScript(false, false);
                EditorUtility.DisplayProgressBar("同步信息", $"删除资源信息 {bind.ComponentType.Name}", index * 1.0f / count);
            }

            EditorUtility.ClearProgressBar();

            UnityTipsHelper.Show($"删除成功: {count}个相关文件");

            AutoTool.CloseWindowRefresh();
        }

        /// <summary>「删除所有」按钮的显示条件：筛选结果非空且其中存在可删除项</summary>
        private bool ShowIfDeleteAll()
        {
            if (m_FiltrateScriptDatas is not { Count: > 0 }) return false;

            foreach (var bind in m_FiltrateScriptDatas)
            {
                var result = bind.ShowIfDeleteScript();
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
        private List<CheckScriptData> m_FiltrateScriptDatas = new();

        /// <summary>全量检查结果</summary>
        private List<CheckScriptData> m_CheckScriptDatas = new();

        /// <summary>模块加载（无需初始化）</summary>
        public override void Initialize()
        {
        }

        /// <summary>模块销毁（无需清理）</summary>
        public override void OnDestroy()
        {
        }

        /// <summary>遍历全部非抽象类型，挑出带 UIAttribute 且能读出 PkgName/ResName 的 UI 类型作为检查项</summary>
        private void InitGetAll()
        {
            var types = RD.Client.AssemblyHelper.GetAllTypes();

            m_CheckScriptDatas.Clear();

            foreach (var type in types)
            {
                if (type.IsAbstract) continue;
                var attribute = type.GetCustomAttribute<UIAttribute>();
                if (attribute == null) continue;
                if (GetBindVo(out var bindVo, attribute, type))
                {
                    m_CheckScriptDatas.Add(bindVo);
                }
            }
        }

        /// <summary>
        /// 由类型与 UIAttribute 组装一条检查数据；
        /// 读不到 PkgName/ResName 字段时返回 false，Panel 却把层级设成 Any 会额外报错
        /// </summary>
        private static bool GetBindVo(out CheckScriptData bindScriptData,
                                      UIAttribute           attribute,
                                      Type                    componentType)
        {
            bindScriptData = new CheckScriptData();
            if (componentType == null ||
                !componentType.GetFieldValue("PkgName", out bindScriptData.PkgName) ||
                !componentType.GetFieldValue("ResName", out bindScriptData.ResName))
            {
                return false;
            }

            bindScriptData.ComponentType = componentType;
            bindScriptData.CodeType      = attribute.UICodeType;
            bindScriptData.PanelLayer    = attribute.UIPanelLayer;
            if (bindScriptData is { CodeType: EUICodeType.Panel, PanelLayer: EPanelLayer.Any })
            {
                Debug.LogError($"{componentType.Name} 错误的设定 既然是Panel 那必须设定所在层级 不能是Any 请检查重新导出");
            }

            return true;
        }

        /// <summary>按 Filtrate 把全量结果过滤进表格：全部 / 可删除 / 忽略</summary>
        public void UpdateFiltrate()
        {
            m_FiltrateScriptDatas.Clear();

            switch (Filtrate)
            {
                case ECheckScriptFiltrate.All:
                    m_FiltrateScriptDatas.AddRange(m_CheckScriptDatas);
                    break;
                case ECheckScriptFiltrate.Delete:
                    foreach (var bind in m_CheckScriptDatas)
                    {
                        var result = bind.ShowIfIgonre();
                        if (result)
                        {
                            m_FiltrateScriptDatas.Add(bind);
                        }
                    }

                    break;
                case ECheckScriptFiltrate.Igonre:
                    foreach (var bind in m_CheckScriptDatas)
                    {
                        var result = bind.ShowIfReIgonre();
                        if (result)
                        {
                            m_FiltrateScriptDatas.Add(bind);
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