using RD.Inspector;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    [UIAutoMenu("系统图标", int.MaxValue)]
    public class UIUnityIconsModule : BaseUIToolModule
    {
        [Button("系统图标", 30)]
        [PropertyOrder(-99999)]
        public void OpenWindow()
        {
            UnityIconsWindow.ShowWindow();
        }

        public override void Initialize()
        {
            UnityIconsWindow.ShowWindow();
        }

        public override void OnDestroy()
        {
        }
    }
}