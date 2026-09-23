using RD.Init.Editor;
using UnityEditor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 保存资源命令
    /// </summary>
    public class SaveAssetCommand : CommandBase
    {
        private readonly UnityEngine.Object _asset;

        public SaveAssetCommand(UnityEngine.Object asset)
        {
            _asset = asset ?? throw new System.ArgumentNullException(nameof(asset));
        }

        public override void Execute()
        {
            EditorUtility.SetDirty(_asset);
            AssetDatabase.SaveAssets();
        }
    }
}
