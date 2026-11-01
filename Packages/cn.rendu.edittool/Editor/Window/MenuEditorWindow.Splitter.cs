using UnityEngine.UIElements;

namespace RD.EditTool.Editor
{
    /// <summary>
    /// 左树与内容区之间的分割条: 布局上一条竖线, 按住可左右拖动改 <see cref="MenuWidth"/>。
    /// </summary>
    public partial class MenuEditorWindow
    {
        /// <summary>
        /// 分割条: 布局只占 1px(参考界面就是一条线), 命中区左右各放宽 2px(不影响布局)。
        /// </summary>
        private const float SplitterWidth = 1f;

        /// <summary>分割条命中区在左右各多出来的宽度(只为好拖, 不占布局)。</summary>
        private const float SplitterHitPadding = 1f;

        private bool m_SplitterResizing;

        private VisualElement BuildSplitter()
        {
            var splitter = new VisualElement();
            splitter.style.width      = SplitterWidth;
            splitter.style.flexGrow   = 0f;
            splitter.style.flexShrink = 0f;

            var line = new VisualElement();
            line.style.width           = SplitterWidth;
            line.style.flexGrow        = 1f;
            line.style.backgroundColor = SplitterColor;
            splitter.Add(line);

            var handle = BuildSplitterHandle();
            splitter.Add(handle);
            return splitter;
        }

        /// <summary>命中区: 绝对定位、横向盖住线两侧, 只负责拖动, 不参与布局(否则又把缝撑宽)。</summary>
        private VisualElement BuildSplitterHandle()
        {
            var handle = new VisualElement();
            handle.style.position = Position.Absolute;
            handle.style.left     = -SplitterHitPadding;
            handle.style.width    = SplitterWidth + SplitterHitPadding * 2f;
            handle.style.top      = 0f;
            handle.style.bottom   = 0f;

            handle.RegisterCallback<PointerDownEvent>(evt =>
            {
                m_SplitterResizing = true;
                handle.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            });

            handle.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (m_SplitterResizing == false) return;

                // 指针坐标: evt.position 是屏幕/世界坐标, 不能直接用; 用"命中区在面板里的位置 + 指针相对命中区的偏移"
                // 换算成面板坐标, 再减掉根元素的位置 = 距窗口左边的距离(与 IMGUI 版的 mousePosition.x 同义)
                MenuWidth = handle.worldBound.x + evt.localPosition.x - rootVisualElement.worldBound.x;
                if (m_MenuPanel != null) m_MenuPanel.style.width = MenuWidth;

                evt.StopPropagation();
            });

            handle.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (m_SplitterResizing == false) return;

                m_SplitterResizing = false;
                handle.ReleasePointer(evt.pointerId);
                evt.StopPropagation();
            });

            return handle;
        }
    }
}
