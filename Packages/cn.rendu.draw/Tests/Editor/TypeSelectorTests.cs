using System.Linq;
using NUnit.Framework;
using RD.Draw.Editor.Selectors;
using UnityEngine;

namespace RD.Draw.Tests.Editor
{
    // 夹具放在命名空间层(TypeCache 对嵌套类型的收录行为不值得赌):
    // 接口 / 抽象基类 / 两个实现 / 封闭类 / 有派生类的具体类
    public interface ITypeSelectorTestNode { }

    public abstract class TypeSelectorTestBase : ITypeSelectorTestNode { }

    public sealed class TypeSelectorTestAlpha : TypeSelectorTestBase { }

    public sealed class TypeSelectorTestBeta : TypeSelectorTestBase { }

    public sealed class TypeSelectorTestLeaf { }

    public class TypeSelectorTestPlain { }

    public sealed class TypeSelectorTestPlainChild : TypeSelectorTestPlain { }

    /// <summary>
    /// 类型选择器的候选规则(纯逻辑, 不弹窗): 哪些类型算候选、什么时候才值得弹、实例化走哪条路。
    /// 窗口本身的交互(搜索 / 键盘 / None)不在 EditMode 里覆盖。
    /// </summary>
    public class TypeSelectorTests
    {
        [Test]
        public void Interface_Candidates_IncludeImplementations_ExcludeDeclaredTypes()
        {
            var candidates = TypeSelector.GetCandidates(typeof(ITypeSelectorTestNode));

            Assert.IsTrue(candidates.Contains(typeof(TypeSelectorTestAlpha)));
            Assert.IsTrue(candidates.Contains(typeof(TypeSelectorTestBeta)));
            Assert.IsFalse(candidates.Contains(typeof(TypeSelectorTestBase)), "抽象基类不该进候选");
            Assert.IsFalse(candidates.Contains(typeof(ITypeSelectorTestNode)), "接口自身不该进候选");
        }

        [Test]
        public void ConcreteBase_IsFirstCandidate_WithDerivedAfter()
        {
            var candidates = TypeSelector.GetCandidates(typeof(TypeSelectorTestPlain));

            Assert.AreEqual(typeof(TypeSelectorTestPlain), candidates[0], "声明类型本身排第一");
            Assert.IsTrue(candidates.Contains(typeof(TypeSelectorTestPlainChild)));
        }

        [Test]
        public void NeedsPicker_ForEveryReferenceTypeWithCandidates()
        {
            Assert.IsTrue(TypeSelector.NeedsPicker(typeof(ITypeSelectorTestNode)), "接口有实现");
            Assert.IsTrue(TypeSelector.NeedsPicker(typeof(TypeSelectorTestPlain)), "具体类");
            Assert.IsTrue(TypeSelector.NeedsPicker(typeof(TypeSelectorTestLeaf)), "封闭类也弹: 候选里有它自己和 None");
            Assert.IsFalse(TypeSelector.NeedsPicker(typeof(int)), "值类型直接创建");
            Assert.IsFalse(TypeSelector.NeedsPicker(typeof(string)));
            Assert.IsFalse(TypeSelector.NeedsPicker(typeof(MonoBehaviour)), "Unity 对象走对象选择器");
        }

        [Test]
        public void UnityObjectBases_ProduceNoCandidates()
        {
            Assert.IsEmpty(TypeSelector.GetCandidates(typeof(MonoBehaviour)));
            Assert.IsEmpty(TypeSelector.GetCandidates(typeof(Component)));
        }

        [Test]
        public void TryCreate_ConcreteType_Succeeds_AbstractOrInterface_Fails()
        {
            Assert.IsTrue(TypeCandidates.TryCreate(typeof(TypeSelectorTestAlpha), out var instance));
            Assert.IsInstanceOf<TypeSelectorTestAlpha>(instance);

            Assert.IsFalse(TypeCandidates.TryCreate(typeof(ITypeSelectorTestNode), out _));
            Assert.IsFalse(TypeCandidates.TryCreate(typeof(TypeSelectorTestBase), out _));
        }

        [Test]
        public void TryCreate_String_AddsEmptyString()
        {
            // string 没有无参构造函数, 走通用创建路径会失败 —— 但"加一条空字符串"是列表 + 的常规操作
            Assert.IsTrue(TypeCandidates.TryCreate(typeof(string), out var text));
            Assert.AreEqual(string.Empty, text);
        }
    }
}
