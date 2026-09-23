#if UNITY_2019_4_OR_NEWER
using System.Reflection;
using UnityEditor;

namespace RD.Init.Editor
{
    /// <summary>
    /// UIElements光标工具类
    /// 通过反射访问UIElements.Cursor的私有属性，创建指定类型的鼠标光标
    /// </summary>
    public static class UIElementsCursor
    {
        private static PropertyInfo _defaultCursorId;

        /// <summary>
        /// 获取UIElements.Cursor的defaultCursorId私有属性
        /// </summary>
        private static PropertyInfo DefaultCursorId
        {
            get
            {
                if (_defaultCursorId != null)
                    return _defaultCursorId;

                _defaultCursorId = typeof(UnityEngine.UIElements.Cursor).GetProperty("defaultCursorId", BindingFlags.NonPublic | BindingFlags.Instance);
                return _defaultCursorId;
            }
        }

        /// <summary>
        /// 创建指定类型的UIElements光标
        /// </summary>
        /// <param name="cursorType">鼠标光标类型</param>
        /// <returns>UIElements.Cursor实例</returns>
        public static UnityEngine.UIElements.Cursor CreateCursor(MouseCursor cursorType)
        {
            var ret = (object)new UnityEngine.UIElements.Cursor();
            DefaultCursorId.SetValue(ret, (int)cursorType);
            return (UnityEngine.UIElements.Cursor)ret;
        }
    }
}
#endif