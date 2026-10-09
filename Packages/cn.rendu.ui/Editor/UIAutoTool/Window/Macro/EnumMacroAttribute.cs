using System;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>枚举宏标记：标注在枚举类型上，表示该枚举参与编辑器宏（Macro）配置</summary>
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
    public class EnumMacroAttribute : Attribute
    {
    }
}
