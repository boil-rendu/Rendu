#if UNITY_EDITOR
using System.Text;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>
    /// 原生序列化文件存储 适用于大数据
    /// 基于 Unity JsonUtility 的序列化读写
    /// </summary>
    public static class JsonSerializationUtility
    {
        public static void Save<T>(T data, string path)
        {
            var json = JsonUtility.ToJson(data, true);
            var bytes = Encoding.UTF8.GetBytes(json);
            EditorHelper.WriteAllBytes(path, bytes);
        }

        public static T Load<T>(string path)
        {
            var bytes = EditorHelper.ReadAllBytes(path);
            if (bytes == null)
            {
                return default;
            }

            var json = Encoding.UTF8.GetString(bytes);

            // 旧版序列化格式(含 $id/$type 元数据)无法用 JsonUtility 解析 直接按不存在处理(由调用方重建)
            if (json.Contains("\"$id\"") || json.Contains("\"$type\"") || json.Contains("\"$odinserialization"))
            {
                Debug.LogWarning($"检测到旧版序列化格式 需要重新生成: {path}");
                return default;
            }

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"反序列化错误 {path}\n{e}");
                return default;
            }
        }
    }
}
#endif
