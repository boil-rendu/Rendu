using System.Runtime.CompilerServices;

/// <summary>
/// 程序集级可见性声明
/// 向同包内的 RD.UI.Editor / RD.ModelView / RD.HotfixView 开放本程序集的 internal 成员
/// （这三者需要直接访问框架内部实现：实体系统、对象池、UIBase 工具等）
/// </summary>
[assembly: InternalsVisibleTo("RD.UI.Editor")]
[assembly: InternalsVisibleTo("RD.ModelView")]
[assembly: InternalsVisibleTo("RD.HotfixView")]
