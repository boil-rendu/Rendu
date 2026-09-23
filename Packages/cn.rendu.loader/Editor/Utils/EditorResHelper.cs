using UnityEditor;

namespace RD.Loader.Editor
{
    public class EditorResHelper
    {
        public static void SaveAssets(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
