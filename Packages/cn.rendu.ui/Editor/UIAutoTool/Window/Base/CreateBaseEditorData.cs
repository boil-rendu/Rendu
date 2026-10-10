#if UNITY_EDITOR
using RD.EditorKit;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>创建基类的编辑器数据占位（保持 Odin 折叠面板的绘制风格一致）</summary>
    [HideReferenceObjectPicker]
    public class CreateBaseEditorData
    {
        /// <summary>空构造</summary>
        public CreateBaseEditorData()
        {
        }
    }
}
#endif
