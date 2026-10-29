
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RD.Draw;
using UnityEditor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    /// <summary>
    /// 一个包在"版本管理"列表里的一行。UI 三列由三个 VerticalGroup 拼成：
    ///   信息  —— 包名 / 版本 / 六个版本号按钮 / 重置 / 请求·禁·解 / 有最新版本可更新
    ///   我依赖 —— <see cref="DependsOn"/>（本包依赖的其它 RD 包）
    ///   依赖我 —— <see cref="DependedBy"/>（其它包反向指向本包的记录，由 PackageVersionHelper 自动生成）
    /// 注意：VersionValue 是 Version 解析后的 int[] 缓存，六个版本按钮直接改它，再经 Version setter 拼回字符串。
    /// </summary>
    [Serializable]
    [HideReferenceObjectPicker]
    public class PackageVersionData
    {
        [ReadOnly]
        [HideLabel]
        [VerticalGroup("信息")]
        [ShowInInspector]
        [PropertyOrder(-50)]
        [Serialize]
        public string Name { get; private set; }

        [ReadOnly]
        [HideLabel]
        [OnValueChanged("OnVersionChanged")]
        [VerticalGroup("信息")]
        [PropertyOrder(-45)]
        [Serialize]
        private string version;

        public string Version
        {
            get => version;
            set
            {
                version     = Regex.Replace(value, RDPackageVersionModule.Pattern, "");
                VersionLong = PackageHelper.GetVersionToLong(version);
                SetVersionValue();
            }
        }

        [HideInInspector]
        public long VersionLong;

        private void OnVersionChanged()
        {
            Version = version;
        }

        [Serialize]
        [HideInInspector]
        public bool IsRDPackage { get; set; }

        [Serialize]
        [HideInInspector]
        private int[] m_VersionValue;

        public int[] VersionValue
        {
            get
            {
                return m_VersionValue;
            }
        }

        private void SetVersionValue()
        {
            var versionSplit = Regex.Replace(Version, RDPackageVersionModule.Pattern, "").Split('.');
            m_VersionValue = new int[versionSplit.Length];
            for (int i = 0; i < versionSplit.Length; i++)
            {
                if (!int.TryParse(versionSplit[i], out m_VersionValue[i]))
                {
                    Debug.LogError($"{versionSplit[i]}不是数字");
                }
            }
        }

        [HideReferenceObjectPicker]
        [Serialize]
        [LabelText(" ")]
        [VerticalGroup("我依赖")]
        [ListDrawerSettings(DefaultExpandedState = true)]
        public List<DependencyInfo> DependsOn = new();

        [HideReferenceObjectPicker]
        [Serialize]
        [LabelText(" ")]
        [VerticalGroup("依赖我")]
        [ListDrawerSettings(DefaultExpandedState = true)]
        public List<DependencyInfo> DependedBy = new();

        // ↓大：主版本号 -1。注意直接改 VersionValue（Version 的 int[] 缓存），再借 Version setter 拼回字符串。
        [Button("↓大")]
        [GUIColor(1f, 0.5f, 0.5f)]
        [VerticalGroup("信息")]
        [ButtonGroup("信息/更新版本")]
        public void DecreaseMajor()
        {
            if (VersionValue.Length < 3) return;
            VersionValue[0]--;
            if (VersionValue[0] < 0)
            {
                VersionValue[0] = 0;
            }

            Version = string.Join(".", VersionValue);
        }

        // ↑大：主版本号 +1。
        [Button("↑大")]
        [GUIColor(1f, 0.65f, 0f)]
        [VerticalGroup("信息")]
        [ButtonGroup("信息/更新版本")]
        public void IncreaseMajor()
        {
            if (VersionValue.Length < 3) return;
            VersionValue[0]++;
            Version = string.Join(".", VersionValue);
        }

        // ↓中：次版本号 -1。
        [Button("↓中")]
        [GUIColor(1f, 0.5f, 0.5f)]
        [VerticalGroup("信息")]
        [ButtonGroup("信息/更新版本")]
        public void DecreaseMinor()
        {
            if (VersionValue.Length < 2) return;
            VersionValue[1]--;
            if (VersionValue[1] < 0)
            {
                VersionValue[1] = 0;
            }

            Version = string.Join(".", VersionValue);
        }

        // ↑中：次版本号 +1。
        [Button("↑中")]
        [GUIColor(1f, 1f, 0.5f)]
        [VerticalGroup("信息")]
        [ButtonGroup("信息/更新版本")]
        public void IncreaseMinor()
        {
            if (VersionValue.Length < 2) return;
            VersionValue[1]++;
            Version = string.Join(".", VersionValue);
        }

        // ↓小：修订号 -1。
        [Button("↓小")]
        [GUIColor(1f, 0.5f, 0.5f)]
        [VerticalGroup("信息")]
        [ButtonGroup("信息/更新版本")]
        public void DecreasePatch()
        {
            VersionValue[^1]--;
            if (VersionValue[^1] < 0)
            {
                VersionValue[^1] = 0;
            }

            Version = string.Join(".", VersionValue);
        }

        // ↑小：修订号 +1。
        [Button("↑小")]
        [GUIColor(0.5f, 1f, 0.7f)]
        [VerticalGroup("信息")]
        [ButtonGroup("信息/更新版本")]
        public void IncreasePatch()
        {
            VersionValue[^1]++;
            Version = string.Join(".", VersionValue);
        }

        [Button("重置")]
        [VerticalGroup("信息")]
        public void ResetVersion()
        {
            var packageInfo = PackageVersionHelper.GetPackageVersionData(Name);
            if (packageInfo == null)
            {
                return;
            }

            Version = packageInfo.Version;
        }

        [Button("请求")]
        [GUIColor(0f, 1f, 1f)]
        [VerticalGroup("信息")]
        [ShowIf("ShowIfReqVersion")]
        [ButtonGroup("信息/请求")]
        public void ReqVersion()
        {
            ReqCheckUpdate();
        }

        public bool ShowIfReqVersion()
        {
            return !IsBan && IsRDPackage && string.IsNullOrEmpty(LastVersion);
        }

        [Button("禁")]
        [GUIColor(1f, 0.3f, 0.3f)]
        [VerticalGroup("信息")]
        [ShowIf("ShowIfBanReqVersion")]
        [ButtonGroup("信息/请求")]
        public void BanReqVersion()
        {
            PackageHelper.BanPackage(Name);
            IsBan = true;
        }

        public bool ShowIfBanReqVersion()
        {
            return !IsBan && IsRDPackage && string.IsNullOrEmpty(LastVersion);
        }

        [Button("解")]
        [GUIColor(0.3f, 1f, 0.3f)]
        [VerticalGroup("信息")]
        [ShowIf("ShowIfReBanReqVersion")]
        [ButtonGroup("信息/请求")]
        public void ReBanReqVersion()
        {
            PackageHelper.ReBanPackage(Name);
            IsBan = false;
        }

        public bool ShowIfReBanReqVersion()
        {
            return IsBan && IsRDPackage && string.IsNullOrEmpty(LastVersion);
        }

        [GUIColor(0f, 1f, 0f)]
        [Button("有最新版本可更新", 50)]
        [VerticalGroup("信息")]
        [ShowIf("CanUpdateVersion")]
        public void CheckUpdateVersion()
        {
            UnityTipsHelper.CallBack($"{Name} 确定更新版本 {Version} >> {LastVersion}\n \n当前更新为覆盖更新模式!!!\n如果需要合并更新请自行解决!!!\n请确保网络没有问题!!!", UpdateDependencies);
        }

        private void UpdateDependencies()
        {
            var packagePath = Application.dataPath.Replace("Assets", "Packages") + "/" + Name;

            try
            {
                if (!System.IO.Directory.Exists(packagePath))
                {
                    return;
                }

                System.IO.Directory.Delete(packagePath, true);

                if (System.IO.Directory.Exists(packagePath))
                {
                    Debug.LogError("删除失败 文件还存在");
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"删除文件失败 {e.Message}");
                return;
            }

            foreach (var data in DependedBy)
            {
                var packageInfo = RDPackageVersionModule.Inst.GetPackageInfoData(data.Name);
                if (packageInfo == null)
                {
                    return;
                }

                data.Version = LastVersion;

                foreach (var info in packageInfo.DependsOn)
                {
                    if (info.Name == Name)
                    {
                        info.Version = LastVersion;
                        break;
                    }
                }
            }

            RDPackageManagerWindow.CloseWindow();
            EditorUtility.DisplayCancelableProgressBar("同步信息", $"正在更新 {Name}...", 0f);
            new PackageRequestAdd(Name, (info) =>
            {
                if (EditorUtility.DisplayCancelableProgressBar("同步信息", "正在收尾...", 1f))
                {
                    EditorUtility.ClearProgressBar();
                    Debug.LogWarning("[Package] 更新已取消收尾: 服务端已生效, 本地未应用, 请重跑一次");
                    return;
                }

                RDPackageManagerWindow.UnloadAllAssets();
                EditorUtility.ClearProgressBar();
                PackageExecuteMenuItemHelper.RDAll();
                AssetDatabase.SaveAssets();
                EditorApplication.ExecuteMenuItem("Assets/Refresh");
            });
        }

        [HideInInspector]
        [Serialize]
        public bool CanUpdateVersion { get; private set; }

        [HideInInspector]
        [Serialize]
        public bool IsBan { get; private set; }

        [ReadOnly]
        [LabelText("最新版本")]
        [DisplayAsString(false, 20, TextAlignment.Left, true)]
        [VerticalGroup("信息")]
        [ShowInInspector]
        [PropertyOrder(-100)]
        private string DisplayLastVersion
        {
            get
            {
                return $"<color=#00FF00>{LastVersion}</color>";
            }
        }

        [HideInInspector]
        [Serialize]
        private string m_LastVersion;

        public string LastVersion
        {
            get { return m_LastVersion; }
            private set
            {
                m_LastVersion   = value;
                LastVersionLong = PackageHelper.GetVersionToLong(value);
            }
        }

        [HideInInspector]
        public long LastVersionLong;

        public PackageVersionData(string name, string version)
        {
            Name        = name;
            Version     = version;
            IsRDPackage = name.Contains("cn.rendu.");
            ReqCheckUpdate();
        }

        private void ReqCheckUpdate()
        {
            if (!IsRDPackage) return;

            IsBan = PackageHelper.IsBanPackage(Name);

            if (IsBan) return;

            PackageHelper.CheckUpdateTarget(Name, (lastVersion) =>
            {
                CanUpdateVersion = false;
                if (string.IsNullOrEmpty(lastVersion))
                {
                    return;
                }

                LastVersion = lastVersion;

                if (PackageHelper.GetVersionToLong(lastVersion) > PackageHelper.GetVersionToLong(Version))
                {
                    CanUpdateVersion = true;
                }
            });
        }
    }
}
