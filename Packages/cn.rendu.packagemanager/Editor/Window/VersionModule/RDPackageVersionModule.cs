using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RD.Draw;
using RD.EditTool.Editor;
using RD.Utilities;
using UnityEditor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    /// <summary>
    /// 版本管理页（左侧菜单项 order=1000）。
    /// UI 由 attribute 驱动，本类被拆成 3 个 partial：
    ///   - <see cref="RDPackageVersionModule"/>           ：筛选条件 + 包列表 + 把改动写回 package.json
    ///   - RDPackageVersionModule_Sync.cs                 ：强制依赖同步 / 强制本地同步
    ///   - RDPackageVersionModule_UpdateAll.cs            ：批量"更新当前所有"
    /// 数据：m_AllPackageInfoDataDic 是资产(PackageVersionAsset)的深拷贝，界面改的是副本，
    ///       只有"同步生成"才会把版本/依赖落盘到各包的 package.json。
    /// </summary>
    [ToolMenu("版本管理", 1000)]
    public partial class RDPackageVersionModule : BaseToolModule
    {
        public static RDPackageVersionModule Inst;

        [HideLabel]
        [HideIf("CheckUpdateAllEnd")]
        [ShowInInspector]
        [DisplayAsString(false, 100, TextAlignment.Center, true)]
        private const string m_CheckUpdateAllReqing = "请求所有包最新版本中...";

        [BoxGroup("信息", centerLabel: true)]
        [EnumToggleButtons]
        [HideLabel]
        [OnValueChanged("OnFilterOperationTypeChanged")]
        [ShowIf("CheckUpdateAllEnd")]
        [PropertyOrder(-666)]
        public EPackagesFilterOperationType FilterOperationType;

        private void OnFilterOperationTypeChanged()
        {
            if (FilterOperationType == EPackagesFilterOperationType.Only)
            {
                LastFilterType = EPackagesFilterType.None;
                FilterType     = EPackagesFilterType.None;
            }

            LoadFilterPackageInfoData();
        }

        [BoxGroup("信息", centerLabel: true)]
        [EnumToggleButtons]
        [HideLabel]
        [OnValueChanged("OnFilterTypeChanged")]
        [ShowIf("CheckUpdateAllEnd")]
        [PropertyOrder(-666)]
        public EPackagesFilterType FilterType;

        private EPackagesFilterType LastFilterType;

        // "唯一"模式下的单选实现：Flags 用位表示多选，但"唯一"要求一次只能选一个。
        // 技巧：把当前选集与"上一次选集"做位差值（|a-b|），从而自动取消前一个、只保留新点的那一位。
        // 例：上一次 RD(1<<1)，本次点 Update(1<<2)，差值 = (RD|Update) - RD = Update。
        private void OnFilterTypeChanged()
        {
            if (FilterOperationType == EPackagesFilterOperationType.Only)
            {
                if (FilterType != LastFilterType)
                {
                    var current = (int)FilterType;
                    var last    = (int)LastFilterType;
                    FilterType = (EPackagesFilterType)(current > last ? current - last : last - current);
                }
            }

            LastFilterType = FilterType;
            LoadFilterPackageInfoData();
        }

        [BoxGroup("信息", centerLabel: true)]
        [LabelText("搜索 (支持正则)")]
        [ShowIf("CheckUpdateAllEnd")]
        [OnValueChanged("OnSearchChanged")]
        [Delayed]
        [ShowInInspector]
        [PropertyOrder(-444)]
        private string Search = "";

        private void OnSearchChanged()
        {
            Search = Search.ToLower();
            LoadFilterPackageInfoData();
        }

        private PrefsValue<EPackagesFilterType> FilterTypePrefs = new("RDPackageVersionModule_FilterType", null, EPackagesFilterType.RD);

        private PrefsValue<EPackagesFilterOperationType> FilterOperationTypePrefs = new("RDPackageVersionModule_FilterOperationType");

        private PrefsValue<bool>   SyncDependencyPrefs = new("RDPackageVersionModule_SyncDependency", null, true);
        private PrefsValue<string> SearchPrefs         = new("RDPackageVersionModule_Search", null, "");

        public bool CheckUpdateAllEnd { get; private set; }

        public bool RequestAllResult { get; private set; }

        public override void Initialize()
        {
            CheckUpdateAllEnd = false;
            RequestAllResult  = false;
            PackageHelper.CheckUpdateAll((result) =>
            {
                CheckUpdateAllEnd = true;
                RequestAllResult  = result;
                if (!result) return;
                Search              = SearchPrefs.Value;
                FilterType          = FilterTypePrefs.Value;
                LastFilterType      = FilterType;
                FilterOperationType = FilterOperationTypePrefs.Value;
                LoadAllPackageInfoData();
                LoadFilterPackageInfoData();
                Inst = this;
            });
        }

        public override void OnDestroy()
        {
            Inst                           = null;
            SearchPrefs.Value              = Search;
            FilterTypePrefs.Value          = FilterType;
            FilterOperationTypePrefs.Value = FilterOperationType;
        }

        [Button("同步生成", 50)]
        [GUIColor(1, 1, 0)]
        [PropertyOrder(-888)]
        [ShowIf("CheckUpdateAllEnd")]
        public void SyncPackages()
        {
            UnityTipsHelper.CallBack($"确定同步生成当前所有改动版本?", () => { var _ = UpdatePackagesInfo(); });
        }

        [Button("文档", 30, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        [PropertyOrder(-999)]
        [ShowIf("CheckUpdateAllEnd")]
        public void OpenDocument()
        {
            RDPackageDocumentModule.RDPackageVersion();
        }

        private async Task UpdatePackagesInfo(bool close = true)
        {
            var count = m_AllPackageInfoDataDic.Count;
            var index = 0;
            foreach (var data in m_AllPackageInfoDataDic)
            {
                index++;
                var packageInfo = data.Value;
                if (!CheckPackageChange(packageInfo)) continue;

                // 可取消: 用户点 Cancel 即中止剩下的更新(已改的保留, 未改的不再动)
                if (EditorUtility.DisplayCancelableProgressBar(
                        "同步信息",
                        $"正在更新 {index}/{count}：{packageInfo.Name}",
                        index * 1f / count))
                {
                    EditorUtility.ClearProgressBar();
                    Debug.Log("[Package] 用户取消了包更新");
                    return;
                }

                await ChangePackageInfo(packageInfo);
            }

            RDPackageManagerWindow.UnloadAllAssets();

            if (close)
            {
                PackageExecuteMenuItemHelper.RD_Init_RepairDependencies();
                EditorUtility.ClearProgressBar();
                RDPackageManagerWindow.CloseWindowRefresh();
            }
        }

        private bool CheckPackageChange(PackageVersionData packageInfoData)
        {
            if (!packageInfoData.IsRDPackage)
            {
                return false;
            }

            var oldAllPackage = PackageVersionHelper.PackageVersionAsset.AllPackageVersionData;
            if (!oldAllPackage.ContainsKey(packageInfoData.Name))
            {
                return false;
            }

            var oldPackageInfo = oldAllPackage[packageInfoData.Name];

            //对比版本
            if (packageInfoData.Version != oldPackageInfo.Version)
            {
                return true;
            }

            //对比依赖
            if (packageInfoData.DependsOn.Count != oldPackageInfo.DependsOn.Count)
            {
                return true;
            }

            for (int i = 0; i < packageInfoData.DependsOn.Count; i++)
            {
                var dependency    = packageInfoData.DependsOn[i];
                var oldDependency = oldPackageInfo.DependsOn[i];
                if (dependency.Name != oldDependency.Name || dependency.Version != oldDependency.Version)
                {
                    return true;
                }
            }

            //对比依赖我
            if (packageInfoData.DependedBy.Count != oldPackageInfo.DependedBy.Count)
            {
                return true;
            }

            for (int i = 0; i < packageInfoData.DependedBy.Count; i++)
            {
                var dependency    = packageInfoData.DependedBy[i];
                var oldDependency = oldPackageInfo.DependedBy[i];
                if (dependency.Name != oldDependency.Name || dependency.Version != oldDependency.Version)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task ChangePackageInfo(PackageVersionData packageInfoData)
        {
            var assetPath   = $"Packages/{packageInfoData.Name}/package.json";
            var packagePath = $"{Application.dataPath}/../{assetPath}";
            if (!File.Exists(packagePath))
            {
                //Debug.LogError($"包 {packageInfoData.Name} 路径 {packagePath} 不存在");
                return;
            }

            var changeVersion = Regex.Replace(packageInfoData.Version, Pattern, "");

            try
            {
                string fileContent = await File.ReadAllTextAsync(packagePath);

                JObject json = JObject.Parse(fileContent);

                json["version"]      = changeVersion;
                json["dependencies"] = NewDependencies(packageInfoData);

                await File.WriteAllTextAsync(packagePath, json.ToString(), System.Text.Encoding.UTF8);

                Debug.Log($"修改成功 {packageInfoData.Name}");
            }
            catch (Exception e)
            {
                Debug.LogError($"错误 修改包 {packageInfoData.Name} 失败 {e.Message}");
                return;
            }
        }

        private JObject NewDependencies(PackageVersionData packageInfoData)
        {
            JObject newDependencies = new JObject();
            foreach (var dependency in packageInfoData.DependsOn)
            {
                newDependencies[dependency.Name] = Regex.Replace(dependency.Version, Pattern, "");
            }

            return newDependencies;
        }

        [TableList(DrawScrollView = true, AlwaysExpanded = true, IsReadOnly = true)]
        [NonSerialized]
        [BoxGroup("筛选包数据", centerLabel: true)]
        [HideLabel]
        [ShowInInspector]
        [ShowIf("CheckUpdateAllEnd")]
        [PropertyOrder(999)]
        private List<PackageVersionData> m_FilterPackageInfoDataList = new();

        private readonly Dictionary<string, PackageVersionData> m_FilterPackageInfoDataDic = new();

        private Dictionary<string, PackageVersionData> m_AllPackageInfoDataDic;

        public PackageVersionData GetPackageInfoData(string packageName)
        {
            m_AllPackageInfoDataDic.TryGetValue(packageName, out PackageVersionData packageInfoData);
            return packageInfoData;
        }

        // 根据 FilterType / FilterOperationType / Search 计算出"筛选包数据"表里要显示的包。
        // Or=满足任一已选条件即可；And=需同时满足；Only=前面已经把 FilterType 收敛成单一条件，直接取该条件结果。
        private void LoadFilterPackageInfoData()
        {
            m_FilterPackageInfoDataList.Clear();
            m_FilterPackageInfoDataDic.Clear();
            var packagesFilterTypeValues = Enum.GetValues(typeof(EPackagesFilterType));

            foreach (var data in m_AllPackageInfoDataDic)
            {
                var name = data.Key;

                if (!string.IsNullOrEmpty(Search) && !Regex.IsMatch(name, Search))
                {
                    continue;
                }

                var infoData = data.Value;
                var add      = false;

                switch (FilterOperationType)
                {
                    case EPackagesFilterOperationType.Only:
                        add = GetResult(FilterType);
                        break;
                    case EPackagesFilterOperationType.Or:
                        add = false;
                        foreach (var value in packagesFilterTypeValues)
                        {
                            var filterTypeValue = (EPackagesFilterType)value;
                            var hasReslut       = FilterType.HasFlag(filterTypeValue);
                            if (hasReslut)
                            {
                                add = GetResult(filterTypeValue);
                                if (add)
                                {
                                    break;
                                }
                            }
                        }

                        break;
                    case EPackagesFilterOperationType.And:
                        add = true;
                        foreach (var value in packagesFilterTypeValues)
                        {
                            var filterTypeValue = (EPackagesFilterType)value;
                            var hasReslut       = FilterType.HasFlag(filterTypeValue);
                            if (hasReslut)
                            {
                                add = GetResult(filterTypeValue);
                                if (!add)
                                {
                                    break;
                                }
                            }
                        }

                        break;
                    default:
                        //Debug.LogError($"未实现的操作类型 {FilterOperationType}");
                        break;
                }

                if (!add) continue;

                m_FilterPackageInfoDataList.Add(infoData);
                m_FilterPackageInfoDataDic.Add(name, infoData);

                continue;

                bool GetResult(EPackagesFilterType filterValue)
                {
                    var result = false;
                    switch (filterValue)
                    {
                        case EPackagesFilterType.All:
                            result = true;
                            break;
                        case EPackagesFilterType.None:
                            result = false;
                            break;
                        case EPackagesFilterType.RD:
                            result = infoData.IsRDPackage;
                            break;
                        case EPackagesFilterType.Update:
                            result = infoData.CanUpdateVersion;
                            break;
                        case EPackagesFilterType.Req:
                            result = infoData.ShowIfReqVersion();
                            break;
                        case EPackagesFilterType.Ban:
                            result = infoData.ShowIfBanReqVersion();
                            break;
                        case EPackagesFilterType.ReBan:
                            result = infoData.ShowIfReBanReqVersion();
                            break;
                        default:
                            //Debug.LogError($"新增了筛选条件 请扩展 {filterValue}");
                            break;
                    }

                    return result;
                }
            }
        }

        private void LoadAllPackageInfoData()
        {
            m_AllPackageInfoDataDic = RD.Serialization.Editor.SerializationUtility.CreateCopy(PackageVersionHelper.PackageVersionAsset.AllPackageVersionData);

            //处理依赖检查
            foreach (var data in m_AllPackageInfoDataDic.Values)
            {
                var name         = data.Name;
                var dependencies = data.DependsOn;
                foreach (var dependency in dependencies)
                {
                    if (!m_AllPackageInfoDataDic.ContainsKey(dependency.Name))
                    {
                        if (dependency.Name.Contains("cn.rendu."))
                        {
                            Debug.LogError($"{name}依赖包{dependency.Name}不存在");
                        }
                        continue;
                    }

                    var target = m_AllPackageInfoDataDic[dependency.Name];

                    if (target.IsRDPackage && !CheckVersion(dependency, target))
                    {
                        Debug.LogError($"[{name}]依赖包[{dependency.Name}] 版本不匹配，依赖版本[{dependency.Version}]，当前版本[{target.Version}]");
                    }
                }
            }
        }

        private readonly Dictionary<string, int[]> m_VersionValueDict = new();

        public const string Pattern = "[^0-9.]";

        private int[] GetVersionValue(string version)
        {
            if (m_VersionValueDict.TryGetValue(version, out int[] value))
            {
                return value;
            }

            var versionSplit = Regex.Replace(version, Pattern, "").Split('.');
            m_VersionValueDict[version] = new int[versionSplit.Length];
            for (int i = 0; i < versionSplit.Length; i++)
            {
                if (!int.TryParse(versionSplit[i], out m_VersionValueDict[version][i]))
                {
                    Debug.LogError($"{version} {i} {versionSplit[i]} 不是数字");
                }
            }

            return m_VersionValueDict[version];
        }

        private bool CheckVersion(DependencyInfo dependencyInfo, PackageVersionData targetData)
        {
            var dependencyVersion  = dependencyInfo.Version;
            var versionValue       = GetVersionValue(dependencyVersion);
            var targetVersionValue = targetData?.VersionValue;
            if (targetVersionValue == null)
            {
                Debug.Log($"{targetData.Name} Version == null");
                return false;
            }

            //长度判断
            if (targetVersionValue.Length != versionValue.Length)
            {
                Debug.Log($"{targetData.Name} Version长度不一致，实际:{targetData.Version} 与 依赖:{dependencyVersion}不匹配");
                return false;
            }

            //大版本号判断
            if (targetVersionValue.Length >= 1 && targetVersionValue[0] != versionValue[0])
            {
                Debug.Log($"{targetData.Name} 大版本号不匹配，实际:{targetData.Version} 与 依赖:{dependencyVersion}不匹配");
                return false;
            }

            //中版本号判断
            if (targetVersionValue.Length >= 2 && targetVersionValue[1] != versionValue[1])
            {
                Debug.Log($"{targetData.Name} 中版本号不匹配，实际:{targetData.Version} 与 依赖:{dependencyVersion}不匹配");
                return false;
            }

            //小版本号判断
            if (targetVersionValue.Length >= 3 && targetVersionValue[2] < versionValue[2])
            {
                Debug.Log($"{targetData.Name} 小版本号不匹配，实际:{targetData.Version} 与 依赖:{dependencyVersion}不匹配");
                return false;
            }

            return true;
        }
    }
}
