using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RD.Draw;
using UnityEditor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    public partial class RDPackageVersionModule
    {
        // 本文件承载"同步"相关两个按钮：
        //   强制依赖同步 —— 把版本号变更沿依赖图递归传播（SyncRefPackages / ChageDependencies）
        //   强制本地同步 —— 注意：当前实现只是"关窗→卸载资源→重开窗"，并不真正联网；
        //                    真正的网络同步逻辑已被注释在文件末尾（#region 强制网络同步 旧实现）。
        #region 强制依赖同步

        [Button("强制依赖同步", 50)]
        [ButtonGroup("同步")]
        [GUIColor(0f, 0.8f, 1)]
        [PropertyOrder(-777)]
        [ShowIf("CheckUpdateAllEnd")]
        private void SyncPackages1()
        {
            UnityTipsHelper.CallBack($"确定强制依赖同步?", SyncRefPackages);
        }

        private void SyncRefPackages()
        {
            var changeInfo = new List<PackageVersionData>();
            foreach (var data in m_AllPackageInfoDataDic.Values)
            {
                var isChange = CheckPackageChange(data);
                if (!isChange) continue;

                changeInfo.Add(data);
            }

            foreach (var data in changeInfo)
            {
                foreach (var refSelf in data.DependedBy)
                {
                    refSelf.SyncSelfVersion();
                }
            }

            foreach (var data in m_AllPackageInfoDataDic.Values)
            {
                if (!data.IsRDPackage)
                {
                    continue;
                }

                foreach (var dependencie in data.DependsOn)
                {
                    if (dependencie.SyncVersionShowIf())
                    {
                        dependencie.SyncVersion();
                    }

                    if (dependencie.SyncSelfVersionShowIf())
                    {
                        dependencie.SyncSelfVersion();
                    }
                }

                foreach (var dependencie in data.DependedBy)
                {
                    if (dependencie.SyncVersionShowIf())
                    {
                        dependencie.SyncVersion();
                    }

                    if (dependencie.SyncSelfVersionShowIf())
                    {
                        dependencie.SyncSelfVersion();
                    }
                }
            }
        }

        public void ChageDependencies(PackageVersionData data)
        {
            var dataVersion = data.Version;

            foreach (var refSelf in data.DependedBy)
            {
                if (refSelf.Version == dataVersion)
                {
                    continue;
                }

                refSelf.Version = dataVersion;

                var refName     = refSelf.Name;
                var packageInfo = GetPackageInfoData(refName);
                if (packageInfo == null)
                {
                    Debug.LogError($"packageInfo == null {refName}");
                    continue;
                }

                var currentVersion = packageInfo.VersionLong;
                var lastVersion    = packageInfo.LastVersionLong;

                foreach (var dependencie in packageInfo.DependsOn)
                {
                    if (dependencie.Name == data.Name)
                    {
                        dependencie.Version = dataVersion;
                        break;
                    }
                }

                if (currentVersion > lastVersion)
                {
                    continue;
                }

                packageInfo.IncreasePatch();
                ChageDependencies(packageInfo);
            }
        }

        #endregion

        [Button("强制本地同步", 50)]
        [ButtonGroup("同步")]
        [GUIColor(0.4f, 0.8f, 1)]
        [PropertyOrder(-777)]
        [ShowIf("CheckUpdateAllEnd")]
        private void SyncNetPackages()
        {
            RDPackageManagerWindow.UnloadAllAssets();
            RDPackageManagerWindow.CloseWindowRefresh();
            RDPackageManagerWindow.OpenWindow();
        }

        /*
        #region 强制网络同步

        [Button("强制网络同步", 50)]
        [ButtonGroup("同步")]
        [GUIColor(0.4f, 0.8f, 1)]
        [PropertyOrder(-777)]
        [ShowIf("CheckUpdateAllEnd")]
        private void SyncNetPackages()
        {
            UnityTipsHelper.CallBack($"确定强制网络同步?", RefreshRequestAll);
        }

        private bool m_RequestingAll;
        private int  m_RefreshCompleteCount;
        private int  m_RefreshMaxCount;

        private void RefreshRequestAll()
        {
            if (m_RequestingAll)
            {
                UnityTipsHelper.Show($"请求中请稍等...请勿频繁请求");
                return;
            }

            CheckUpdateAllEnd      = false;
            m_RequestingAll        = true;
            m_RefreshMaxCount      = m_AllPackageInfoDataDic.Count;
            m_RefreshCompleteCount = 0;

            EditorUtility.DisplayProgressBar("同步信息", $"请求中... {m_RefreshCompleteCount} / {m_RefreshMaxCount}", 0);

            foreach (var package in m_AllPackageInfoDataDic.Values)
            {
                if (!package.IsRDPackage) continue;

                var packageName = package.Name;

                new PackageRequestTarget(
                    packageName,
                    (info) =>
                    {
                        if (info != null)
                        {
                            package.Version = info.version;
                        }

                        RequestComplete();
                    });
            }
        }

        private void RequestComplete()
        {
            m_RefreshCompleteCount++;

            EditorUtility.DisplayProgressBar(
                "同步信息",
                $"请求中... {m_RefreshCompleteCount} / {m_RefreshMaxCount}",
                (float)m_RefreshCompleteCount / m_RefreshMaxCount);

            if (m_RefreshCompleteCount >= m_RefreshMaxCount)
            {
                CheckUpdateAllEnd = true;
                m_RequestingAll   = false;
                EditorUtility.ClearProgressBar();
            }
        }

        #endregion
        */
    }
}
