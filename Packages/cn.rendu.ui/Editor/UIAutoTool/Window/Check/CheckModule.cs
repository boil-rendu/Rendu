using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RD.Inspector;
using RD.Inspector.Editor;
using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    [UIAutoMenu("检查", int.MaxValue)]
    public class CheckModule : BaseToolModule
    {
        [Button("文档", 30)]
        [PropertyOrder(-99999)]
        public void OpenDocument()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/GOBCwA1rdi7nBhkaBh8cLCjRnah");
        }

        [BoxGroup("检查类型", centerLabel: true)]
        [HideLabel]
        [EnumToggleButtons]
        public ECheckViewType CheckViewType = ECheckViewType.Script;

        [HideLabel]
        [ShowIf("CheckViewType", ECheckViewType.Script)]
        public CheckScriptModule CheckScript = new();

        [HideLabel]
        [ShowIf("CheckViewType", ECheckViewType.Prefab)]
        public CheckPrefabModule CheckPrefab = new();

        [HideLabel]
        private RD.Inspector.Editor.EnumPrefs<ECheckViewType> UICheckViewPrefs = new("UIAutoTool_EUICheckViewType");

        public override void Initialize()
        {
            CheckViewType = UICheckViewPrefs.Value;
        }

        public override void OnDestroy()
        {
            UICheckViewPrefs.Value = CheckViewType;
        }
    }
}