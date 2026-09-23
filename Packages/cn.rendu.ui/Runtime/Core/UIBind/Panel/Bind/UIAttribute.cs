using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 用于标记
    /// ET组件的关系 >> 关联 >> UIBase
    /// </summary>
    /// <remarks>
    /// RD.UI.BindSourceGenerator（源码见 cn.rendu.ui/DotNet~）按本特性的类型全名扫描 UI 组件类，
    /// 生成 UICodeGenerated.UIBindProvider。若改类名，需同步生成器里的 AttributeNames。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class UIAttribute : BaseAttribute
    {
        //组件类型
        public EUICodeType UICodeType { get; }

        //层级类型 如果不是panel则无效
        public EPanelLayer UIPanelLayer { get; }

        public UIAttribute(EUICodeType codeType, EPanelLayer panelLayer = EPanelLayer.Any)
        {
            UICodeType   = codeType;
            UIPanelLayer = panelLayer;
        }
    }
}
