using System;
using System.Collections.Generic;
using System.Reflection;
using RD.Inspector;
using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    using Object = UnityEngine.Object;
    public class UICheckScriptModule: BaseUIToolModule
    {
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

        [BoxGroup("检查筛选", centerLabel: true)]
        [HideLabel]
        [EnumToggleButtons]
        [OnValueChanged("OnValueChangedFiltrate")]
        [PropertyOrder(-98)]
        public EUICheckScriptFiltrate Filtrate = EUICheckScriptFiltrate.Delete;

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

            List<UICheckScriptData> deleteVo = new();
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

            UIAutoTool.CloseWindowRefresh();
        }

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

        [TableList(DrawScrollView = true, IsReadOnly = true)]
        [BoxGroup("检查结果", centerLabel: true)]
        [HideLabel]
        [ShowInInspector]
        [HideReferenceObjectPicker]
        private List<UICheckScriptData> m_FiltrateScriptDatas = new();

        private List<UICheckScriptData> m_CheckScriptDatas = new();

        public override void Initialize()
        {
        }

        public override void OnDestroy()
        {
        }

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

        private static bool GetBindVo(out UICheckScriptData bindScriptData,
                                      UIAttribute           attribute,
                                      Type                    componentType)
        {
            bindScriptData = new UICheckScriptData();
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

        public void UpdateFiltrate()
        {
            m_FiltrateScriptDatas.Clear();

            switch (Filtrate)
            {
                case EUICheckScriptFiltrate.All:
                    m_FiltrateScriptDatas.AddRange(m_CheckScriptDatas);
                    break;
                case EUICheckScriptFiltrate.Delete:
                    foreach (var bind in m_CheckScriptDatas)
                    {
                        var result = bind.ShowIfIgonre();
                        if (result)
                        {
                            m_FiltrateScriptDatas.Add(bind);
                        }
                    }

                    break;
                case EUICheckScriptFiltrate.Igonre:
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