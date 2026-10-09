using System;
using UnityEngine;

#if UNITY_EDITOR

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>
    /// 按自定义脚本名生成 RD-Component 代码（不覆盖已有文件）
    /// 生命周期标志位会拼成接口列表传入模板
    /// </summary>
    public class CreateScriptComponentCode : BaseTemplate
    {
        private         string m_EventName = "RD-Component 代码创建";
        /// <summary>生成事件名</summary>
        public override string EventName => m_EventName;

        /// <summary>是否覆盖同名文件（false，保留手写逻辑）</summary>
        public override bool Cover => false;

        private         bool m_AutoRefresh = false;
        /// <summary>生成后是否自动刷新</summary>
        public override bool AutoRefresh => m_AutoRefresh;

        private         bool m_ShowTips = false;
        /// <summary>是否弹出提示</summary>
        public override bool ShowTips => m_ShowTips;

        /// <summary>按 CreateScriptData 填充模板并创建文件，result 表示是否创建成功</summary>
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

        /// <summary>按位枚举 LifeTpye，拼出要实现的全部生命周期接口（逗号分隔形式）</summary>
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

        /// <summary>单个生命周期标志 → 接口名片段；All/Def/None 不产出内容</summary>
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