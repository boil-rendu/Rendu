using UnityEditor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 编辑器对话框辅助类
    /// </summary>
    public static class EditorDialogHelper
    {
        /// <summary>
        /// 显示复杂对话框（三个按钮）
        /// </summary>
        public static int DisplayDialogComplex(string title, string message, string ok, string cancel, string alt)
        {
            return EditorUtility.DisplayDialogComplex(title, message, ok, cancel, alt);
        }
    }
}
