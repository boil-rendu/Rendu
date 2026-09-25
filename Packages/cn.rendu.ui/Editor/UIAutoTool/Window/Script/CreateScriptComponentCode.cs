using System;
using UnityEngine;

#if UNITY_EDITOR

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    public class CreateScriptComponentCode : BaseTemplate
    {
        private         string m_EventName = "RD-Component 代码创建";
        public override string EventName => m_EventName;

        public override bool Cover => false;

        private         bool m_AutoRefresh = false;
        public override bool AutoRefresh => m_AutoRefresh;

        private         bool m_ShowTips = false;
        public override bool ShowTips => m_ShowTips;

        public CreateScriptComponentCode(out bool result, string authorName, CreateScriptData codeData) : base(authorName)
        {
            var componentType = codeData.ComponentTpye.ToString();
            var path          = $"{codeData.ComponentPath}/{codeData.Name}{componentType}.cs";
            var template      = $"{UIConstHelper.Const.TemplatePath}/Script/UICreateScriptCommonTemplate.txt";
            CreateVo = new CreateVo(template, path);

            m_EventName               = $"{codeData.Name} RD-Component 自动生成";
            m_AutoRefresh             = codeData.AutoRefresh;
            m_ShowTips                = codeData.ShowTips;
            ValueDic["Namespace"]     = codeData.Namespace;
            ValueDic["Name"]          = codeData.Name;
            ValueDic["Desc"]          = codeData.Desc;
            ValueDic["ComponentType"] = componentType;
            ValueDic["Life"]          = GetLife(codeData);

            result = CreateNewFile();
        }

        private string GetLife(CreateScriptData codeData)
        {
            var life = "";
            foreach (EScriptLifeType lifeEnum in Enum.GetValues(typeof(EScriptLifeType)))
            {
                if (codeData.LifeTpye.HasFlag(lifeEnum))
                {
                    life += SwitchLife(lifeEnum);
                }
            }

            return life;
        }

        private string SwitchLife(EScriptLifeType life)
        {
            switch (life)
            {
                case EScriptLifeType.All:
                    break;
                case EScriptLifeType.Def:
                    break;
                case EScriptLifeType.None:
                    break;
                case EScriptLifeType.IAwake:
                    return ", IAwake";
                case EScriptLifeType.IUpdate:
                    return ", IUpdate";
                case EScriptLifeType.IDestroy:
                    return ", IDestroy";
                default:
                    Debug.LogError($"是否新增了类型 请检查 {life}");
                    break;
            }

            return "";
        }
    }
}
#endif