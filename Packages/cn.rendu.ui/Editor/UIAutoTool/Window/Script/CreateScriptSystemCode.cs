using System;
using UnityEngine;

#if UNITY_EDITOR

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>
    /// 按自定义脚本名生成 RD-System 代码（不覆盖已有文件）
    /// 与 Component 版不同：生命周期不是拼接口名，而是按标志位展开成完整的 EntitySystem 方法体
    /// </summary>
    public class CreateScriptSystemCode : BaseTemplate
    {
        private         string m_EventName = "RD-System 代码创建";
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
        public CreateScriptSystemCode(out bool result, string authorName, CreateScriptData codeData) : base(authorName)
        {
            var componentType = codeData.ComponentTpye.ToString();
            var path          = $"{codeData.SystemPath}/{codeData.Name}{componentType}System.cs";
            var template      = $"{UIConstHelper.Const.TemplatePath}/Script/UICreateScriptSystemTemplate.txt";
            CreateVo = new CreateVo(template, path);

            m_EventName               = $"{codeData.Name} RD-System 自动生成";
            m_AutoRefresh             = codeData.AutoRefresh;
            m_ShowTips                = codeData.ShowTips;
            ValueDic["Namespace"]     = codeData.Namespace;
            ValueDic["Name"]          = codeData.Name;
            ValueDic["Desc"]          = codeData.Desc;
            ValueDic["ComponentType"] = componentType;
            ValueDic["ObjectSystem"]  = GetLife(codeData);

            result = CreateNewFile();
        }

        /// <summary>按位枚举 LifeTpye，逐项展开成 EntitySystem 方法（经 SbPool 拼接，非空才换行）</summary>
        private string GetLife(CreateScriptData codeData)
        {
            var sbA = SbPool.Get();
            foreach (EScriptLifeType lifeEnum in Enum.GetValues(typeof(EScriptLifeType)))
            {
                if (codeData.LifeTpye.HasFlag(lifeEnum))
                {
                    var contentSystem = SwitchLife(codeData, lifeEnum);
                    if (!string.IsNullOrEmpty(contentSystem))
                    {
                        sbA.Append(contentSystem);
                        sbA.AppendLine();
                    }
                }
            }

            return SbPool.PutAndToStr(sbA);
        }

        /// <summary>单个生命周期标志 → EntitySystem 方法文本；All/Def/None 不产出内容</summary>
        private string SwitchLife(CreateScriptData codeData, EScriptLifeType life)
        {
            var componentType = codeData.ComponentTpye.ToString();
            switch (life)
            {
                case EScriptLifeType.All:
                    break;
                case EScriptLifeType.Def:
                    break;
                case EScriptLifeType.None:
                    break;
                case EScriptLifeType.IAwake:
                    return string.Format(lifeTemp, codeData.Name, "Awake", componentType);
                case EScriptLifeType.IUpdate:
                    return string.Format(lifeTemp, codeData.Name, "Update", componentType);
                case EScriptLifeType.IDestroy:
                    return string.Format(lifeTemp, codeData.Name, "Destroy", componentType);
                default:
                    Debug.LogError($"是否新增了类型 请检查 {life}");
                    break;
            }

            return "";
        }

        /// <summary>EntitySystem 方法模板（占位符依次为 类名 / 生命周期名 / 组件类型）</summary>
        private const string lifeTemp = @"
        [EntitySystem]
        private static void {1}(this {0}{2} self)
        {{
        }}";
    }
}
#endif