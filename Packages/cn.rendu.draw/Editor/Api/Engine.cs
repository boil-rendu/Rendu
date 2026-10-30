using System;
using UnityEditor;
using UnityEngine;
using RD.Draw.Editor.Rendering;
using RD.Draw.Editor.Rules;
using RD.Draw.Editor.Tables;

namespace RD.Draw.Editor
{
    /// <summary>
    /// 绘制引擎的**对外门面** —— 宿主唯一需要认识的类型。
    ///
    /// 依据反射 + Runtime 层特性即时绘制任意 C# 对象:
    ///   - <see cref="DrawObject(object, EditorWindow, Action{object}, Action{object})"/> 画一个对象;
    ///   - <see cref="CreateHostState"/> 拿一份宿主状态句柄(折叠/滚动互不串台);
    ///   - <see cref="SetDiagnosticSink"/> 接管排障日志。
    ///
    /// 门面之外的实际结构:
    ///   - 绘制编排在 <see cref="InspectorRenderer"/>(状态走 <see cref="DrawContext"/>);
    ///   - 反射/分组/取值/判定等规则各自成类, 由 <see cref="EngineServices"/> 装配并持有缓存;
    ///   - 引擎里没有任何进程级的可变绘制状态(没有"当前后端/当前会话/当前在不在单元格里")。
    ///
    /// 这里刻意**只有绘制入口**, 不提供"给内部转发用"的静态代理: 规则层的调用方直接依赖
    /// <see cref="EngineServices"/>, 门面上挂二十几个 <c>Engine.GetXxx</c> 只会把服务定位器的
    /// 味道搬到最外层, 并让真正的依赖方向反过来(内层 → 门面 → 服务)。
    /// </summary>
    public static class Engine
    {
        /// <summary>宿主未显式提供会话时的兜底, 使引擎可脱离宿主单独使用(也便于测试)。</summary>
        private static readonly HostSession s_DefaultSession = new();

        /// <summary>
        /// 接管表格几何诊断的输出(排障用: 横向滚动条从哪来, 靠对比盒宽/格宽/滚动区宽)。
        ///
        /// 默认输出到 Unity Console 且只在数值变化时打一次。宿主可以换成自己的实现 ——
        /// 接进日志窗口、写文件、或者在测试里做断言 —— 不必改引擎源码。
        /// 传 null 恢复默认。
        ///
        /// 换 sink 不会影响绘制: 它只在"确实要输出诊断"时被调用。
        /// </summary>
        public static void SetDiagnosticSink(IDiagnosticSink sink)
        {
            EngineServices.Shared.Diagnostics = sink ?? new TableGeometrySink();
        }

        //-------------------------------------- 对外入口 --------------------------------------

        /// <summary>
        /// 绘制任意对象。
        /// host 为 EditorWindow 时表示窗口自绘(可绘制窗口自身); 传 null 表示宿主是 UnityEditor.Editor,
        /// 此时根对象允许是 MonoBehaviour / ScriptableObject —— 但其成员里的 Unity 对象仍不会被内窥, 除非标了 [InlineEditor]。
        /// onBeforeChange: 数据即将写入(供宿主记录 Undo); onChanged: 数据已变化(供宿主标记脏 / 重绘)。
        /// </summary>
        public static void DrawObject(object target, EditorWindow host,
                                      Action<object> onBeforeChange = null,
                                      Action<object> onChanged = null)
        {
            DrawObject(target, host, onBeforeChange, onChanged, (HostState)null);
        }

        /// <summary>
        /// 绘制任意对象, 并显式指定宿主状态句柄(由 <see cref="CreateHostState"/> 创建)。
        /// 每个宿主各持一份句柄, 折叠 / 滚动状态互不串台; 传 null 时走默认会话。
        /// 宿主可以位于其它程序集(例如独立的窗口包) —— HostState 不透明, 内部实现不外泄。
        /// </summary>
        public static void DrawObject(object target, EditorWindow host,
                                      Action<object> onBeforeChange,
                                      Action<object> onChanged,
                                      HostState state)
        {
            DrawObject(target, host, onBeforeChange, onChanged, state?.Session);
        }

        /// <summary>
        /// 绘制任意对象, 并显式指定本次绘制使用的后端(<see cref="Driver"/> /
        /// 自己的实现)。不传时按宿主的 <c>rootVisualElement</c> 直接挂一个 <see cref="Driver"/>。
        /// </summary>
        public static void DrawObject(object target, EditorWindow host,
                                      Action<object> onBeforeChange,
                                      Action<object> onChanged,
                                      Driver driver)
        {
            DrawObject(target, host, onBeforeChange, onChanged, (HostState)null, driver);
        }

        /// <summary>绘制任意对象: 显式指定状态句柄 + 绘制后端。</summary>
        public static void DrawObject(object target, EditorWindow host,
                                      Action<object> onBeforeChange,
                                      Action<object> onChanged,
                                      HostState state,
                                      Driver driver)
        {
            DrawObject(target, host, onBeforeChange, onChanged, state?.Session, driver);
        }

        /// <summary>创建一份宿主状态句柄; 由宿主自己持有, 生命周期与宿主一致。</summary>
        public static HostState CreateHostState()
        {
            return new HostState(new HostSession());
        }

