#if UNITY_EDITOR

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>生成 UI 的 Base 类代码（由 CreateBaseData 填充模板占位符，始终覆盖旧文件）</summary>
    public class CreateBaseCode : BaseTemplate
    {
        private         string m_EventName = "UI核心代码创建";
        /// <summary>生成事件名</summary>
        public override string EventName => m_EventName;

        /// <summary>是否覆盖同名文件</summary>
        public override bool Cover => true;

        private         bool m_AutoRefresh = false;
        /// <summary>生成后是否自动刷新</summary>
        public override bool AutoRefresh => m_AutoRefresh;

        private         bool m_ShowTips = false;
        /// <summary>是否弹出提示</summary>
        public override bool ShowTips => m_ShowTips;

        /// <summary>按 CreateBaseData 填充模板并创建文件，result 表示是否创建成功</summary>
        public CreateBaseCode(out bool result, string authorName, CreateBaseData codeData) : base(authorName)
        {
            var path = $"{string.Format(UIConstHelper.Const.ComponentGenPath, CreateModule.CreatePackageName)}/{codeData.PkgName}/{codeData.ResName}Base.cs";
            var template = $"{UIConstHelper.Const.TemplatePath}/UICreateBaseTemplate.txt";
            CreateVo = new CreateVo(template, path);

            m_EventName               = $"{codeData.ResName}Base 自动生成";
            m_AutoRefresh             = codeData.AutoRefresh;
            m_ShowTips                = codeData.ShowTips;
            ValueDic["Namespace"]     = codeData.Namespace;
            ValueDic["PkgName"]       = codeData.PkgName;
            ValueDic["ResName"]       = codeData.ResName;
            ValueDic["Variables"]     = codeData.Variables;
            ValueDic["UIBind"]        = codeData.UIBind;
            ValueDic["UIUnBind"]      = codeData.UIUnBind;
            ValueDic["VirtualMethod"] = codeData.VirtualMethod;
            ValueDic["PanelViewEnum"] = codeData.PanelViewEnum;

            result = CreateNewFile();
        }
    }
}
#endif
