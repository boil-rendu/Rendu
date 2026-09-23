namespace RD.Init.Editor
{
#if UNITY_2019
    /// <summary>
    /// Unity 2019版本的ListView扩展方法
    /// 提供ClearSelection和Rebuild方法以兼容不同Unity版本的API
    /// </summary>
    public static partial class UnityEngine_UIElements_ListView_Extension
    {
        /// <summary>
        /// 清空ListView的选中项
        /// </summary>
        /// <param name="o">ListView实例</param>
        public static void ClearSelection(this UnityEngine.UIElements.ListView o)
        {
            o.selectedIndex = -1;
        }
    }
#endif

#if UNITY_2019 || UNITY_2020
    /// <summary>
    /// Unity 2019和2020版本的ListView扩展方法
    /// 提供Rebuild方法以兼容不同Unity版本的API
    /// </summary>
    public static partial class UnityEngine_UIElements_ListView_Extension
    {
        /// <summary>
        /// 重建ListView（等同于Refresh方法）
        /// </summary>
        /// <param name="o">ListView实例</param>
        public static void Rebuild(this UnityEngine.UIElements.ListView o)
        {
            o.Refresh();
        }
    }
#endif
}