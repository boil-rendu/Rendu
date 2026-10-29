using System;
using System.Collections.Generic;
using UnityEngine;

namespace RD.Draw.Editor.Selectors
{
    /// <summary>
    /// 多态类型选择器 —— 一个带搜索的浮窗, 列出"基类型 + 所有可实例化的派生/实现",
    /// 选中即回调, 取消(关闭/失焦)则什么都不发生。
    ///
    /// 行为约定:
    ///   - **引用类型元素一律弹**(<see cref="NeedsPicker"/>): 具体类只有一个候选时也弹,
    ///     列表里能看到声明类型本身和 <c>None</c>; 不想弹的用
    ///     <c>[ListDrawerSettings(AlwaysAddDefaultValue = true)]</c> 关掉;
    ///   - **"None" 是可选项**: 传 <c>allowNone: true</c> 才列出, 选中回调 <c>null</c>;
    ///   - 候选里**不含 Unity 对象**: MonoBehaviour / ScriptableObject 该走对象选择器,
    ///     用类型选择器 new 出来的是游离实例, 不是资产/场景里的那个对象。
    ///
    /// 引擎里已经接在集合的"+"上(见 <c>CollectionMutator.AddElement</c>); 宿主也可以直接调
    /// <see cref="Open"/> 自己用。
    /// </summary>
    public static class TypeSelector
    {
        /// <summary>
        /// 打开选择器浮窗的实际实现钩子。默认未注册；由宿主包（cn.rendu.edittool 的
        /// <c>TypeSelectorWindow</c>）通过 <see cref="InitializeOnLoadAttribute"/> 注册。
        /// 这样 RD.Draw 作为纯引擎不持有任何 <see cref="EditorWindow"/>，也不会反向依赖宿主包（避免循环依赖）。
        /// </summary>
        public static Action<Type, Action<Type>, bool, Type, Vector2?> PickerOpener { get; set; }

        /// <summary>
        /// 新增这种元素时是否要先弹选择器 —— 引用类型(有候选时)一律 true(只有一个候选也弹);
        /// 值类型 / 枚举 / string 为 false(直接创建), Unity 对象为 false(它们走对象选择器)。
        /// 为 false 时调用方应按类型直接创建, 不必弹窗。
        /// </summary>
        public static bool NeedsPicker(Type baseType)
        {
            return TypeCandidates.NeedsPicker(baseType);
        }

        /// <summary>
        /// 取候选类型(已过滤: 无抽象/泛型定义/编译器生成/Unity 对象; 已排序: 基类型自身在最前)。
        /// 结果按类型缓存, 可直接用于渲染自己的选项列表。
        /// </summary>
        public static IReadOnlyList<Type> GetCandidates(Type baseType)
        {
            return TypeCandidates.Get(baseType);
        }

        /// <summary>
        /// 打开选择器浮窗。
        /// </summary>
        /// <param name="baseType">声明类型(集合的元素类型 / 成员的声明类型)。</param>
        /// <param name="onPicked">选中回调: 具体类型; 选了 "None" 时传 <c>null</c>。关闭窗口不会回调。</param>
        /// <param name="allowNone">是否列出 "None"(置空)一行。</param>
        /// <param name="current">当前值类型: 打开时预先高亮它(为 null 则高亮第一个候选)。</param>
        /// <param name="screenAnchor">屏幕坐标锚点(通常传鼠标位置); 不传则居中于当前窗口。</param>
        public static void Open(Type baseType, Action<Type> onPicked, bool allowNone = true,
                                Type current = null, Vector2? screenAnchor = null)
        {
            if (baseType == null || onPicked == null) return;

            // 浮窗是 EditorWindow 宿主，住在 cn.rendu.edittool；这里只经委托转发，保持 RD.Draw 纯引擎。
            PickerOpener?.Invoke(baseType, onPicked, allowNone, current, screenAnchor);
        }
    }
}
