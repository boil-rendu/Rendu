using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    public static class PackageHelper
    {
        public const string RDPackagePath = "Packages/cn.rendu.packagemanager";

        public const string RDPackageAssetsFolderPath = RDPackagePath + "/Editor/Assets";

        public const string RDPackageInfoAssetPath = RDPackageAssetsFolderPath + "/PackageInfoAsset.asset";

        private static PackageInfoAsset m_PackageInfoAsset;

        public static PackageInfoAsset PackageInfoAsset
        {
            get
            {
                if (m_PackageInfoAsset == null)
                {
                    var result = LoadAsset();
                    if (!result)
                    {
                        Debug.LogError($"PackageInfoAsset == null");
                        return null;
                    }
                }

                return m_PackageInfoAsset;
            }
        }

        private static readonly Dictionary<string, UnityEditor.PackageManager.PackageInfo> m_CurrentRegisteredPackages = new();
        public static           Dictionary<string, UnityEditor.PackageManager.PackageInfo> CurrentRegisteredPackages => m_CurrentRegisteredPackages;

        private static void GetAllRegisteredPackages()
        {
            m_CurrentRegisteredPackages.Clear();
            foreach (var packageInfo in UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages())
            {
                var name = packageInfo.name;
                if (!name.Contains("cn.rendu."))
                {
                    continue;
                }

                var versionLong = GetVersionToLong(packageInfo.version);
                if (versionLong <= 0)
                {
                    continue;
                }

                m_CurrentRegisteredPackages.Add(name, packageInfo);
            }
        }

        public static void Unload()
        {
            m_PackageInfoAsset = null;
        }

        public static bool LoadAsset()
        {
            GetAllRegisteredPackages();

            if (m_PackageInfoAsset != null)
            {
                return true;
            }

            m_PackageInfoAsset = AssetDatabase.LoadAssetAtPath<PackageInfoAsset>(RDPackageInfoAssetPath);

            if (m_PackageInfoAsset == null)
            {
                CreateAsset();
            }

            if (m_PackageInfoAsset == null)
            {
                Debug.LogError($"没有找到 配置资源 且自动创建失败 请检查");
                return false;
            }

            m_PackageInfoAsset.ReUpdateInfo();

            return true;
        }

        private static void CreateAsset()
        {
            m_PackageInfoAsset = ScriptableObject.CreateInstance<PackageInfoAsset>();

            var assetFolder = $"{Application.dataPath}/../{RDPackageAssetsFolderPath}";
            if (!Directory.Exists(assetFolder))
                Directory.CreateDirectory(assetFolder);

            AssetDatabase.CreateAsset(m_PackageInfoAsset, RDPackageInfoAssetPath);
        }

        public static bool IsBanPackage(string name)
        {
            if (m_PackageInfoAsset == null)
            {
                Debug.LogError($"PackageInfoAsset == null");
                return false;
            }

            return m_PackageInfoAsset.BanPackageInfoHash.Contains(name);
        }

        public static void BanPackage(string name)
        {
            if (m_PackageInfoAsset == null)
            {
                var result = LoadAsset();
                if (!result)
                {
                    Debug.LogError($"PackageInfoAsset == null");
                    return;
                }
            }

            if (IsBanPackage(name))
            {
                return;
            }

            m_PackageInfoAsset.BanPackageInfoHash.Add(name);
            UpdateBanPackage();
        }

        public static void ReBanPackage(string name)
        {
            if (m_PackageInfoAsset == null)
            {
                var result = LoadAsset();
                if (!result)
                {
                    Debug.LogError($"PackageInfoAsset == null");
                    return;
                }
            }

            if (!IsBanPackage(name))
            {
                return;
            }

            m_PackageInfoAsset.BanPackageInfoHash.Remove(name);
            UpdateBanPackage();
        }

        private static void UpdateBanPackage()
        {
            var count = m_PackageInfoAsset.BanPackageInfoHash.Count;
            m_PackageInfoAsset.BanPackageInfo = new string[count];
            var index = 0;
            foreach (var value in m_PackageInfoAsset.BanPackageInfoHash)
            {
                m_PackageInfoAsset.BanPackageInfo[index] = value;
                index++;
            }

            EditorUtility.SetDirty(m_PackageInfoAsset);
        }

        public static string GetPackageLastVersion(string name)
        {
            if (m_PackageInfoAsset == null)
            {
                var result = LoadAsset();
                if (!result)
                {
                    Debug.LogError($"PackageInfoAsset == null");
                    return "";
                }
            }

            if (m_PackageInfoAsset.AllLastPackageInfoDic.TryGetValue(name, out var version))
            {
                return version;
            }

            return "";
        }

        public static UnityEditor.PackageManager.PackageInfo GetPackageInfo(string name)
        {
            return m_CurrentRegisteredPackages.GetValueOrDefault(name);
        }

        public static string GetPackageCurrentVersion(string name)
        {
            if (m_CurrentRegisteredPackages.TryGetValue(name, out var info))
            {
                return info.version;
            }

            return "";
        }

        private static void ResetPackageLastInfo(string name, string version)
        {
            if (m_PackageInfoAsset == null)
            {
                var result = LoadAsset();
                if (!result)
                {
                    Debug.LogError($"PackageInfoAsset == null");
                    return;
                }
            }

            m_PackageInfoAsset.AllLastPackageInfoDic[name] = version;
        }

        private static void UpdateAllLastPackageInfo()
        {
            Dictionary<string, PackageLastVersionData> packageInfo = new();
            if (m_PackageInfoAsset.AllLastPackageInfo != null)
            {
                foreach (var pair in m_PackageInfoAsset.AllLastPackageInfo)
                {
                    var name = pair.Name;
                    if (m_PackageInfoAsset.AllLastPackageInfoDic.TryGetValue(name, out string version))
                    {
                        pair.Version      = version;
                        packageInfo[name] = pair;
                    }
                }
            }

            foreach (var info in m_PackageInfoAsset.AllLastPackageInfoDic)
            {
                var name = info.Key;
                if (!packageInfo.ContainsKey(name))
                {
                    var version = info.Value;
                    var pair    = new PackageLastVersionData();
                    pair.Name         = name;
                    pair.Version      = version;
                    packageInfo[name] = pair;
                }
            }

            m_PackageInfoAsset.AllLastPackageInfo = new PackageLastVersionData[packageInfo.Count];
            var index = 0;
            foreach (var pair in packageInfo.Values)
            {
                m_PackageInfoAsset.AllLastPackageInfo[index] = pair;
                index++;
            }

            EditorUtility.SetDirty(m_PackageInfoAsset);
        }

        private static bool                        m_Requesting;
        private static ListRequest                 m_ListRequest;
        private static readonly List<Action<bool>> m_RequestAllCallbacks = new();

        public static void CheckUpdateAll(Action<bool> callback)
        {
            try
            {
                if (m_PackageInfoAsset == null)
            {
                var result = LoadAsset();
                if (!result)
                {
                    callback?.Invoke(false);
                    return;
                }
            }

            // 上一次请求若已结束却未清理干净(回调丢失/异常), 复位, 避免永久卡在"请求中"
            if (m_Requesting && (m_ListRequest == null || m_ListRequest.IsCompleted))
            {
                EditorApplication.update -= CheckUpdateAllProgress;
                m_Requesting = false;
                m_RequestAllCallbacks.Clear();
                EditorUtility.ClearProgressBar();
            }

            // 已有请求在途: 把回调挂到同一请求上, 等它完成一起回调。
            // 旧实现用单个静态回调且开头置空: 多个模块(库/版本管理)首次选中都会调本方法,
            // 后发起者会覆盖先发起者的回调, 先发起者永远等不到回调 → 卡在"请求中"。
            if (m_Requesting)
            {
                if (callback != null) m_RequestAllCallbacks.Add(callback);
                return;
            }

            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (currentTime - m_PackageInfoAsset.LastUpdateTime < m_PackageInfoAsset.UpdateInterval)
            {
                callback?.Invoke(true);
                return;
            }

            m_PackageInfoAsset.LastUpdateTime = currentTime;
            EditorUtility.DisplayCancelableProgressBar("同步信息", "正在请求全部包信息...", 0f);

            ListRequest request;
            try
            {
                request = Client.List();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.ClearProgressBar();
                callback?.Invoke(false);
                return;
            }

            m_ListRequest = request;
            m_Requesting  = true;
            if (callback != null) m_RequestAllCallbacks.Add(callback);
            EditorApplication.update -= CheckUpdateAllProgress; // 防重复订阅
            EditorApplication.update += CheckUpdateAllProgress;
            }
            catch (Exception e)
            {
                // 同步阶段任何异常(如解析包版本号失败)都必须兜底回调, 否则调用方
                // (版本管理/库/更新窗口) 会永远停在"请求中..."。返回 false 让模块进入空数据态而非卡死。
                Debug.LogException(e);
                EditorUtility.ClearProgressBar();
                callback?.Invoke(false);
            }
        }

        /// <summary>结束本次全部包信息请求, 统一回调所有等待者(成功/失败/取消)。</summary>
        private static void FinishAllRequest(bool result)
        {
            EditorApplication.update -= CheckUpdateAllProgress;
            m_Requesting = false;
            EditorUtility.ClearProgressBar();

            if (m_RequestAllCallbacks.Count == 0) return;

            var callbacks = m_RequestAllCallbacks.ToArray();
            m_RequestAllCallbacks.Clear();
            foreach (var cb in callbacks)
            {
                try
                {
                    cb?.Invoke(result);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private static void CheckUpdateAllProgress()
        {
            // 请求未完成时每帧问一次取消: 用户点 Cancel 即放弃本次请求
            if (!m_ListRequest.IsCompleted)
            {
                if (EditorUtility.DisplayCancelableProgressBar("同步信息", "正在请求全部包信息...", 0f))
                {
                    Debug.Log("[Package] 用户取消了全部包信息请求");
                    FinishAllRequest(false);
                }

                return;
            }

            if (m_ListRequest.Status == StatusCode.Success)
            {
                m_PackageInfoAsset.ReSetAllLastPackageInfo();

                foreach (var package in m_ListRequest.Result.Reverse())
                {
                    var lastVersion = package.version;
                    if (package.versions != null)
                    {
                        lastVersion = package.versions.latest;
                    }

                    ResetPackageLastInfo(package.name, lastVersion);
                }

                UpdateAllLastPackageInfo();
                FinishAllRequest(true);
            }
            else
            {
                Debug.LogError(m_ListRequest.Error.message);
                FinishAllRequest(false);
            }
        }

        private static HashSet<string> m_RequestTargets = new();

        public static void CheckUpdateTarget(string name, Action<string> callback, bool showBar = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError($"不可查询 null包 请传入名称");
                callback?.Invoke("");
                return;
            }

            if (IsBanPackage(name))
            {
                callback?.Invoke("");
                return;
            }

            var version = GetPackageLastVersion(name);
            if (!string.IsNullOrEmpty(version))
            {
                callback?.Invoke(version);
                return;
            }

            if (showBar)
            {
                EditorUtility.DisplayCancelableProgressBar("同步信息", $"{name} 请求中...", 0f);
            }

            if (m_RequestTargets.Contains(name))
            {
                callback?.Invoke("");
                return;
            }

            m_RequestTargets.Add(name);

            new PackageRequestTarget(
                name,
                (packageInfo) =>
                {
                    m_RequestTargets.Remove(name);
                    if (showBar)
                    {
                        EditorUtility.ClearProgressBar();
                    }

                    if (packageInfo == null) return;
                    var lastVersion = packageInfo.version;
                    if (packageInfo.versions != null)
                    {
                        lastVersion = packageInfo.versions.latest;
                    }

                    ResetPackageLastInfo(packageInfo.name, lastVersion);
                    UpdateAllLastPackageInfo();
                    callback?.Invoke(lastVersion);
                });
        }

        public static long GetVersionToLong(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                Debug.LogError($"包版本号为空, 无法转换成长整型, 已跳过: {version}");
                return 0;
            }

            var splitVersion = version.Split(".");
            if (splitVersion.Length != 3)
            {
                Debug.LogError($"请修改 版号写法必须是 A.B.C  不支持: {version}");
                return 0;
            }

            var versionStr = "";
            for (int i = 0; i < splitVersion.Length; i++)
            {
                int.TryParse(Regex.Replace(splitVersion[i], "[^0-9]", ""), out int result);
                if (result > 99999)
                {
                    Debug.LogError($"小版本号最高支持5位数 不建议写这么大 请修改 {version}");
                    result = 99999;
                }

                versionStr += result.ToString("D5");
            }

            long.TryParse(versionStr, out long versionInt);

            return versionInt;
        }
    }
}