        /// <summary>
        /// 是否 Unity"值包"类型(AnimationCurve / Gradient): 数据在原生对象里,
        /// 序列化按整体读写, 绘制走 CurveField/GradientField 而不是展开成员。
        ///
        /// 判定本身住在 <see cref="DefaultTypeShape"/>(与容器判定同源), 这里只是把它公开出去 ——
        /// 宿主 cn.rendu.serialization 的序列化规则要跟引擎用**同一条**判定, 不能各写一份。
        /// </summary>
        public static bool IsUnityValueBagType(Type type)
        {
            return DefaultTypeShape.IsUnityValueBagType(type);
        }

        /// <summary>
        /// 绘制任意对象, 并显式指定本次绘制使用的状态会话。
        /// 内部实现重载: HostSession 是内部类型, 程序集外的宿主请走 HostState 句柄。
        /// </summary>
        internal static void DrawObject(object target, EditorWindow host,
                                        Action<object> onBeforeChange,
                                        Action<object> onChanged,
                                        HostSession session)
        {
            DrawObject(target, host, onBeforeChange, onChanged, session, null);
        }

        /// <summary>
        /// 绘制任意对象, 并显式指定本次绘制使用的状态会话与后端。
        /// 内部实现重载: HostSession 是内部类型, 程序集外的宿主请走 HostState 句柄。
        /// </summary>
        internal static void DrawObject(object target, EditorWindow host,
                                        Action<object> onBeforeChange,
                                        Action<object> onChanged,
                                        HostSession session,
                                        Driver driver)
        {
            if (target == null) return;
            // 已销毁的 Unity 对象(fake null)不再绘制
            if (target is UnityEngine.Object unityTarget && unityTarget == null) return;

            var active  = driver ?? new Driver(host.rootVisualElement);
            var ctx     = DrawContext.Create(active, session ?? s_DefaultSession, onBeforeChange, onChanged,
                                             Application.isPlaying);
            var renderer = InspectorRenderer.Create(ctx, EngineServices.Shared);

            // BeginDraw 成功之后才配对 EndDraw: 后端若在开场就抛(例如  还没有可挂的树),
            // 再补一记 EndDraw 只会把栈弹穿, 把真正的错误盖成"Begin/End 不配平"。
            var began = false;
            try
            {
                active.BeginDraw();
                began = true;

                // 根测量: 本次绘制所在容器给的可用宽(面板的真实宽度)。
                //
                // 为什么必须在根上量: 表格盒的**最大宽** = 里面子控件最大宽之和(表头各列宽之和),
                // 盒子宽度被夹在这个上限里 —— 在盒内量到的是"表格自己的宽", 不是"面板给的宽",
                // 列宽与盒宽互相追着跑, 永远收不回来。根节点上量到的才是真实可用宽。
                //
                // 但根上量到的宽仍然是"内容自己撑出来的": 面板内容宽 = max(滚动区视口宽, 面板最小宽),
                // 而面板最小宽被表格自己的定宽表头/单元格抬高 —— 量到的宽跟着涨 → 列宽再涨 → 更宽,
                // 形成"越撑越宽"的死循环(实测能撑到几千像素)。
                // 所以宿主若报了**视口宽**(滚动区视口, 与内容无关), 以它为准: 没有反馈环。
                //
                // 时机: GetRect 有布局副作用(每帧每 pass 只能调一次), 且 Layout 事件里量不到真值,
                // 所以这里只把量到的值交出去, 由表格存进会话跨帧使用(排版要求两遍一致)。
                var measured = ctx.Driver.MeasureContentWidth();
                var viewport = ctx.Session.ViewportWidth;
                ctx.RootWidth = viewport > 1f ? viewport : measured;

                // 根对象始终允许绘制: EditorWindow 自绘, 或 Editor 检视组件
                renderer.DrawRoot(target, host);
            }
            catch (Exception e) when (!(e is ExitGUIException))
            {
                DrawLog.Error($"[Inspector] 绘制失败 {target.GetType().Name}\n{e}");
            }
            finally
            {
                if (began) active.EndDraw();
            }
        }

        //-------------------------------------- 变更胶水（宿主共享） --------------------------------------

        /// <summary>
        /// 一次写入的共享胶水：构造时拍 Undo 快照，<see cref="Dispose"/> 时标记脏 / 登记预制体修改 / 重绘。
        /// 作用域风格：<c>using (Engine.BeginChange(target)) { 改字段; }</c>。
        /// </summary>
        public sealed class ChangeScope : IDisposable
        {
            private readonly object _target;
            public ChangeScope(object target)
            {
                _target = target;
                if (target is UnityEngine.Object uo)
                    Undo.RecordObject(uo, "RD Draw Change");
            }
            public void Dispose() => ApplyChange(_target);
        }

        /// <summary>作用域风格的变更胶水：构造拍 Undo 快照，<c>using</c> 结束（<see cref="ChangeScope.Dispose"/>）时标记脏与重绘。</summary>
        public static IDisposable BeginChange(object target) => new ChangeScope(target);

        /// <summary>回调风格的变更胶水：在引擎写入完成（onChanged）后调用，标记脏 / 登记预制体修改 / 重绘。</summary>
        public static void EndChange(object target) => ApplyChange(target);

        private static void ApplyChange(object target)
        {
            if (target is not UnityEngine.Object uo) return;
            EditorUtility.SetDirty(uo);
            if (PrefabUtility.IsPartOfPrefabInstance(uo))
                PrefabUtility.RecordPrefabInstancePropertyModifications(uo);
            if (target is EditorWindow win) win.Repaint();
            else EditorWindow.focusedWindow?.Repaint();
        }
    }
}
