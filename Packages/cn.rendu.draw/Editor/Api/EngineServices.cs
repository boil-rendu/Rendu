using RD.Draw.Editor.Expressions;
using RD.Draw.Editor.Metadata;
using RD.Draw.Editor.Rendering;
using RD.Draw.Editor.Rules;
using RD.Draw.Editor.Tables;

namespace RD.Draw.Editor
{
    /// <summary>
    /// 引擎的服务容器 —— **组合根**: 反射、分组、取值、判定、枚举、表格结构、状态更新、
    /// 标签文案、类型形态、诊断输出都在这里被装配起来。
    ///
    /// 这些服务都是"只跟类型有关"的(结果可以跨帧复用), 因此引擎持有一份**进程级**实例
    /// (<see cref="Shared"/>), 缓存与跨帧状态由它们各自持有 —— 每次绘制新建的只有
    /// <see cref="DrawContext"/> 与 <see cref="InspectorRenderer"/>。
    ///
    /// 它不是"服务定位器": 各服务由 <see cref="InspectorRenderer"/> 在构造时按需接收,
    /// 没有"运行时按类型查找"这一步。之所以集中在一个对象里, 只是因为它们都是进程级单例,
    /// 传一个对象比传 8 个参数清楚。
    ///
    /// 依赖方向: 内层(表达式求值、可见性判定……)可以依赖本容器, 但**不依赖 <see cref="Engine"/>** ——
    /// 门面是最外层, 内层回头找门面会形成类型环(Interpreter → Engine → 服务 → Interpreter)。
    /// 门面自己转调本容器, 因此 <see cref="Engine"/> 里没有任何"给内部转发用"的方法。
    ///
    /// 其中 <see cref="Labels"/> 与 <see cref="Shape"/> 是**策略**(接口实现),
    /// 宿主可以换成自己的一套规则; 其余是引擎自己的实现, 没有替换的必要。
    /// </summary>
    internal sealed class EngineServices
    {
        /// <summary>进程级实例。绘制路径与各规则层共用这一份, 缓存因此能跨帧复用。</summary>
        internal static EngineServices Shared { get; } = new EngineServices();

        /// <summary>标签与文案规则(可替换: 宿主可自定义命名风格)。</summary>
        public ILabelFormatter Labels { get; }

        /// <summary>类型形态分类(可替换: 宿主可自定义"什么算容器/什么算标量")。</summary>
        public ITypeShape Shape { get; }

        public MemberCollector Members { get; } = new MemberCollector();

        public GroupResolver Groups { get; } = new GroupResolver();

        public ValueAccessor Values { get; } = new ValueAccessor();

        public VisibilityEvaluator Visibility { get; }

        public EnumCatalog Enums { get; }

        public TableSchema Tables { get; }

        public StateUpdater States { get; }

        /// <summary>
        /// 诊断输出。默认打到 Console; 宿主可通过 <see cref="Engine.SetDiagnosticSink"/> 换成
        /// 自己的实现(接日志窗口 / 测试断言), 换掉之后引擎源码不必改动。
        /// </summary>
        public IDiagnosticSink Diagnostics { get; set; } = new TableGeometrySink();

        public EngineServices()
        {
            // 装配顺序即依赖顺序: 先有策略, 再有依赖策略的服务。
            Labels = new DefaultLabelFormatter();
            Shape  = new DefaultTypeShape();

            Visibility = new VisibilityEvaluator(Values);
            Enums      = new EnumCatalog(Labels);
            Tables     = new TableSchema(Members, Labels);
            States     = new StateUpdater(Values);
        }
    }
}
