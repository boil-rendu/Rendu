#if UNITY_EDITOR

using RD.Draw;
using UnityEditor;
using UnityEngine;

namespace RD.Excel.Editor
{
    /// <summary>
    /// Excel 列表的一行 —— 表格([TableList])的列来自本类型的成员, 所以要在这里挂列宽/列名/行内按钮,
    /// 而不是去改 <see cref="ExcelItemModel"/>(那是业务数据模型, 不该带编辑器绘制特性)。
    /// </summary>
    public class ExcelFileRow
    {
        [ShowInInspector]
        [LabelText("文件名")]
        [TableColumnWidth(220)]
        [InlineButton(nameof(Ping), "定位")]
        public string FileName;

        [ShowInInspector]
        [LabelText("所在目录")]
        [TableColumnWidth(180)]
        public string RelativeDir;

        [ShowInInspector]
        [LabelText("完整路径")]
        [TableColumnWidth(420)]
        [DisplayAsString]
        public string FullPath;

        /// <summary>行内"定位"按钮: 在 Project 窗口里选中并 Ping 这个文件(与原 UXML 版点文件名的行为一致)。</summary>
        private void Ping()
        {
            if (string.IsNullOrEmpty(FullPath))
            {
                return;
            }

            var assetPath = FullPath;
#if UNITY_EDITOR_WIN
            assetPath = assetPath.Replace(Application.dataPath.Replace("Assets", "").Replace("/", "\\"), "");
#elif UNITY_EDITOR_OSX
            assetPath = assetPath.Replace(Application.dataPath.Replace("Assets", ""), "");
#endif

            var obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (obj != null)
            {
                EditorGUIUtility.PingObject(obj);
                Selection.activeObject = obj;
            }
            else
            {
                Debug.LogError($"文件未找到: {assetPath}");
            }
        }

        public static ExcelFileRow From(ExcelItemModel item)
        {
            return new ExcelFileRow
            {
                FileName    = item.FileName,
                RelativeDir = item.RelativeDir ?? ".",
                FullPath    = item.FullPath,
            };
        }
    }
}

#endif
