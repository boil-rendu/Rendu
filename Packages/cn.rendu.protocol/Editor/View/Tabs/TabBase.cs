using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.Protocol.Editor
{
    public abstract class TabBase : ITabStrategy
    {
        public abstract string Label { get; }
        public abstract void OnGUI();
        public abstract VisualElement CreateVisualElement();

        protected void DrawSectionHeader(string title)
        {
            EditorGUILayout.LabelField(title,
                new GUIStyle(UnityEditor.EditorStyles.boldLabel) { fontSize = 16 });
            GUILayout.Space(5);
        }

        protected void DrawInfoBox(string message)
        {
            EditorGUILayout.HelpBox(message, MessageType.Info);
            GUILayout.Space(5);
        }

        protected void DrawWarningBox(string message)
        {
            EditorGUILayout.HelpBox(message, UnityEditor.MessageType.Warning);
            GUILayout.Space(5);
        }

        protected VisualElement CreateSectionHeader(string title)
        {
            var label = new Label(title);
            label.style.fontSize = 16;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 5;
            return label;
        }

        protected VisualElement CreateInfoBox(string message)
        {
            var helpBox = new HelpBox(message, HelpBoxMessageType.Info);
            helpBox.style.marginBottom = 5;
            return helpBox;
        }

        protected VisualElement CreateWarningBox(string message)
        {
            var helpBox = new HelpBox(message, HelpBoxMessageType.Warning);
            helpBox.style.marginBottom = 5;
            return helpBox;
        }
    }
}
