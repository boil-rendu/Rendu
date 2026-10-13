#if UNITY_EDITOR

using RD.Panel;

namespace RD.Recast.Editor
{
    /// <summary>
    /// 场景列表的一行 —— [TableList] 的列来自本类型的成员, 列宽/列名/只读都挂在这里,
    /// 而不是塞进业务数据里(原 SceneConfig 是窗口的私有内部类, 现在由本类型承担它的职责)。
    /// </summary>
    public class SceneExportRow
    {
        /// <summary>是否导出。表格里是一列可勾选的 Toggle(整表只读时也一样能勾 —— 列表只读控制的是增删/拖拽)。</summary>
        [ShowInInspector]
        [LabelText("导出")]
        [TableColumnWidth(50)]
        public bool Enabled = true;

        [ShowInInspector]
        [LabelText("场景")]
        [TableColumnWidth(160)]
        [ReadOnly]
        public string SceneName;

        [ShowInInspector]
        [LabelText("路径")]
        [TableColumnWidth(380)]
        [ReadOnly]
        public string ScenePath;

        public SceneExportRow()
        {
        }

        public SceneExportRow(string sceneName, string scenePath, bool enabled = true)
        {
            SceneName = sceneName;
            ScenePath = scenePath;
            Enabled   = enabled;
        }
    }
}

#endif
