using UnityEditor;
using RD.Draw.Editor.Metadata;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// "把一个成员 / 一个对象画出来"的**回调宿主** —— 各渲染器互相回画时依赖的窄接口。
    ///
    /// 回调面收成一个接口: 需要"往回画一个成员"的人只依赖这一个契约。
    ///
    /// 叫 Host 而不是 Drawer, 是为了与 <see cref="IMemberRenderer"/> 区分清楚:
    /// <c>IMemberRenderer</c> 是链上的一档(它**画**某个成员), 本接口是"往回画"的**去处**
    /// —— 它自己不画任何东西, 只把请求转回编排者。两个概念混用同一个词, 正是当初
    /// "这个成员到底由谁画"这条知识说不清的原因之一。
    ///
    /// 它同时是**递归绘制**的入口: 容器展开、InlineEditor 展开、表格单元格里的成员,
    /// 都要回到"按成员的规则画"这件事上, 走的就是这个接口。
    /// </summary>
    internal interface IMemberDrawHost
    {
        /// <summary>绘制一个成员(按成员种类分发: 按钮 / 回调方法 / 值成员)。</summary>
        void DrawMember(object target, CachedMember member, EditorWindow host, int depth);

        /// <summary>绘制一个成员的值(表格单元格内用, 标签由表头承担)。</summary>
        void DrawValueMember(object target, CachedMember member, EditorWindow host, int depth, bool inTableCell);

        /// <summary>把一个对象当作子对象展开绘制(嵌套对象 / [InlineEditor])。</summary>
        void DrawNested(object value, EditorWindow host, int depth, object parent, bool allowUnityObject);
    }
}
