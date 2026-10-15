using RD.Draw.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.Inspector.Editor
{
    /// <summary>
    /// Inspector 通用检视器
    /// 用 Engine 反射绘制目标对象, 支持 [ShowInInspector] / [Button] / 各类分组等全部 Inspector 属性。
    ///
    /// 用法: 由业务侧显式注册, 例如
    ///     [CustomEditor(typeof(MyBehaviour))]
    ///     public class MyBehaviourEditor : InspectorEditor { }
    /// 也可以用 [CustomEditor(typeof(MyBase), true)] 一次覆盖整个继承族。
    /// 本类刻意不做全局注册(如 [CustomEditor(typeof(MonoBehaviour), true)]),
    /// 以免替换掉全工程其它脚本的 Unity 原生检视器。
    ///
    /// 绘制后端由本包内部切换(<see cref="InspectorBackend"/>), 业务代码不用动:
    ///   - IMGUI(默认): 挂一个 IMGUIContainer, 每帧调 OnInspectorGUI —— 与未改造前逐帧等价;
    ///   - UI Toolkit: 引擎画进一棵 VisualElement 树(UitkDriver 按位置复用元素)。
    /// 重写 CreateInspectorGUI 之后 Unity 不再自己调 OnInspectorGUI, IMGUI 分支由容器代调,
    /// 所以两条分支最终都汇到 OnInspectorGUI / Redraw 这两个入口。
    ///
    /// 与 Unity 原生检视器的差异(反射写值的固有限制, 接入前需确认可接受):
    ///   1. 不显示预制体覆盖(override)标记, 也无法用右键菜单还原单个属性;
    ///   2. 不做多对象差异编辑, 多选时只绘制第一个对象;
    ///   3. Undo 由本类在"写入前"自行拍快照(粒度到一次交互), 与 SerializedProperty 的行为略有差别。
    ///
    /// 注意: 基类必须写全 UnityEditor.Editor —— 本文件位于 RD.Inspector.Editor 命名空间内,
    ///       简单名 Editor 会被解析成 Inspector 下的 Editor 命名空间(CS0118)。
    /// </summary>
    [CanEditMultipleObjects]
    public class InspectorEditor : UnityEditor.Editor
    {
        private const string UndoName = "Inspector";

        /// <summary>本检视器的状态会话: 生存期与 Editor 实例一致, 折叠状态不会泄漏到进程级。</summary>
        private readonly HostState m_Session = Engine.CreateHostState();

        /// <summary>UI Toolkit 模式下引擎绘制的容器; IMGUI 模式下为空。</summary>
        private VisualElement m_Panel;

        /// <summary>UI Toolkit 模式下的绘制后端(与 m_Panel 同生共死, 重建面板时一起换新)。</summary>
        private IDriver m_Driver;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            if (InspectorBackend.UseUIToolkit)
            {
                // 多对象提示不能挂进引擎的容器: 后端按位置复用元素, 混进去会被当成"第一个控件"换掉
                if (targets != null && targets.Length > 1)
                {
                    root.Add(new HelpBox("Inspector 检视器不支持多对象编辑, 此处仅显示第一个对象。",
                                         HelpBoxMessageType.Warning));
                }

                m_Panel = new VisualElement();
                m_Panel.style.flexGrow = 1f;
                root.Add(m_Panel);

                m_Driver = new UitkDriver(m_Panel, Redraw);
                Redraw();
            }
            else
            {
                m_Panel  = null;
                m_Driver = null;

                // 用容器承载原来的逐帧 IMGUI: flexGrow 占满, 之后的绘制路径与未改造前一致
                var container = new IMGUIContainer(OnInspectorGUI) { style = { flexGrow = 1f } };
                root.Add(container);
            }

            return root;
        }

        /// <summary>
        /// IMGUI 分支的绘制体(容器每帧调这里)。
        /// 控件式修改(输入框/滑杆/开关)由 BeginChangeCheck 兜底;
        /// 按钮与集合增删不走 GUI.changed, 由引擎的 onChanged 回调兜底。
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (target == null) return;

            if (targets.Length > 1)
            {
                EditorGUILayout.HelpBox("Inspector 检视器不支持多对象编辑, 此处仅显示第一个对象。", MessageType.Warning);
            }

            EditorGUI.BeginChangeCheck();
            Engine.DrawObject(target, null, OnBeforeChange, OnChanged, m_Session);
            if (EditorGUI.EndChangeCheck())
            {
                OnChanged(target);
            }
        }

        /// <summary>UI Toolkit 分支的绘制体: 把 DrawObject 再跑一遍, 由 UitkDriver 的重绘回调触发。</summary>
        private void Redraw()
        {
            if (target == null || m_Driver == null) return;

            Engine.DrawObject(target, null, OnBeforeChange, OnChanged, m_Session, m_Driver);
        }

        /// <summary>
        /// 引擎即将写入数据: 在修改之前拍 Undo 快照。
        /// 反射写值绕过了 SerializedProperty 的 Undo 机制, 只有在此刻记录才能让 Ctrl+Z 回到修改前的状态。
        /// 注意用 Editor.target(序列化根)而不是引擎传回的 owner:
        /// owner 可能是组件内部的普通 C# 对象(如 UIData), 对它做 Undo / SetDirty 都不起作用。
        /// </summary>
        private void OnBeforeChange(object owner)
        {
            Undo.RecordObject(target, UndoName);
        }

        /// <summary>引擎已修改数据: 标记脏并请求重绘, 保证修改能落盘。</summary>
        private void OnChanged(object owner)
        {
            EditorUtility.SetDirty(target);

            // 预制体实例上的反射修改不会被 Unity 自动识别为 override, 需要显式登记
            if (PrefabUtility.IsPartOfPrefabInstance(target))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            }

            RepaintInspector();
        }

        /// <summary>请求重绘当前检视器所在的窗口(Editor 自身没有 Repaint 接口)。</summary>
        private static void RepaintInspector()
        {
            var focused = EditorWindow.focusedWindow;
            if (focused != null)
            {
                focused.Repaint();
            }
        }
    }
}
