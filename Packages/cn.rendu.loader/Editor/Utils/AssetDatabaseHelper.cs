using UnityEditor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 资源数据库辅助类
    /// </summary>
    public static class AssetDatabaseHelper
    {
        /// <summary>
        /// 加载指定路径的资源
        /// </summary>
        public static T LoadAssetAtPath<T>(string path) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}
