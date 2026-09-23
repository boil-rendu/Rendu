using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Net;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using DotRecast.Detour.Io;
using DotRecast.Recast.Toolset.Builder;

namespace RD.Recast
{
    /// <summary>
    /// 统一导航网格构建组件（协调网格导出、导航构建、文件管理）
    /// </summary>
    public class NavMeshExporter : MonoBehaviour
    {
        #region 服务组件

        private readonly NavMeshBuilderService _navMeshBuilder = new();
        private readonly BatchExportService _batchExporter = new();
        private readonly PathConfig _pathConfig = new();

        #endregion

        #region 基础配置

        [Header("📁 文件配置")]
        [Tooltip("导出/保存路径（默认使用 PathConfig 配置）")]
        public string exportPath;

        [Tooltip("文件名称（默认使用当前场景名）")]
        public string fileName;

        [Tooltip("是否保留中间文件（vertices/triangles/obj）")]
        public bool keepIntermediateFiles = false;

        [Header("🌎 场景配置")]
        [Tooltip("场景根节点（自动取当前物体，递归收集所有 MeshFilter）")]
        public Transform sceneRoot;

        #endregion

        #region Recast 构建参数

        [Header("🧱 导航网格构建参数")]
        [Tooltip("导航网格单元格大小（越小越精细，性能越低）")]
        public float cellSize = 0.1666667f;

        [Tooltip("导航网格单元格高度")]
        public float cellHeight = 0.05f;

        [Tooltip("角色半径（决定障碍物旁可通行区域大小）")]
        public float agentRadius = 0.5f;

        [Tooltip("角色高度（决定能通过的缝隙高度）")]
        public float agentHeight = 2.0f;

        [Tooltip("角色能爬上的最大台阶高度")]
        public float agentMaxClimb = 0.25f;

        [Tooltip("角色能跨越的最大斜坡角度（度）")]
        public float agentMaxSlope = 45.0f;

        [Header("🔧 高级构建参数（一般无需修改）")]
        [Tooltip("最小区域大小")]
        public int minRegionSize = 8;

        [Tooltip("合并区域大小")]
        public int mergedRegionSize = 20;

        [Tooltip("最大边长度")]
        public float edgeMaxLen = 12;

        [Tooltip("最大简化误差")]
        public float edgeMaxError = 1.3f;

        [Tooltip("每多边形顶点数")]
        public int vertsPerPoly = 6;

        [Tooltip("瓦片尺寸")]
        public int tileSize = 256;

        [Tooltip("细节网格采样距离")]
        public float detailSampleDist = 6.0f;

        [Tooltip("细节网格最大误差")]
        public float detailSampleMaxError = 1.0f;

        #endregion

        #region 生命周期

        private void Awake()
        {
            InitDefaultValues();
        }

        private void Reset()
        {
            InitDefaultValues();
        }

        /// <summary>
        /// 初始化默认参数
        /// </summary>
        private void InitDefaultValues()
        {
            // 路径默认使用配置文件
            if (string.IsNullOrEmpty(exportPath))
            {
                exportPath = _pathConfig.RecastExportPath;
            }

            // 文件名默认使用场景名
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            }

            // 场景根节点默认指向当前物体
            if (sceneRoot == null)
            {
                sceneRoot = transform;
            }
        }

        #endregion

        #region 核心功能 - ContextMenu

