using RD;
using RD.Core;
using System;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
#endif

namespace RD.Client
{
    public static class UIConstHelper
    {
        public const string UIConstAssetName = "UIConstAsset";

        /// <summary>
        /// UI 常量文件路径 本工程 UI 资源集中在 Packages/cn.rendu.gameres/UI (YooAsset UI 收集组的路径)
        /// 必须落在被收集的目录内 否则打包清单里不会有这个资源
        /// </summary>
        public const string UIConstAssetPath = "Packages/cn.rendu.game/Assets/GameRes/UI/UISettings/UIConstAsset.txt";

        private static UIConstAsset m_UIConstAsset;

        #if !UNITY_EDITOR
        public static UIConstAsset Const => m_UIConstAsset;
        #endif

        public static async Promise<bool> LoadAsset(Scene scene)
        {
            // UI 组已开启可寻址(AddressRuleName = AddressByFileName) 资源名即 location 这里用 UIConstAssetName
            // UIConstAssetPath 只给编辑器侧用: AssetDatabase 读写需要工程相对路径 走不到 YooAsset 清单
            m_UIConstAsset = await JsonSerializationHelper.RuntimeLoad<UIConstAsset>(scene, UIConstAssetName);
            return m_UIConstAsset != null;
        }

        #if UNITY_EDITOR

        public static UIConstAsset Const
        {
            get
            {
                return m_UIConstAsset ??= LoadEditorAsset();
            }
        }

        private static UIConstAsset LoadEditorAsset()
        {
            var constAsset = JsonSerializationHelper.EditorLoad<UIConstAsset>(UIConstAssetPath);
            constAsset ??= CreateAsset();
            return constAsset;
        }

        private static void WriteAllBytes(string path, byte[] bytes)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(path);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllBytes(path, bytes);
            }
            catch (Exception e)
            {
                Debug.LogError("写入文件失败: path =" + path + ", err=" + e);
            }
        }

        private static UIConstAsset CreateAsset()
        {
            var constAsset = new UIConstAsset();
            var json = JsonUtility.ToJson(constAsset);
            var bytes = Encoding.UTF8.GetBytes(json);
            WriteAllBytes($"{Application.dataPath}/../{UIConstAssetPath}", bytes);
            return constAsset;
        }

        public static UIConstAsset ResetAsset()
        {
            return m_UIConstAsset = CreateAsset();
        }

        #endif
    }
}