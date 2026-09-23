using Microsoft.CodeAnalysis;

namespace RD
{
    public static class UIEntitySystemAnalyzerRule
    {
        private const string Title = "UI Entity类存在未实现的事件函数";

        private const string MessageFormat = "UI Entity类: {0} 存在未实现的事件函数";

        private const string Description = "UI Entity类存在未实现的事件函数.";

        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.UIEntitySystemAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.UIModel,
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }

    public static class UIEntitySystemMethodNeedSystemOfAttrAnalyzerRule
    {
        private const string Title = "UI EntitySystem标签只能添加在含有EntitySystemOf标签的静态类中";

        private const string MessageFormat = "UI 方法:{0}的{1}标签只能添加在含有{2}标签的静态类中";

        private const string Description = "UI EntitySystem标签只能添加在含有EntitySystemOf标签的静态类中.";

        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.UIEntitySystemMethodNeedSystemOfAttrAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.UIModel,
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }

}