        [ContextMenu("🚀 一键生成导航网格")]
        public void GenerateNavMesh()
        {
            if (!ValidateConfig())
            {
                Debug.LogError("配置校验失败，终止导航网格生成！");
                return;
            }

            try
            {
                // 导出并构建
                ExportAndBuild();

                // 清理中间文件
                if (!keepIntermediateFiles)
                {
                    NavMeshFileManager.ClearIntermediateFiles(exportPath, fileName);
                    Debug.Log("已清理中间文件，仅保留导航网格文件");
                }

                Debug.Log("<color=green>✅ 导航网格生成完成！</color>");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"<color=red>❌ 导航网格生成失败：{e.Message}</color>");
            }
        }

        [ContextMenu("📤 仅导出场景网格")]
        public void ExportSceneMesh()
        {
            if (!ValidateConfig())
            {
                Debug.LogError("配置校验失败，终止网格导出！");
                return;
            }

            // 收集并导出
            List<MeshFilter> meshFilters = MeshDataExporter.CollectMeshFilters(sceneRoot);
            
            Debug.Log($"场景根节点：{sceneRoot.name}");
            Debug.Log($"收集到 {meshFilters.Count} 个带 MeshFilter 的物体");
            
            if (meshFilters.Count == 0)
            {
                Debug.LogError($"错误详情：在 [{sceneRoot.name}] 及其子物体中未找到任何 MeshFilter");
                Debug.LogError("请检查：");
                Debug.LogError("1. 场景中是否有带 MeshFilter 的物体");
                Debug.LogError("2. sceneRoot 是否指向正确的根节点");
                throw new System.Exception("场景中未找到任何带 MeshFilter 的物体！");
            }

            Mesh combinedMesh = MeshDataExporter.CombineMeshes(meshFilters);
            MeshDataExporter.ExportMeshData(combinedMesh, exportPath, fileName);

            Debug.Log($"已导出 {meshFilters.Count} 个物体的合并网格");
        }

        [ContextMenu("🏗️ 仅构建导航网格（需先导出网格）")]
        public void BuildNavMesh()
        {
            if (!ValidateConfig())
            {
                Debug.LogError("配置校验失败，终止导航构建！");
                return;
            }

            ExportAndBuild();
        }

        [ContextMenu("🗑️ 清理所有生成文件")]
        public void ClearAllFiles()
        {
            // 清理缓存的可视化数据
            _visualNavMesh = null;
            
            NavMeshFileManager.ClearAllFiles(exportPath, fileName);
        }

        /// <summary>
        /// 批量导出多个场景的导航网格
        /// </summary>
        [ContextMenu("📦 批量导出导航网格（需配置 BatchExportParams）")]
        public void BatchExportNavMesh(BatchExportParams exportParams)
        {
            if (exportParams == null || exportParams.exportConfigs == null || exportParams.exportConfigs.Length == 0)
            {
                Debug.LogError("批量导出参数未配置！");
                return;
            }

            try
            {
                List<BatchExportResult> results = _batchExporter.ExportAll(exportParams);
                
                // 输出详细结果
                foreach (var result in results)
                {
                    if (result.Success)
                    {
                        Debug.Log($"✅ [{result.SceneName}] 成功 - 顶点:{result.VertexCount}, 三角面:{result.TriangleCount}, 多边形:{result.PolygonCount}");
                    }
                    else
                    {
                        Debug.LogError($"❌ [{result.SceneName}] 失败 - {result.Message}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"<color=red>❌ 批量导出失败：{e.Message}</color>");
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 导出并构建导航网格（核心流程）
        /// </summary>
        private void ExportAndBuild()
        {
            // 1. 读取或导出网格数据
            (float[] vertices, int[] triangles) = LoadOrExportMeshData();

            // 2. 构建导航网格
            var buildParams = GetBuildParams();
            var buildResult = _navMeshBuilder.BuildNavMesh(vertices, triangles, buildParams);

            // 3. 保存导航网格
            _navMeshBuilder.SaveNavMesh(buildResult.NavMesh, exportPath, fileName);

            // 4. 输出统计信息
            Debug.Log($"导航网格统计：\n" +
                      $"顶点数：{buildResult.VertexCount}\n" +
                      $"三角面数：{buildResult.TriangleCount}\n" +
                      $"导航多边形数：{buildResult.PolygonCount}");
        }

        /// <summary>
        /// 加载已导出的网格数据，如果不存在则重新导出
        /// </summary>
        private (float[] vertices, int[] triangles) LoadOrExportMeshData()
        {
            // 尝试读取已有数据
            var data = MeshDataExporter.ReadMeshData(exportPath, fileName);
            if (data.vertices != null && data.triangles != null)
            {
                Debug.Log("使用已导出的网格数据");
                return data;
            }

            // 重新导出
            Debug.Log("未找到已导出网格数据，开始重新导出...");
            ExportSceneMesh();
            return MeshDataExporter.ReadMeshData(exportPath, fileName);
        }

        /// <summary>
        /// 从当前配置获取构建参数
        /// </summary>
        private NavMeshBuilderService.BuildParams GetBuildParams()
        {
            return new NavMeshBuilderService.BuildParams
            {
                cellSize = cellSize,
                cellHeight = cellHeight,
                agentRadius = agentRadius,
                agentHeight = agentHeight,
                agentMaxClimb = agentMaxClimb,
                agentMaxSlope = agentMaxSlope,
                minRegionSize = minRegionSize,
                mergedRegionSize = mergedRegionSize,
                edgeMaxLen = edgeMaxLen,
                edgeMaxError = edgeMaxError,
                vertsPerPoly = vertsPerPoly,
                tileSize = tileSize,
                detailSampleDist = detailSampleDist,
                detailSampleMaxError = detailSampleMaxError,
                filterLowHangingObstacles = false,
                filterLedgeSpans = false,
                filterWalkableLowHeightSpans = true,
                keepIntermediate = true,
                buildAllTiles = true
            };
        }

        /// <summary>
        /// 校验基础配置
        /// </summary>
        private bool ValidateConfig()
        {
            if (sceneRoot == null)
            {
                Debug.LogError("场景根节点未指定！");
                return false;
            }

            return NavMeshFileManager.EnsureExportDirectory(exportPath);
        }

        #endregion

        #region 可视化 - 导航网格 & 寻路路径

        [Header("🎨 可视化配置")]
        [Tooltip("导航网格绘制颜色")]
        public Color navMeshColor = new Color(0, 1, 1, 0.3f);

        [Tooltip("寻路路径绘制颜色")]
        public Color pathColor = Color.red;
        [Tooltip("寻路路径线宽")]
        public float pathLineWidth = 0.1f;
        
        [Tooltip("导航网格绘制模式")]
        public DrawMode drawMode = DrawMode.Wireframe;
        
        /// <summary>
        /// 导航网格绘制模式
        /// </summary>
        public enum DrawMode
        {
            Wireframe,    // 线框模式
            Filled,       // 填充模式
            Both          // 两者都显示
        }

        // 临时存储加载的导航网格（用于可视化）
        private DtNavMesh _visualNavMesh;

        /// <summary>
        /// Editor 下绘制导航网格 Gizmos
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (_visualNavMesh == null && !string.IsNullOrEmpty(exportPath) && !string.IsNullOrEmpty(fileName))
            {
                string navMeshPath = Path.Combine(exportPath, $"{fileName}.recast");
                if (File.Exists(navMeshPath))
                {
                    _visualNavMesh = _navMeshBuilder.LoadNavMesh(exportPath, fileName);
                }
            }

            if (_visualNavMesh != null)
            {
                Gizmos.color = navMeshColor;
                DrawNavMeshPolygons(_visualNavMesh);
            }
        }

        /// <summary>
        /// 绘制导航网格的所有多边形
        /// </summary>
        private void DrawNavMeshPolygons(DtNavMesh navMesh)
        {
            for (int tileIdx = 0; tileIdx < navMesh.GetMaxTiles(); tileIdx++)
            {
                DtMeshTile tile = navMesh.GetTile(tileIdx);
                if (tile == null || tile.data == null) continue;

                DtPoly[] polys = tile.data.polys;
                float[] verts = tile.data.verts;
                for (int p = 0; p < tile.data.header.polyCount; p++)
                {
                    DtPoly poly = polys[p];
                    if (poly.GetPolyType() != DtPolyTypes.DT_POLYTYPE_GROUND) continue;

                    List<Vector3> polyVerts = new List<Vector3>();
                    for (int v = 0; v < poly.vertCount; v++)
                    {
                        int vertIdx = poly.verts[v];
                        float x = verts[vertIdx * 3];
                        float y = verts[vertIdx * 3 + 1];
                        float z = verts[vertIdx * 3 + 2];
                        polyVerts.Add(new Vector3(-x, y, z));
                    }

                    if (polyVerts.Count >= 3)
                    {
                        
                        // 根据绘制模式决定如何显示
                        switch (drawMode)
                        {
                            case DrawMode.Filled:
                                // 只填充
                                Gizmos.DrawMesh(MeshCreator.CreatePolygonMesh(polyVerts), Vector3.zero, Quaternion.identity);
                                break;
                                
                            case DrawMode.Wireframe:
                                // 只线框
                                Gizmos.DrawWireMesh(MeshCreator.CreatePolygonMesh(polyVerts), Vector3.zero,
                                    Quaternion.identity);
                                break;
                                
                            case DrawMode.Both:
                                // 两者都显示
                                Gizmos.DrawMesh(MeshCreator.CreatePolygonMesh(polyVerts), Vector3.zero, Quaternion.identity);
                                Gizmos.DrawWireMesh(MeshCreator.CreatePolygonMesh(polyVerts), Vector3.zero,
                                    Quaternion.identity);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 辅助类：创建多边形 Mesh（用于 Gizmos 绘制）
        /// </summary>
        public static class MeshCreator
        {
            public static Mesh CreatePolygonMesh(List<Vector3> vertices)
            {
                Mesh mesh = new Mesh();
                mesh.vertices = vertices.ToArray();

                // 生成三角面（简单三角化：以第一个顶点为中心）
                List<int> triangles = new List<int>();
                for (int i = 1; i < vertices.Count - 1; i++)
                {
                    triangles.Add(0);
                    triangles.Add(i);
                    triangles.Add(i + 1);
                }

                mesh.triangles = triangles.ToArray();
                mesh.RecalculateNormals();
                return mesh;
            }
        }

        #endregion

        #region 测试寻路 - 可视化路径

        [Header("🧪 测试寻路（仅 Editor）")]
        [Tooltip("寻路起点")]
        public Vector3 pathStart;

        [Tooltip("寻路终点")]
        public Vector3 pathEnd;

        [ContextMenu("📏 测试寻路并绘制路径")]
        public void TestPathFinding()
        {
            if (_visualNavMesh == null)
            {
                string navMeshPath = Path.Combine(exportPath, $"{fileName}.recast");
                if (!File.Exists(navMeshPath))
                {
                    Debug.LogError("导航网格文件不存在，请先生成！");
                    return;
                }

                _visualNavMesh = _navMeshBuilder.LoadNavMesh(exportPath, fileName);
            }

            DtNavMeshQuery query = new DtNavMeshQuery(_visualNavMesh);
            List<Vector3> pathPoints = new List<Vector3>();
            bool findPath = FindPath(query, pathStart, pathEnd, pathPoints);

            if (findPath)
            {
                Debug.Log($"<color=green>✅ 寻路成功，路径点数量：{pathPoints.Count}</color>");
                DrawPathGizmos(pathPoints);
            }
            else
            {
                Debug.LogError("<color=red>❌ 寻路失败，起点/终点不可达（比如台阶未被导航网格覆盖）</color>");
            }
        }

        /// <summary>
        /// 寻路核心逻辑
        /// </summary>
        private bool FindPath(DtNavMeshQuery query, Vector3 start, Vector3 end, List<Vector3> pathPoints)
        {
            RcVec3f startVec = new RcVec3f(start.x, start.y, start.z);
            RcVec3f endVec = new RcVec3f(end.x, end.y, end.z);

            query.FindNearestPoly(startVec, new RcVec3f(agentRadius, agentHeight, agentRadius),
                null, out long startRef, out RcVec3f startNearest, out _);
            query.FindNearestPoly(endVec, new RcVec3f(agentRadius, agentHeight, agentRadius), null,
                out long endRef, out RcVec3f endNearest, out _);

            if (startRef == 0 || endRef == 0)
            {
                Debug.LogError("起点/终点不在导航网格内！");
                return false;
            }

            long[] pathRefs = new long[1024];
            DtStatus pathStatus = query.FindPath(startRef, endRef, startNearest, endNearest, null, pathRefs, out int pathCount, pathRefs.Length);
            if (pathCount == 0) return false;

            DtStraightPath[] straightPath = new DtStraightPath[1024];
            int straightPathCount;
            query.FindStraightPath(startNearest, endNearest, pathRefs, pathCount, straightPath, out straightPathCount, 1024, 0);

            for (int i = 0; i < straightPathCount; i++)
            {
                pathPoints.Add(new Vector3(straightPath[i].pos.X, straightPath[i].pos.Y, straightPath[i].pos.Z));
            }

            return pathPoints.Count > 0;
        }

        /// <summary>
        /// 绘制寻路路径（Gizmos）
        /// </summary>
        private void DrawPathGizmos(List<Vector3> pathPoints)
        {
            Gizmos.color = pathColor;
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                for (float offset = -pathLineWidth; offset <= pathLineWidth; offset += 0.02f)
                {
                    Vector3 from = pathPoints[i] + new Vector3(offset, 0, offset);
                    Vector3 to = pathPoints[i + 1] + new Vector3(offset, 0, offset);
                    Gizmos.DrawLine(from, to);
                }

                Gizmos.DrawSphere(pathPoints[i], 0.1f);
            }

            Gizmos.DrawSphere(pathPoints[pathPoints.Count - 1], 0.15f);
        }

        #endregion
    }
}
