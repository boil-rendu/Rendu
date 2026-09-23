using UnityEngine;

namespace RD.Recast
{
    /// <summary>
    /// 批量导出配置项（单个场景的导出设置）
    /// </summary>
    [System.Serializable]
    public class BatchExportConfig
    {
        [Tooltip("场景名称（必须与场景文件名一致）")]
        public string sceneName;

        [Tooltip("场景根节点 Prefab（可选，如果为空则使用场景本身）")]
        public GameObject sceneRootPrefab;

        [Tooltip("输出文件名（可选，如果为空则使用场景名）")]
        public string outputFileName;

        [Tooltip("是否启用该场景导出")]
        public bool enabled = true;

        /// <summary>
        /// 获取实际使用的输出文件名
        /// </summary>
        public string GetOutputFileName()
        {
            return string.IsNullOrEmpty(outputFileName) ? sceneName : outputFileName;
        }
    }

    /// <summary>
    /// 批量导出参数（包含 Recast 构建参数）
    /// </summary>
    [System.Serializable]
    public class BatchExportParams
    {
        [Header("批量导出设置")]
        [Tooltip("要导出的场景列表")]
        public BatchExportConfig[] exportConfigs;

        [Tooltip("导出路径（默认使用 PathConfig 配置）")]
        public string exportPath;

        [Tooltip("是否保留中间文件")]
        public bool keepIntermediateFiles = false;

        [Header("Recast 构建参数")]
        [Tooltip("导航网格单元格大小")]
        public float cellSize = 0.1666667f;

        [Tooltip("导航网格单元格高度")]
        public float cellHeight = 0.05f;

        [Tooltip("角色半径")]
        public float agentRadius = 0.5f;

        [Tooltip("角色高度")]
        public float agentHeight = 2.0f;

        [Tooltip("角色能爬上的最大台阶高度")]
        public float agentMaxClimb = 0.25f;

        [Tooltip("角色能跨越的最大斜坡角度（度）")]
        public float agentMaxSlope = 45.0f;

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

        /// <summary>
        /// 转换为 NavMeshBuilderService.BuildParams
        /// </summary>
        public NavMeshBuilderService.BuildParams ToBuildParams()
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
    }

    /// <summary>
    /// 批量导出结果
    /// </summary>
    public class BatchExportResult
    {
        public string SceneName { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public int VertexCount { get; set; }
        public int TriangleCount { get; set; }
        public int PolygonCount { get; set; }
    }
}
