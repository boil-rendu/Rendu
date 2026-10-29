using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RD.Draw.Editor.Rules
{
    /// <summary>
    /// <see cref="ITypeShape"/> 的默认实现 —— 引擎自带的类型分类规则。
    ///
    /// 纯判定、无缓存需求: 这些都是 O(1) 的类型判断, 不值得缓存。
    /// 想换一套规则时实现 <see cref="ITypeShape"/> 并注册到 <see cref="EngineServices"/> 即可。
    /// </summary>
    internal sealed class DefaultTypeShape : ITypeShape
    {
        /// <summary>
        /// Unity"值包"类型的唯一判定: 曲线 / 渐变。
        ///
        /// 放在这里而不是门面上, 是为了让 <see cref="ITypeShape"/> 的实现不必回头依赖
        /// <see cref="Engine"/>(门面是最外层, 反向依赖会成环)。需要对外暴露时由
        /// <see cref="Engine.IsUnityValueBagType"/> 转调本方法 —— 全仓仍只有一份实现,
        /// 绘制与序列化因此不会各判各的。
        /// </summary>
        internal static bool IsUnityValueBagType(Type type)
        {
            return type == typeof(AnimationCurve) || type == typeof(Gradient);
        }

        /// <summary>曲线/渐变等"Unity 值类型包": 数据在原生对象里, 展开无意义, 交给专用控件。</summary>
        public bool IsUnityValueBag(Type type)
        {
            return IsUnityValueBagType(type);
        }

        /// <summary>Unity 对象引用(MonoBehaviour / ScriptableObject / Texture …)。</summary>
        public bool IsUnityObject(Type type)
        {
            return type != null && typeof(UnityEngine.Object).IsAssignableFrom(type);
        }

        /// <summary>列表 / 字典 / 集合 —— 需要用带增删改的容器画法展开。</summary>
        public bool IsCollection(Type type)
        {
            if (type == null) return false;
            if (typeof(IList).IsAssignableFrom(type)) return true;
            if (typeof(IDictionary).IsAssignableFrom(type)) return true;
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>);
        }

        /// <summary>
        /// 需要"展开成子成员"的容器: 集合, 或普通用户类/结构体。
        ///
        /// 刻意排除的三类(以及为什么):
        ///   - IsUnityValueBag: 曲线/渐变的数据在原生对象里, 展开出来只是一堆无意义的
        ///     句柄成员 —— 应交给 CurveField/GradientField;
        ///   - string: 虽有成员但没有可编辑语义;
        ///   - IsUnityObject: 引用该走对象选择器, 除非显式 [InlineEditor]。
        /// </summary>
        public bool IsContainer(Type type)
        {
            if (type == null) return false;
            if (IsCollection(type)) return true;
            if (IsUnityValueBag(type)) return false;
            if (type.IsClass == false) return false;
            if (type == typeof(string)) return false;
            if (IsUnityObject(type)) return false;
            return true;
        }

        /// <summary>
        /// [InlineEditor] 是否有意义: 只有"能展开成子成员"的类型才值得内联。
        /// 标量(string/基本类型/枚举/数组)一律否 —— 内联一个 int 没有内容可画。
        /// </summary>
        public bool CanInlineEdit(Type type)
        {
            if (type == null) return false;
            if (type == typeof(string) || type.IsPrimitive || type.IsEnum || type.IsArray) return false;
            return IsUnityObject(type) || IsContainer(type);
        }

        /// <summary>
        /// 有专用复合控件的结构类型: 交给后端挑具体控件(见 Driver.StructField)。
        /// </summary>
        public bool HasStructFieldControl(Type type)
        {
            return type == typeof(Vector2)     || type == typeof(Vector3)    || type == typeof(Vector4) ||
                   type == typeof(Vector2Int)  || type == typeof(Vector3Int) || type == typeof(Quaternion) ||
                   type == typeof(Rect)        || type == typeof(RectInt)    || type == typeof(Bounds) ||
                   type == typeof(BoundsInt)   || type == typeof(LayerMask);
        }

        /// <summary>集合的元素类型(数组取元素类型, 泛型集合取第一个泛型实参), 不是集合时返回 null。</summary>
        public Type GetElementType(Type collectionType)
        {
            if (collectionType == null) return null;
            if (collectionType.IsArray) return collectionType.GetElementType();
            if (collectionType.IsGenericType) return collectionType.GetGenericArguments()[0];
            return null;
        }

        /// <summary>"标量型" —— 能用一行输入框表示的类型。</summary>
        public bool IsScalarLike(Type type)
        {
            if (type == null) return false;
            if (type.IsEnum || type.IsPrimitive) return true;
            if (type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime)) return true;
            return type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Color) ||
                   typeof(UnityEngine.Object).IsAssignableFrom(type);
        }
    }
}
