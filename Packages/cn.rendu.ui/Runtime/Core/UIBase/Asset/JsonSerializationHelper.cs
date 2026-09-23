using System;
using System.IO;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 原生序列化文件存储 适用于大数据
    /// 只读 存储必须在编辑器下
    /// 基于 Unity JsonUtility 的序列化读写 (UIConstAsset 等仅含普通字段)
    /// </summary>
    public static class JsonSerializationHelper
    {
        #if UNITY_EDITOR

        public static T EditorLoad<T>(string path)
        {
            var bytes = ReadAllBytes(path);
            if (bytes == null)
            {
                return default;
            }

            var json = System.Text.Encoding.UTF8.GetString(bytes);

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
            catch (Exception e)
            {
                Debug.LogError($"反序列化错误 {path}\n{e}");
                return default;
            }
        }

        private static byte[] ReadAllBytes(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                return File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                Debug.LogError("读取文件失败: path =" + path + ", err=" + e);
                return null;
            }
        }

        #endif

        public static async Promise<T> RuntimeLoad<T>(Scene scene, string assetName)
        {
            var loadResult = await EventSystem.Instance?.UIInvokeEntityAsync<UIInvokeEntity_Load, Promise<UnityObject>>(scene, new UIInvokeEntity_Load
            {
                LoadType = typeof(TextAsset),
                ResName = assetName
            });

            if (loadResult == null)
            {
                return default;
            }

            var bytes = ((TextAsset)loadResult).bytes;
            if (bytes == null)
            {
                return default;
            }

            var json = System.Text.Encoding.UTF8.GetString(bytes);
            if (json.Contains("\"$id\"") || json.Contains("\"$type\""))
            {
                Debug.LogError($"检测到旧版序列化格式 无法加载 {assetName} 请重新生成");
                return default;
            }

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"反序列化错误 {typeof(T).Name} {assetName}\n{e}");
                return default;
            }
        }
    }
}
