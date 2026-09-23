using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI资源绑定信息
    /// </summary>
    public struct UIBindVo
    {
        //当前UI类型
        public EUICodeType CodeType;

        //ET的类
        public Type ComponentType;

        //包名/模块名
        public string PkgName;

        //资源名
        public string ResName;

        //所在层级 如果不是panel则无效
        public EPanelLayer PanelLayer;
    }
}
