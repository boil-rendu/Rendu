using System;

namespace RD.Draw.Editor.Rules
{
    /// <summary>
    /// "这个类型在引擎眼里是什么形态" —— 引擎对类型分类的**契约**。
    ///
    /// 分类收敛于此一处, 谁需要分类都问这一个对象。
    ///
    /// 之所以是接口而不是一个静态工具类: "形态"本质是**策略** ——
    /// 宿主可以换一套自己的分类规则(例如把某种结构体也当容器展开),
    /// 引擎的其余部分不必知道。换实现不需要改引擎源码(依赖倒置)。
    /// </summary>
    internal interface ITypeShape
    {
        /// <summary>曲线/渐变等"Unity 值类型包": 数据在原生对象里, 展开无意义, 交给专用控件。</summary>
        bool IsUnityValueBag(Type type);

        /// <summary>Unity 对象引用(MonoBehaviour / ScriptableObject / Texture …)。</summary>
        bool IsUnityObject(Type type);

        /// <summary>列表 / 字典 / 集合 —— 需要用带增删改的容器画法展开。</summary>
        bool IsCollection(Type type);

        /// <summary>需要"展开成子成员"的容器: 集合, 或普通用户类/结构体。</summary>
        bool IsContainer(Type type);

        /// <summary>[InlineEditor] 是否有意义: 只有"能展开成子成员"的类型才值得内联。</summary>
        bool CanInlineEdit(Type type);

        /// <summary>有专用复合控件的结构类型: 交给后端挑具体控件(见 Driver.StructField)。</summary>
        bool HasStructFieldControl(Type type);

        /// <summary>
        /// 集合的元素类型(数组取元素类型, 泛型集合取第一个泛型实参), 不是集合时返回 null。
        /// 表格的列结构由它推导, 列表的"添加元素"也要靠它决定实例化哪个类型。
        /// </summary>
        Type GetElementType(Type collectionType);

        /// <summary>
        /// "标量型" —— 能用一行输入框表示的类型(枚举/基本类型/字符串/Vector/Color/Unity 对象引用)。
        /// 集合里的元素是不是标量, 决定了它画成输入框还是展开成子成员。
        /// </summary>
        bool IsScalarLike(Type type);
    }
}
