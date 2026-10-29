using System.Collections.Generic;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 有序责任链的共享机制: 按注册顺序遍历, **第一个能接管的档获胜**, 全都不接管时用兜底。
    ///
    /// 引擎里有两条形状完全一致的链, 它们回答的是两个不同的问题:
    ///   - 成员链(见 MemberRenderChain): "这个成员整体由谁画";
    ///   - 值链(见 ValueControlChain): "这个值用哪个控件"。
    ///
    /// 两条链的**遍历规则必须一致** —— 若一处支持优先级权重、另一处不支持, "加了新特性却没生效"
    /// 这类问题就会只在一处出现, 极难排查。所以规则只写在这里一次, 差异只体现在
    /// CanHandle 与 Handle 两个原语上。
    ///
    /// 这是**模板方法**: 遍历与兜底由本类固定, 每档做什么由子类原语决定。
    /// 档本身不知道自己排在第几位, 也就没人能把顺序依赖偷偷写进某一档。
    /// </summary>
    /// <typeparam name="TStage">链上的一档(如某个成员渲染器 / 某个值控件)。</typeparam>
    /// <typeparam name="TArgs">传给每档的输入包。</typeparam>
    /// <typeparam name="TResult">链条的产出; 只表现为副作用的链用 <see cref="Nothing"/>。</typeparam>
    internal abstract class DrawChain<TStage, TArgs, TResult> where TStage : class
    {
        private readonly List<TStage> m_Stages;

        /// <param name="stages">按优先级从高到低排列的各档。传 null 等于空链(只会走兜底)。</param>
        protected DrawChain(IEnumerable<TStage> stages)
        {
            m_Stages = stages == null ? new List<TStage>() : new List<TStage>(stages);
        }

        /// <summary>这一档是否接管。必须只看声明类型与特性, 不要看运行时值。</summary>
        protected abstract bool CanHandle(TStage stage, in TArgs args);

        /// <summary>由这一档完成工作并给出产出。</summary>
        protected abstract TResult Handle(TStage stage, in TArgs args);

        /// <summary>全链都不接管时的兜底。</summary>
        protected abstract TResult Fallback(in TArgs args);

        /// <summary>用第一个接管的档完成工作; 都不接管时走 <see cref="Fallback"/>。</summary>
        public TResult Resolve(in TArgs args)
        {
            for (var i = 0; i < m_Stages.Count; i++)
            {
                var stage = m_Stages[i];
                if (CanHandle(stage, args)) return Handle(stage, args);
            }

            return Fallback(args);
        }
    }

    /// <summary>
    /// "无产出"的类型标记: 用于结果只表现为副作用的链(例如成员渲染链 —— 画完即结束)。
    ///
    /// 之所以不用 void: DrawChain 需要一个统一的 TResult 才能让遍历与兜底只写一份,
    /// 而 C# 里没有可作泛型实参的 void。用一个真实类型比用 object 更安全 ——
    /// 忘了 return 会在编译期报错, 而不是悄悄返回 null。
    /// </summary>
    internal readonly struct Nothing
    {
        /// <summary>唯一取值。</summary>
        public static readonly Nothing Instance = default;
    }
}
