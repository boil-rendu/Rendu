#if UNITY_EDITOR

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    public class UICreateCommonSystemCode : BaseTemplate
    {
        private         string m_EventName = "RD-System 代码创建";
        public override string EventName => m_EventName;

        public override bool Cover => false;

        private         bool m_AutoRefresh = false;
        public override bool AutoRefresh => m_AutoRefresh;

        private         bool m_ShowTips = false;
        public override bool ShowTips => m_ShowTips;

        public UICreateCommonSystemCode(out bool result, string authorName, UICreateSystemData codeData) : base(authorName)
        {
            var path = $"{string.Format(UIConstHelper.Const.UIETSystemPath, UICreateModule.UICreatePackageName)}/{codeData.PkgName}/{codeData.ResName}ComponentSystem.cs";
            var template = $"{UIConstHelper.Const.UITemplatePath}/UICreateCommonSystemTemplate.txt";
            CreateVo = new CreateVo(template, path);

            m_EventName           = $"{codeData.ResName} RD-System 自动生成";
            m_AutoRefresh         = codeData.AutoRefresh;
            m_ShowTips            = codeData.ShowTips;
            ValueDic["Namespace"] = codeData.Namespace;
            ValueDic["PkgName"]   = codeData.PkgName;
            ValueDic["ResName"]   = codeData.ResName;

            if (!TemplateEngine.FileExists(CreateVo.SavePath))
            {
                result = CreateNewFile();
            }

            if (codeData.OverrideDic == null)
            {
                result = true;
                return;
            }

            result = OverrideCheckCodeFile(codeData.OverrideDic);
        }
    }
}
#endif
