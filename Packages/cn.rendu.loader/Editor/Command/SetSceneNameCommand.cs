using RD.Init.Editor;
using UnityEditor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 设置场景名称命令
    /// </summary>
    public class SetSceneNameCommand : CommandBase
    {
        private readonly string _sceneName;

        public SetSceneNameCommand(string sceneName)
        {
            _sceneName = sceneName;
        }

        public override void Execute()
        {
            UnityEngine.Debug.Log($"[SetSceneName] 设置 SceneName: {_sceneName}");

            GlobalConfig globalConfig = AssetDatabase.LoadAssetAtPath<GlobalConfig>(GlobalConfigModel.AssetPath);
            if (globalConfig == null)
            {
                UnityEngine.Debug.LogError("[SetSceneName] 未找到 GlobalConfigAsset");
                return;
            }

            globalConfig.SceneName = _sceneName;
            EditorUtility.SetDirty(globalConfig);
            AssetDatabase.SaveAssets();

            UnityEngine.Debug.Log($"[SetSceneName] SceneName 设置完成");
        }
    }
}
