using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>按类型名从 AssetDatabase 加载唯一的 ScriptableObject 配置资产</summary>
    public static class EditorScriptableLoader
    {
        /// <summary>
        /// 按类型名查找资产并加载
        /// 找不到报错返回 null；找到多个会逐个打印路径（仍返回第一个），提示配置重复
        /// </summary>
        public static T Load<T>() where T : ScriptableObject
        {
            var settingType = typeof(T);
            var guids       = AssetDatabase.FindAssets($"t:{settingType.Name}");
            if (guids.Length == 0)
            {
                Debug.LogError($"没有这个文件 : {settingType.Name}");
                return null;
            }
            else
            {
                if (guids.Length != 1)
                {
                    foreach (var guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        Debug.LogError($"找到多个文件 : {path}");
                    }
                }

                string filePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                var    asset    = AssetDatabase.LoadAssetAtPath<T>(filePath);
                return asset;
            }
        }
    }
}