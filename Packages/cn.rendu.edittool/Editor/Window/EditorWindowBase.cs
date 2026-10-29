using RD.Draw.Editor;
using UnityEditor;

namespace RD.EditTool.Editor
{
    /// <summary>
    /// 轻量编辑器窗口基类（上层）。
    ///
    /// 后端二义已经全部下沉到 RD.Draw 的 <see cref="DrawWindow"/>：本类只是继承，自身不再持有任何
    ///  /  分支（原来的 <c>IWindowDrawMode</c> / <c>EnsureBackend</c> / <c>IsToolkit</c>
    /// 都已删除）。子类照旧用 <see cref="DrawWindow.DrawTargetObject"/> / <see cref="DrawWindow.ReportViewportWidth"/>，
    /// 需要自定义内容时重写 <see cref="DrawWindow.OnDrawContent"/>（两后端共用，一份代码）。
    ///
    /// 默认行为：画窗口自身成员（与改造前一致）。窗口宿主恒定以 后端绘制（见 <see cref="DrawWindow"/>）。
    /// </summary>
    public class EditorWindowBase : DrawWindow
    {
    }
}
