#if UNITY_EDITOR

using System;
using RD.Panel;
using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    [Flags]
    public enum EScriptLifeType
    {
        [LabelText("所有")]
        All = -1,

        [LabelText("默认")]
        Def = IAwake | IDestroy,

        [LabelText("无")]
        None = 0,

        [LabelText("IAwake")]
        IAwake = 1 << 1,

        [LabelText("IUpdate")]
        IUpdate = 1 << 2,

        [LabelText("IDestroy")]
        IDestroy = 1 << 3,
    }

    public enum EScriptComponentType
    {
        Component = 0,
        Child     = 1,
    }

    /// <summary>
    ///  自动生成脚本
    /// </summary>
    [UIAutoMenu("代码生成", 200000)]
    public class ScriptModule : BaseToolModule
    {
        [Button("文档", 30)]
        [PropertyOrder(-99999)]
        public void OpenDocument()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/XKHYwD7Ppi95f6kqz7IcijmKnQg");
        }

        [HideLabel]
        [BoxGroup("名称")]
        public string ComponentName;

        [HideLabel]
        [BoxGroup("描述")]
        public string ComponentDesc;

        [EnumToggleButtons, HideLabel]
        [BoxGroup("组件类型")]
        public EScriptComponentType ComponentType = EScriptComponentType.Component;

        [EnumToggleButtons, HideLabel]
        [BoxGroup("生命周期")]
        public EScriptLifeType LifeType = EScriptLifeType.Def;

        private const string ParentFolderPath = "Assets/";

        [BoxGroup("Component路径")]
        [HideLabel]
        [FolderPath(Parent = ParentFolderPath)]
        public string ComponentPath;

        private StringPrefs ComponentPathPrefs = new StringPrefs("UIScriptModule_ComponentPath", null, "Assets/Scripts/Codes/Model");

        [BoxGroup("System路径")]
        [HideLabel]
        [FolderPath(Parent = ParentFolderPath)]
        public string SystemPath;

        private StringPrefs SystemPathPrefs = new StringPrefs("UIScriptModule_SystemPath", null, "Assets/Scripts/Codes/Hotfix");

        [Button("生成", 50), GUIColor(0.4f, 0.8f, 1)]
        private void CreateCode()
        {
            if (string.IsNullOrEmpty(ComponentName))
            {
                UnityTipsHelper.ShowError("必须输入名称");
                return;
            }

            ComponentName = NameUtility.ToFirstUpper(ComponentName);

            var data = new CreateScriptData
            {
                Namespace     = GetNamespace(),
                Name          = ComponentName,
                Desc          = ComponentDesc,
                ComponentTpye = this.ComponentType,
                LifeTpye      = this.LifeType,
                ComponentPath = ParentFolderPath + ComponentPath,
                SystemPath    = ParentFolderPath + SystemPath,
            };

            new CreateScriptComponentCode(out var resultComponent, AutoTool.Author, data);
            if (resultComponent)
            {
                if ((int)this.LifeType != 0)
                {
                    new CreateScriptSystemCode(out var resultSystem, AutoTool.Author, data);
                }
            }

            AutoTool.CloseWindowRefresh();
        }

        private string GetNamespace()
        {
            string scriptNamespace = "RD";
            if (ComponentPath.Contains("Client") || ComponentPath.Contains("HotfixView") || ComponentPath.Contains("ModelView"))
            {
                scriptNamespace += ".Client";
            }
            else if (ComponentPath.Contains("Server"))
            {
                scriptNamespace += ".Server";
            }

            return scriptNamespace;
        }

        public override void Initialize()
        {
            base.Initialize();
            ComponentPath = ComponentPathPrefs.Value;
            SystemPath    = SystemPathPrefs.Value;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            ComponentPathPrefs.Value = ComponentPath;
            SystemPathPrefs.Value    = SystemPath;
        }
    }
}
#endif
