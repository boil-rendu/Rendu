#if UNITY_EDITOR

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
    /// <summary>
    ///  宏
    /// </summary>
    [UIAutoMenu("宏设置", 200100)]
    public class UIMacroModule : BaseUIToolModule
    {
        [Button("文档", 30)]
        [PropertyOrder(-99999)]
        public void OpenDocument()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/F1cDwqKSliIiBIkWMwmcfsj6nUe");
        }

        [BoxGroup("宏", centerLabel: true)]
        [HideLabel]
        [EnumToggleButtons]
        [OnValueChanged("OnMacroTypeChanged")]
        public UIMacroType MacroType = UIMacroType.RD;

        [HideLabel]
        public BaseUIToolModule CurrentMacroModule;

        [HideLabel]
        private EnumPrefs<UIMacroType> UIMacroPrefs = new("UIAutoTool_UIMacroModule_MacroType", null, UIMacroType.Unity);

        private void OnMacroTypeChanged()
        {
            SelectMacroType(MacroType);
        }

        private void SelectMacroType(UIMacroType macroType)
        {
            switch (macroType)
            {
                case UIMacroType.RD:
                    CurrentMacroModule = new ETMacroModule();
                    break;
                case UIMacroType.Unity:
                    CurrentMacroModule = new UnityMacroModule();
                    break;
                default:
                    UnityTipsHelper.ShowError($"未知的宏类型:{macroType}");
                    CurrentMacroModule = null;
                    break;
            }

            if (CurrentMacroModule != null)
            {
                CurrentMacroModule.Initialize();
            }

            MacroType = macroType;
        }

        public override void Initialize()
        {
            SelectMacroType(UIMacroPrefs.Value);
        }

        public override void OnDestroy()
        {
            UIMacroPrefs.Value = MacroType;
        }
    }

    public enum UIMacroType
    {
        Unity,
        RD
    }
}
#endif
