
using System;
using System.Text.RegularExpressions;
using RD.Draw;
using RD.Serialization;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    /// <summary>
    /// 一条"包 ↔ 包"的依赖边（既是"我依赖 A"也是"A 依赖我"的同一结构）。
    ///   - <see cref="Name"/>          ：关系另一端的包名
    ///   - <see cref="RelatedPackageName"/>：本边记录所属的那个包的包名（即"我"是谁）
    ///   - <see cref="IsDependedByEdge"/>：true=这条边属于"依赖我"方向（A 依赖我），false=属于"我依赖"方向（我依赖 A）
    /// 两个"同步"按钮（SyncVersion / SyncSelfVersion）只是按方向不同、点了之后都走 ChageDependencies 递归改版本。
    /// </summary>
    [Serializable]
    [HideLabel]
    [HideReferenceObjectPicker]
    public class DependencyInfo
    {
        [Serialize]
        [ReadOnly]
        [LabelText("名称")]
        public string Name;

        [Serialize]
        [ShowInInspector]
        [ReadOnly]
        [LabelText("版本")]
        [OnValueChanged("OnVersionChanged")]
        private string version;

        public string Version
        {
            get => version;
            set
            {
                version     = Regex.Replace(value, RDPackageVersionModule.Pattern, "");
                VersionLong = PackageHelper.GetVersionToLong(version);
            }
        }

        [Serialize]
        [HideInInspector]
        [Tooltip("本边记录所属的那个包的包名（即关系里的\"我\")")]
        public string RelatedPackageName;

        [Serialize]
        [HideInInspector]
        [Tooltip("true=属于'依赖我'方向的边；false=属于'我依赖'方向的边")]
        public bool IsDependedByEdge;

        [HideInInspector]
        public long VersionLong;

        private void OnVersionChanged()
        {
            Version = version;
        }

        #region 我依赖的版本不是最新时

        [Button("同步")]
        [GUIColor(0.7f, 0.4f, 0.8f)]
        [ShowIf("SyncVersionShowIf")]
        public void SyncVersion()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return;
            }

            var packageInfo = RDPackageVersionModule.Inst.GetPackageInfoData(Name);
            if (packageInfo == null)
            {
                return;
            }

            RDPackageVersionModule.Inst.ChageDependencies(packageInfo);
        }

        //同步目标的版本 当目标name的版本与当前版本不一致时显示同步按钮
        public bool SyncVersionShowIf()
        {
            if (IsDependedByEdge)
            {
                return false;
            }

            if (string.IsNullOrEmpty(Name))
            {
                return false;
            }

            var packageInfo = RDPackageVersionModule.Inst?.GetPackageInfoData(Name);
            if (packageInfo == null)
            {
                return false;
            }

            return packageInfo.Version != Version;
        }

        #endregion

        #region 依赖我的版本不是最新时

        [Button("同步")]
        [GUIColor(0.4f, 0.8f, 1)]
        [ShowIf("SyncSelfVersionShowIf")]
        public void SyncSelfVersion()
        {
            if (string.IsNullOrEmpty(RelatedPackageName))
            {
                return;
            }

            var packageInfo = RDPackageVersionModule.Inst.GetPackageInfoData(RelatedPackageName);
            if (packageInfo == null)
            {
                return;
            }

            RDPackageVersionModule.Inst.ChageDependencies(packageInfo);
        }

        //依赖我  但是我的版本已经升级了 他哪里的版本还是旧的时候显示同步按钮
        public bool SyncSelfVersionShowIf()
        {
            if (!IsDependedByEdge)
            {
                return false;
            }

            var packageInfo = RDPackageVersionModule.Inst?.GetPackageInfoData(RelatedPackageName);
            if (packageInfo == null)
            {
                return false;
            }

            return packageInfo.Version != Version;
        }

        #endregion
    }
}
