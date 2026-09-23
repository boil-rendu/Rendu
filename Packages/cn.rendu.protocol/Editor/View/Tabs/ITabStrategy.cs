using UnityEngine.UIElements;

namespace RD.Protocol.Editor
{
    public interface ITabStrategy
    {
        string Label { get; }

        // UI Toolkit 方法
        VisualElement CreateVisualElement();

        // IMGUI 方法（向后兼容）
        void OnGUI();
    }
}
