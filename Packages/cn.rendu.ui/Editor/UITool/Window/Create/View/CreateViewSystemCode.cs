#if UNITY_EDITOR

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>
    /// 生成 View 的 System 代码：文件不存在才新建，已存在则按 OverrideDic 做增量覆盖
    /// </summary>
    public class CreateViewSystemCode : BaseTemplate
    {
        private         string m_EventName = "RD-System 代码创建";
        /// <summary>生成事件名</summary>
        public override string EventName => m_EventName;

        /// <summary>是否覆盖同名文件（此处为 false，改用 OverrideDic 增量覆盖）</summary>
        public override bool Cover => false;

        private         bool m_AutoRefresh = false;
        /// <summary>生成后是否自动刷新</summary>
        public override bool AutoRefresh => m_AutoRefresh;

        private         bool m_ShowTips = false;
        /// <summary>是否弹出提示</summary>
        public override bool ShowTips => m_ShowTips;

        /// <summary>
        /// 按 CreateSystemData 填充模板：文件不存在先新建，随后用 OverrideDic 做增量覆盖；
        /// OverrideDic 为 null 时直接返回 true
        /// </summary>
        public CreateViewSystemCode(out bool result, string authorName, CreateSystemData codeData) : base(authorName)
        {
            var path = $"{string.Format(UIConstHelper.Const.SystemPath, CreateModule.CreatePackageName)}/{codeData.PkgName}/{codeData.ResName}ComponentSystem.cs";
            var template = $"{UIConstHelper.Const.TemplatePath}/UICreateViewSystemTemplate.txt";
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
