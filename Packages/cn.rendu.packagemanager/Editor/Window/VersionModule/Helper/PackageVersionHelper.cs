
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    public static class PackageVersionHelper
    {
        public const string RDPackageVersionAssetPath = PackageHelper.RDPackageAssetsFolderPath + "/PackageVersionAsset.asset";

        private static PackageVersionAsset m_PackageVersionAsset;

        public static PackageVersionAsset PackageVersionAsset
        {
            get
            {
                if (m_PackageVersionAsset == null)
                {
                    if (!LoadAsset())
                    {
                        Debug.LogError($"m_PackageVersionAsset == null");
                        return null;
                    }
                }

                return m_PackageVersionAsset;
            }
        }

        public static void Unload()
        {
            m_PackageVersionAsset = null;
        }

        public static void SaveAsset()
        {
            if (m_PackageVersionAsset == null) return;
            EditorUtility.SetDirty(m_PackageVersionAsset);
        }

        public static bool LoadAsset()
        {
            m_PackageVersionAsset = AssetDatabase.LoadAssetAtPath<PackageVersionAsset>(RDPackageVersionAssetPath);

            if (m_PackageVersionAsset == null)
            {
                CreateAsset();
            }

            if (m_PackageVersionAsset == null)
            {
                Debug.LogError($"没有找到 配置资源 且自动创建失败 请检查");
                return false;
            }

            LoadAllPackageInfoData();
            return true;
        }

        private static void CreateAsset()
        {
            m_PackageVersionAsset = ScriptableObject.CreateInstance<PackageVersionAsset>();

            var assetFolder = $"{Application.dataPath}/../{PackageHelper.RDPackageAssetsFolderPath}";
            if (!Directory.Exists(assetFolder))
                Directory.CreateDirectory(assetFolder);

            AssetDatabase.CreateAsset(m_PackageVersionAsset, RDPackageVersionAssetPath);
        }

        private static void LoadAllPackageInfoData()
        {
            var m_AllPackageInfoDataList = m_PackageVersionAsset.AllPackageVersionData;
            m_AllPackageInfoDataList.Clear();

            //初始化
            foreach (var packageInfo in PackageHelper.CurrentRegisteredPackages.Values)
            {
                var name         = packageInfo.name;
                var version      = packageInfo.version;
                var dependencies = packageInfo.dependencies;

                var infoData = new PackageVersionData(name, version);
                infoData.DependsOn = new();
                foreach (var dependency in dependencies)
                {
                    infoData.DependsOn.Add(
                        new DependencyInfo()
                        {
                            RelatedPackageName = name,
                            Name             = dependency.name,
                            Version          = dependency.version,
                            IsDependedByEdge = false
                        });
                }

                m_AllPackageInfoDataList.Add(name, infoData);
            }

            //处理依赖关系
            foreach (var data in m_AllPackageInfoDataList.Values)
            {
                var name         = data.Name;
                var dependencies = data.DependsOn;
                foreach (var dependency in dependencies)
                {
                    if (!m_AllPackageInfoDataList.ContainsKey(dependency.Name))
                    {
                        if (dependency.Name.Contains("cn.rendu."))
                        {
                            Debug.LogError($"{name}依赖包{dependency.Name}不存在");
                        }
                        continue;
                    }

                    var target = m_AllPackageInfoDataList[dependency.Name];

                    if (target.DependedBy == null)
                    {
                        target.DependedBy = new();
                    }

                    target.DependedBy.Add(
                        new DependencyInfo()
                        {
                            RelatedPackageName = dependency.Name,
                            Name             = name,
                            Version          = dependency.Version,
                            IsDependedByEdge = true,
                        });
                }
            }
        }

        public static PackageVersionData GetPackageVersionData(string packageName)
        {
            return PackageVersionAsset.AllPackageVersionData.GetValueOrDefault(packageName);
        }
    }
}

