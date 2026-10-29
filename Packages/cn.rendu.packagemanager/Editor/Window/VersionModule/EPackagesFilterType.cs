using System;
using RD.Draw;

namespace RD.PackageManager.Editor
{
    /// <summary>
    /// 包列表的筛选维度（Flags 位标记）。
    /// 每个枚举值对应"一个过滤条件"，配合 <see cref="EPackagesFilterOperationType"/> 决定多条条件如何组合。
    /// 注意值不是连续位：<see cref="All"/> 单独占 1&lt;&lt;30，<see cref="None"/> = 1 作为"空选择"基准，
    /// 其余 RD/Update/Req/Ban/ReBan 才是真正的 1&lt;&lt;1..5 位。
    /// </summary>
    [Flags]
    public enum EPackagesFilterType
    {
        /// <summary>全部包（不过滤）。</summary>
        [LabelText("全部")]
        All = 1 << 30,

        /// <summary>未选任何条件（基线，值为 1 便于"唯一"模式做差值运算）。</summary>
        [LabelText("无")]
        None = 1,

        /// <summary>RD 自研包（包名含 cn.rendu.）。</summary>
        [LabelText("RD包")]
        RD = 1 << 1,

        /// <summary>存在可更新新版本（CanUpdateVersion）。</summary>
        [LabelText("更新")]
        Update = 1 << 2,

        /// <summary>有"请求最新版本"按钮可点的包。</summary>
        [LabelText("请求")]
        Req = 1 << 3,

        /// <summary>已禁用请求（IsBan）的包。</summary>
        [LabelText("禁用")]
        Ban = 1 << 4,

        /// <summary>已禁用、需要解禁的包。</summary>
        [LabelText("解禁")]
        ReBan = 1 << 5,
    }

    /// <summary>
    /// 多个筛选条件的组合方式：
    /// <list type="bullet">
    ///   <item><see cref="Only"/>：单选，自动取消其它已选项（靠 FilterType 差值运算实现，见 OnFilterTypeChanged）。</item>
    ///   <item><see cref="Or"/>：满足条件之一即显示。</item>
    ///   <item><see cref="And"/>：需同时满足所有已选条件。</item>
    /// </list>
    /// </summary>
    public enum EPackagesFilterOperationType
    {
        [LabelText("唯一")]
        Only = 0,

        [LabelText("或")]
        Or = 1,

        [LabelText("与")]
        And = 2,
    }
}
