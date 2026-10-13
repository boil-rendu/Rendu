#if UNITY_EDITOR

using RD.Panel;
using UnityEditor;
using UnityEngine;

namespace RD.Protocol.Editor
{
    /// <summary>
    /// Proto 列表的一行 —— [TableList] 的列来自本类型的成员, 列宽/列名/行内按钮挂在这里,
    /// 不污染业务模型 <see cref="ProtoItemModel"/>。
    /// </summary>
    public class ProtoRow
    {
        [ShowInInspector]
        [LabelText("名称")]
        [TableColumnWidth(200)]
        [InlineButton(nameof(Ping), "定位")]
        public string Name;

        [ShowInInspector]
        [LabelText("编号")]
        [TableColumnWidth(70)]
        [ReadOnly]
        public int Num;

        [ShowInInspector]
        [LabelText("路径")]
        [TableColumnWidth(360)]
        [ReadOnly]
        public string SimpPath;

        [ShowInInspector]
        [LabelText("状态")]
        [TableColumnWidth(100)]
        [ReadOnly]
        public string Status;

        /// <summary>原始完整路径, 供行内"定位"用; 不参与显示(没有 [ShowInInspector])。</summary>
        public string FullPath;

        /// <summary>行内"定位": 在 Project 窗口选中并 Ping 该 proto 文件(与原列表点文件名的行为一致)。</summary>
        private void Ping()
        {
            if (string.IsNullOrEmpty(FullPath)) return;

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
        }

        public static ProtoRow From(ProtoItemModel item)
        {
            var status = item switch
            {
                { IsFormatError: true }     => "❌ 格式错误",
                { IsDuplicateError: true }  => "⚠️ 数值重复",
                { IsParseError: true }      => "❌ 解析失败",
                _                           => "✅ 正常",
            };

            return new ProtoRow
            {
                Name     = item.Name,
                Num      = item.Num,
                SimpPath = item.SimpPath,
                Status   = status,
                FullPath = item.FullPath,
            };
        }
    }
}

#endif
