using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RD.Recast
{
    /// <summary>
    /// 批量导出服务（负责处理多场景的导航网格批量生成）
    /// </summary>
    public class BatchExportService
    {
        private readonly NavMeshBuilderService _navMeshBuilder = new();
        private readonly PathConfig _pathConfig = new();

        /// <summary>
        /// 批量导出所有场景
        /// </summary>
        public List<BatchExportResult> ExportAll(BatchExportParams exportParams)
        {
            List<BatchExportResult> results = new List<BatchExportResult>();

            // 验证配置
            if (exportParams.exportConfigs == null || exportParams.exportConfigs.Length == 0)
            {
                Debug.LogError("批量导出配置为空！");
                return results;
            }

            // 确保导出目录
            string exportPath = string.IsNullOrEmpty(exportParams.exportPath) 
                ? _pathConfig.RecastExportPath 
                : exportParams.exportPath;

            if (!NavMeshFileManager.EnsureExportDirectory(exportPath))
            {
                return results;
            }

            Debug.Log($"开始批量导出，共 {exportParams.exportConfigs.Length} 个场景");

            // 逐个导出场景
            foreach (BatchExportConfig config in exportParams.exportConfigs)
            {
                if (!config.enabled)
                {
                    Debug.Log($"跳过未启用的场景：{config.sceneName}");
                    continue;
                }

                try
                {
                    BatchExportResult result = ExportSingleScene(config, exportParams, exportPath);
                    results.Add(result);

                    if (result.Success)
                    {
                        Debug.Log($"✅ 场景 [{config.sceneName}] 导出成功");
                    }
                    else
                    {
                        Debug.LogError($"❌ 场景 [{config.sceneName}] 导出失败：{result.Message}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"❌ 场景 [{config.sceneName}] 导出异常：{e.Message}");
                    results.Add(new BatchExportResult
                    {
                        SceneName = config.sceneName,
                        Success = false,
                        Message = $"导出异常：{e.Message}"
                    });
                }
            }

            // 输出统计
            int successCount = results.Count(r => r.Success);
            int totalCount = results.Count;
            Debug.Log($"批量导出完成：成功 {successCount}/{totalCount}");

            return results;
        }

        /// <summary>
        /// 导出单个场景
        /// </summary>
        private BatchExportResult ExportSingleScene(BatchExportConfig config, BatchExportParams exportParams, string exportPath)
        {
            BatchExportResult result = new BatchExportResult
            {
                SceneName = config.sceneName,
                Success = false
            };

            // 1. 加载或获取场景根节点
            Transform sceneRoot = GetSceneRoot(config);
            if (sceneRoot == null)
            {
                result.Message = $"无法获取场景 [{config.sceneName}] 的根节点";
                return result;
            }

            // 2. 收集网格数据
            List<MeshFilter> meshFilters = MeshDataExporter.CollectMeshFilters(sceneRoot);
            if (meshFilters.Count == 0)
            {
                result.Message = $"场景 [{config.sceneName}] 中未找到任何 MeshFilter";
                return result;
            }

            // 3. 合并网格
            Mesh combinedMesh = MeshDataExporter.CombineMeshes(meshFilters);

            // 4. 导出网格数据
            string outputFileName = config.GetOutputFileName();
            MeshDataExporter.ExportMeshData(combinedMesh, exportPath, outputFileName);

            // 5. 构建导航网格
            var buildParams = exportParams.ToBuildParams();
            var meshData = MeshDataExporter.ReadMeshData(exportPath, outputFileName);
            var buildResult = _navMeshBuilder.BuildNavMesh(
                meshData.vertices,
                meshData.triangles,
                buildParams
            );

            // 6. 保存导航网格
            _navMeshBuilder.SaveNavMesh(buildResult.NavMesh, exportPath, outputFileName);

            // 7. 清理中间文件
            if (!exportParams.keepIntermediateFiles)
            {
                NavMeshFileManager.ClearIntermediateFiles(exportPath, outputFileName);
            }

            // 8. 设置结果
            result.Success = true;
            result.Message = "导出成功";
            result.VertexCount = buildResult.VertexCount;
            result.TriangleCount = buildResult.TriangleCount;
            result.PolygonCount = buildResult.PolygonCount;

            return result;
        }

        /// <summary>
        /// 获取场景根节点
        /// </summary>
        private Transform GetSceneRoot(BatchExportConfig config)
        {
            // 如果指定了 Prefab，使用 Prefab
            if (config.sceneRootPrefab != null)
            {
                return config.sceneRootPrefab.transform;
            }

            // 否则使用当前激活场景的根物体
            UnityEngine.SceneManagement.Scene activeScene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            // 查找名为 sceneName 的物体
            foreach (GameObject rootObj in rootObjects)
            {
                if (rootObj.name == config.sceneName)
                {
                    return rootObj.transform;
                }
            }

            // 如果找不到，返回第一个根物体
            if (rootObjects.Length > 0)
            {
                Debug.LogWarning($"未找到名为 [{config.sceneName}] 的根节点，使用第一个根节点");
                return rootObjects[0].transform;
            }

            return null;
        }
    }
}
