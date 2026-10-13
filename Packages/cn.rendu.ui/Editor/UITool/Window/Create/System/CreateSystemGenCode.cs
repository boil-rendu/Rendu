#if UNITY_EDITOR

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>生成 System 的 Gen 代码（含 UIFriend/UIBase 等模板占位符，始终覆盖旧文件）</summary>
    public class CreateSystemGenCode : BaseTemplate
    {
        private         string m_EventName = "RD-System Gen 自动生成";
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
        public CreateSystemGenCode(out bool result, string authorName, CreateBaseData codeData) : base(authorName)
        {
            var path = $"{string.Format(UIConstHelper.Const.SystemGenPath, CreateModule.CreatePackageName)}/{codeData.PkgName}/{codeData.ResName}ComponentSystemGen.cs";
            var template = $"{UIConstHelper.Const.TemplatePath}/UICreateSystemGenTemplate.txt";
            CreateVo = new CreateVo(template, path);

            m_EventName               = $"{codeData.ResName} RD-System Gen 自动生成";
            m_AutoRefresh             = codeData.AutoRefresh;
            m_ShowTips                = codeData.ShowTips;
            ValueDic["Namespace"]     = codeData.Namespace;
            ValueDic["PkgName"]       = codeData.PkgName;
            ValueDic["ResName"]       = codeData.ResName;
            ValueDic["Variables"]     = codeData.Variables;
            ValueDic["UIFriendOf"]    = codeData.UIFriend;
            ValueDic["UIBase"]        = codeData.UIBase;
            ValueDic["UIBind"]        = codeData.UIBind;
            ValueDic["UIUnBind"]      = codeData.UIUnBind;
            ValueDic["VirtualMethod"] = codeData.VirtualMethod;
            ValueDic["PanelViewEnum"] = codeData.PanelViewEnum;
            ValueDic["CodeType"]      = codeData.CodeType.ToString();

            result = CreateNewFile();
        }
    }
}
#endif
