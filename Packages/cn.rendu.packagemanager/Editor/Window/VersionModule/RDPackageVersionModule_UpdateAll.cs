
using System;
using System.Collections.Generic;
using System.IO;
using RD.Draw;
using UnityEditor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    public partial class RDPackageVersionModule
    {
        // 本文件承载"更新当前所有"按钮：对筛选结果里每个可更新包做"删旧目录→改依赖版本→请求服务端"的批量覆盖更新。
        private bool ShowIfUpdateAll()
        {
            return FilterType == EPackagesFilterType.Update && m_FilterPackageInfoDataList.Count > 0;
        }

        [Button("更新当前所有", 50)]
        [GUIColor(0f, 1f, 0f)]
        [BoxGroup("筛选包数据", centerLabel: true)]
        [ShowIf("ShowIfUpdateAll")]
        [PropertyOrder(99)]
        private void UpdateAll()
        {
            UnityTipsHelper.CallBack($"确定更新所有版本 {m_FilterPackageInfoDataList.Count}个包 \n \n当前更新为覆盖更新模式!!!\n如果需要合并更新请自行解决!!!\n请确保网络没有问题!!!", () =>
            {
                List<string> allName = new();
                foreach (var packageInfo in m_FilterPackageInfoDataList)
                {
                    if (UpdateDependencies(packageInfo))
                    {
                        allName.Add(packageInfo.Name);
                    }
                }

                RequestUpdateAll(allName);
            });
        }

        private bool UpdateDependencies(PackageVersionData packageData)
        {
            var packagePath = Application.dataPath.Replace("Assets", "Packages") + "/" + packageData.Name;

            try
            {
                if (!Directory.Exists(packagePath))
                {
                    return false;
                }

                Directory.Delete(packagePath, true);

                if (Directory.Exists(packagePath))
                {
                    Debug.LogError("删除失败 文件还存在");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"删除文件失败 {e.Message}");
                return false;
            }

            foreach (var data in packageData.DependedBy)
            {
                var packageInfo = GetPackageInfoData(data.Name);
                if (packageInfo == null)
                {
                    return false;
                }

                data.Version = packageData.LastVersion;

                foreach (var info in packageInfo.DependsOn)
                {
                    if (info.Name == packageData.Name)
                    {
                        info.Version = packageData.LastVersion;
                        break;
                    }
                }
            }

            return true;
        }

        private void RequestUpdateAll(List<string> allName)
        {
            if (allName == null || allName.Count == 0)
            {
                Debug.LogError("没有需要更新的包");
                return;
            }

            RDPackageManagerWindow.CloseWindow();
            EditorUtility.DisplayCancelableProgressBar("同步信息", $"正在更新全部包（{allName.Count} 个）...", 0f);
            new PackageRequestAddAndRemove(allName, (infos) =>
            {
                // 等待期间用户可能点了取消: 关框走人, 跳过本地收尾(服务端已生效, 需重跑一次才能应用)
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
    }
}
