using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.EditTool.Editor
{
    /// <summary>
    /// 菜单树的拖拽: 菜单项之间的重排(拖拽源 + 落点判定), 以及 Unity 外部对象(资源等)拖进菜单。
    /// </summary>
    public partial class MenuEditorWindow
    {
        /// <summary>菜单项拖拽在 DragAndDrop 里用的键: 只认本窗口自己发起的项拖拽。</summary>
        private const string MenuItemDragKey = "RD.MenuTreeItem";

        /// <summary>位移超过它才算"拖", 免得手抖把一次点击变成拖拽。</summary>
        private const float DragThreshold = 4f;

        /// <summary>按下时暂存的拖拽候选(真正起拖要等 PointerMove 越过阈值)。</summary>
        private MenuTreeItem m_DragCandidate;

        /// <summary>按下时的指针位置: 指针事件的 position 是 Vector3(带 z), 与位移阈值比较要同类型。</summary>
        private Vector3      m_DragStartPos;
        private bool         m_DragPointerDown;
        private bool         m_DragStarted;

        //-------------------------------------- 外部对象拖入 --------------------------------------

        /// <summary>
        /// 外部 Unity 对象拖到 target 节点上: 默认不接收, 子类重写决定语义
        /// (要在菜单里加项的话, 记得自己调 <see cref="RebuildMenuTree"/> 刷新)。
        /// </summary>
        protected virtual void OnDropExternalObjects(MenuTreeItem target, UnityEngine.Object[] objects) { }

        /// <summary>面板级拖放: 命中哪个节点由 userData 沿父链反查。</summary>
        private void SetupExternalDrop(VisualElement menuPanel)
        {
            menuPanel.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                if (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0) return;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                evt.StopPropagation();
            });

            menuPanel.RegisterCallback<DragPerformEvent>(evt =>
            {
                var objs = DragAndDrop.objectReferences;
                if (objs == null || objs.Length == 0) return;

                DragAndDrop.AcceptDrag();
                evt.StopPropagation();

                var target = ResolveItemFromElement(evt.target as VisualElement);
                if (target != null)
                {
                    try { OnDropExternalObjects(target, objs); }
                    catch (Exception e) { Debug.LogError($"[EditTool] 外部对象拖入失败 {target.Name}\n{e}"); }
                }

                ResetDragState();
            });
        }

        //-------------------------------------- 菜单项重排 --------------------------------------

        /// <summary>行的拖拽源与落点判定: 源要 Draggable, 落点按悬停行的上/下半段决定。</summary>
        private void SetupRowDrag(VisualElement row, MenuTreeItem item)
        {
            row.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0) return;
                m_DragPointerDown = true;
                m_DragCandidate   = item;
                m_DragStartPos    = evt.position;
                m_DragStarted     = false;
            });

            row.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (m_DragStarted || m_DragPointerDown == false) return;
                if (ReferenceEquals(m_DragCandidate, item) == false) return;
                if (item.Draggable == false) return;
                if ((evt.position - m_DragStartPos).magnitude <= DragThreshold) return;

                m_DragStarted = true;
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData(MenuItemDragKey, item);
                DragAndDrop.StartDrag(string.IsNullOrEmpty(item.Name) ? "MenuTreeItem" : item.Name);
            });

            row.RegisterCallback<PointerUpEvent>(_ => ResetDragState());

            // 作为落点: 只认本窗口发起的项拖拽(外部拖拽由面板那一层处理)
            row.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                var dragged = DragAndDrop.GetGenericData(MenuItemDragKey) as MenuTreeItem;
                if (dragged == null || ReferenceEquals(dragged, item)) return;
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                evt.StopPropagation();
            });

            row.RegisterCallback<DragPerformEvent>(evt =>
            {
                var dragged = DragAndDrop.GetGenericData(MenuItemDragKey) as MenuTreeItem;
                if (dragged == null) return;

                DragAndDrop.AcceptDrag();
                evt.StopPropagation();

                try
                {
                    var (parent, index) = ResolveDropTarget(item, evt.mousePosition, row);
                    m_MenuTree?.MoveItem(dragged, parent, index);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EditTool] 菜单项移动失败 {item.Name}\n{e}");
                }
                finally
                {
                    ResetDragState();
                }
            });
        }

        /// <summary>
        /// 落点: 悬停行**上半** → 插到它前面(同一父级); **下半** → 有子级就落为第一个子项, 否则插到它后面。
        /// 与常见树控件一致 —— 想放进某个文件夹, 就往它下半部分拖。
        /// </summary>
        private (MenuTreeItem parent, int index) ResolveDropTarget(MenuTreeItem hovered, Vector2 mousePos, VisualElement row)
        {
            var list = hovered.Parent != null ? hovered.Parent.ChildMenuItems : m_MenuTree.MenuItems;
            var idx  = list.IndexOf(hovered);

            // 落点按鼠标落在行的哪一半算: mousePosition 与 worldBound 同为面板坐标系, 无需换算
            var bound = row.worldBound;
            var upper = bound.height > 0f && mousePos.y < bound.yMin + bound.height * 0.5f;

            if (upper) return (hovered.Parent, idx < 0 ? 0 : idx);
            if (hovered.HasChildren) return (hovered, 0);
            return (hovered.Parent, idx < 0 ? list.Count : idx + 1);
        }

        /// <summary>事件 target 可能是行内的图标 / 文字, 沿父链找回它所属的那一项。</summary>
        private static MenuTreeItem ResolveItemFromElement(VisualElement ve)
        {
            while (ve != null)
            {
                if (ve.userData is MenuTreeItem item) return item;
                ve = ve.parent;
            }

            return null;
        }

        private void ResetDragState()
        {
            m_DragPointerDown = false;
            m_DragStarted     = false;
            m_DragCandidate   = null;
        }
    }
}
