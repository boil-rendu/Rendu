using RD.Panel;
using RD.EditTool.Editor;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>UITool 左侧菜单中的「系统图标」模块，点击即打开 Unity 内置图标浏览窗口</summary>
    [UIAutoMenu("系统图标", int.MaxValue, Scope = "ui")]
    public class UnityIconsModule : BaseToolModule
    {
        /// <summary>菜单按钮（排在面板最顶部）</summary>
        [Button("系统图标", 30)]
        [PropertyOrder(-99999)]
        public void OpenWindow()
        {
            UnityIconsWindow.ShowWindow();
        }

        /// <summary>模块加载时直接打开窗口</summary>
        public override void Initialize()
        {
            UnityIconsWindow.ShowWindow();
        }

        /// <summary>模块销毁（无需清理）</summary>
        public override void OnDestroy()
        {
        }
    }
}