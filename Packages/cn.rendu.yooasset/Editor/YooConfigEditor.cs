using UnityEditor;
using UnityEngine;
using YooAsset;
using RD.YooAsset;

namespace YooAsset.Editor
{
    [CustomEditor(typeof(YooConfig))]
    public class YooConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _playModeProp;
        private SerializedProperty _editorForceRedownloadProp;
        private SerializedProperty _editorSimulateDownloadSpeedProp;
        private SerializedProperty _urlProp;
        private SerializedProperty _hostForceClearSandboxProp;

        private void OnEnable()
        {
            _playModeProp = serializedObject.FindProperty("EPlayMode");
            _editorForceRedownloadProp = serializedObject.FindProperty("EditorForceRedownload");
            _editorSimulateDownloadSpeedProp = serializedObject.FindProperty("EditorSimulateDownloadSpeed");
            _urlProp = serializedObject.FindProperty("Url");
            _hostForceClearSandboxProp = serializedObject.FindProperty("HostForceClearSandbox");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_playModeProp, new GUIContent("E Play Mode", "选择资源运行模式"));
            EditorGUILayout.Space(5);

            var playMode = (EPlayMode)_playModeProp.enumValueIndex;
            switch (playMode)
            {
                case EPlayMode.EditorSimulateMode:
                    DrawEditorSimulateMode();
                    break;
                case EPlayMode.HostPlayMode:
                    DrawHostPlayMode();
                    break;
                case EPlayMode.OfflinePlayMode:
                case EPlayMode.WebPlayMode:
                    DrawNoConfigMode(playMode);
                    break;
                default:
                    EditorGUILayout.HelpBox($"当前运行模式 {playMode} 在 YooConfig 中没有专属配置，使用 YooAsset 默认参数。", MessageType.Info);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEditorSimulateMode()
        {
            EditorGUILayout.LabelField("编辑器模拟模式配置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_editorForceRedownloadProp);
            EditorGUILayout.PropertyField(_editorSimulateDownloadSpeedProp, new GUIContent("虚拟下载速度（字节/秒）", "调小可放慢以便观察下载进度"));
        }

        private void DrawHostPlayMode()
        {
            EditorGUILayout.LabelField("联机模式配置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_urlProp, new GUIContent("Url", "资源服务器 CDN 地址"));

            if (string.IsNullOrWhiteSpace(_urlProp.stringValue))
            {
                EditorGUILayout.HelpBox("Host Play Mode 需要配置 Url，否则无法连接资源服务器。", MessageType.Warning);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("联机模式测试配置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_hostForceClearSandboxProp);
        }

        private void DrawNoConfigMode(EPlayMode playMode)
        {
            string tips = playMode switch
            {
                EPlayMode.OfflinePlayMode => "离线模式：不连接 CDN，直接使用 StreamingAssets / Resources 中内置的资源包。YooConfig 中无需额外配置。",
                EPlayMode.WebPlayMode => "Web 模式：YooConfig 当前未提供 Web 模式专属配置，将使用 YooAsset 默认参数。",
                _ => $"当前运行模式 {playMode} 在 YooConfig 中没有专属配置。"
            };
            EditorGUILayout.HelpBox(tips, MessageType.Info);
        }
    }
}
