namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 折叠状态("这一块现在展开还是收起")的**唯一**读写入口:
    ///   读会话 → 画 Foldout → 与读到的值比较 → 变了就 RequestRepaint → 写回会话。
    /// 漏写"比较后请求重绘"在保留模式()下会表现为"点了三角没反应"
    /// ( 每帧重画则看不出差别)。规则只写在这里一次, 新增容器时不必再记一遍。
    ///
    /// 键仍然是成员名(同一宿主对象分桶, 见 <see cref="HostSession"/>);
    /// 分组展开用 GroupID 路径、走的是另一套 API, 见 <see cref="Rendering.GroupRenderer"/>。
    /// </summary>
    internal static class CollapseState
    {
        /// <summary>
        /// 产树版: 产出折叠头节点 + 记录状态回调, 返回**当前帧**的展开态(调用方据此决定是否产子树)。
        ///
        /// 与 <see cref="Draw"/> 的差异: 解释与产树分离后, "用户点了三角"要到渲染完才知道,
        /// 因此折叠态的写回放进回调, 由 <see cref="ChangeApplier"/> 在检测到状态变化时触发。
        /// </summary>
        public static bool Build(DrawContext ctx, object owner, string key, bool fallback, string label,
                                 WidgetBuilder builder)
        {
            var current = ctx.Session.GetToggle(owner, key, fallback);
            builder.Foldout(current, label, next =>
            {
                ctx.Session.SetToggle(owner, key, next);
            });
            return current;
        }
    }
}
