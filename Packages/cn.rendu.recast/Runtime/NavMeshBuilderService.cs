using System.IO;
using DotRecast.Core;
using DotRecast.Detour;
using DotRecast.Detour.Io;
using DotRecast.Recast;
using DotRecast.Recast.Geom;
using DotRecast.Recast.Toolset;
using DotRecast.Recast.Toolset.Builder;

namespace RD.Recast
{
    /// <summary>
    /// 导航网格构建器（负责 Recast 导航网格的生成和保存）
    /// </summary>
    public class NavMeshBuilderService
    {
        private readonly PathConfig _pathConfig = new();

        /// <summary>
        /// 构建导航网格参数配置
        /// </summary>
        public struct BuildParams
        {
            // Recast 构建核心参数
            public float cellSize;
            public float cellHeight;
            public float agentRadius;
            public float agentHeight;
            public float agentMaxClimb;
            public float agentMaxSlope;
            
            // 高级参数
            public int minRegionSize;
            public int mergedRegionSize;
            public float edgeMaxLen;
            public float edgeMaxError;
            public int vertsPerPoly;
            public int tileSize;
            public float detailSampleDist;
            public float detailSampleMaxError;

            // 布尔过滤参数
            public bool filterLowHangingObstacles;
            public bool filterLedgeSpans;
            public bool filterWalkableLowHeightSpans;
            
            // 调试参数
            public bool keepIntermediate;
            public bool buildAllTiles;

            /// <summary>
            /// 使用默认值初始化参数
            /// </summary>
            public static BuildParams CreateDefault()
            {
                return new BuildParams
                {
                    cellSize = 0.1666667f,
                    cellHeight = 0.05f,
                    agentRadius = 0.5f,
                    agentHeight = 2.0f,
                    agentMaxClimb = 0.25f,
                    agentMaxSlope = 45.0f,
                    minRegionSize = 8,
                    mergedRegionSize = 20,
                    edgeMaxLen = 12,
                    edgeMaxError = 1.3f,
                    vertsPerPoly = 6,
                    tileSize = 256,
                    detailSampleDist = 6.0f,
                    detailSampleMaxError = 1.0f,
                    filterLowHangingObstacles = false,
                    filterLedgeSpans = false,
                    filterWalkableLowHeightSpans = true,
                    keepIntermediate = true,
                    buildAllTiles = true
                };
            }
        }

        /// <summary>
        /// 从网格数据构建导航网格
        /// </summary>
        public BuildResult BuildNavMesh(float[] vertices, int[] triangles, BuildParams buildParams)
        {
            // 创建几何数据
            RcSampleInputGeomProvider geom = new RcSampleInputGeomProvider(vertices, triangles);

            // 构建导航网格
            TileNavMeshBuilder builder = new TileNavMeshBuilder();
            var result = builder.Build(
                geom,
                buildParams.tileSize,
                RcPartition.WATERSHED,
                buildParams.cellSize,
                buildParams.cellHeight,
                buildParams.agentMaxSlope,
                buildParams.agentHeight,
                buildParams.agentRadius,
                buildParams.agentMaxClimb,
                buildParams.minRegionSize,
                buildParams.mergedRegionSize,
                buildParams.edgeMaxLen,
                buildParams.edgeMaxError,
                buildParams.vertsPerPoly,
                buildParams.detailSampleDist,
                buildParams.detailSampleMaxError,
                buildParams.filterLowHangingObstacles,
                buildParams.filterLedgeSpans,
                buildParams.filterWalkableLowHeightSpans,
                buildParams.keepIntermediate,
                buildParams.buildAllTiles
            );

            if (result.NavMesh == null)
            {
                throw new System.Exception("导航网格构建失败，请检查参数或网格数据！");
            }

            return new BuildResult
            {
                NavMesh = result.NavMesh,
                VertexCount = vertices.Length / 3,
                TriangleCount = triangles.Length / 3,
                PolygonCount = CalculatePolygonCount(result.NavMesh)
            };
        }

        private int CalculatePolygonCount(DtNavMesh navMesh)
        {
            int totalPolyCount = 0;
            for (int i = 0; i < navMesh.GetMaxTiles(); i++)
            {
                DtMeshTile tile = navMesh.GetTile(i);
                if (tile != null && tile.data != null && tile.data.header != null)
                {
                    totalPolyCount += tile.data.header.polyCount;
                }
            }
            return totalPolyCount;
        }

        /// <summary>
        /// 保存导航网格到文件
        /// </summary>
        public void SaveNavMesh(DtNavMesh navMesh, string exportPath, string fileName)
        {
            string navMeshPath = Path.Combine(exportPath, $"{fileName}.recast");

            using (FileStream fs = new FileStream(navMeshPath, FileMode.Create))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                DtMeshSetWriter writer = new DtMeshSetWriter();
                writer.Write(bw, navMesh, RcByteOrder.LITTLE_ENDIAN, true);
            }

            UnityEngine.Debug.Log($"导航网格已保存到：{navMeshPath}");
        }

        /// <summary>
        /// 加载导航网格
        /// </summary>
        public DtNavMesh LoadNavMesh(string exportPath, string fileName)
        {
            string navMeshPath = Path.Combine(exportPath, $"{fileName}.recast");
            if (!File.Exists(navMeshPath))
            {
                UnityEngine.Debug.LogError($"找不到导航网格文件：{navMeshPath}");
                return null;
            }

            using (FileStream fs = new FileStream(navMeshPath, FileMode.Open))
            using (BinaryReader br = new BinaryReader(fs))
            {
                DtMeshSetReader reader = new DtMeshSetReader();
                return reader.Read(br, 6);
            }
        }

        /// <summary>
        /// 构建结果
        /// </summary>
        public class BuildResult
        {
            public DtNavMesh NavMesh { get; set; }
            public int VertexCount { get; set; }
            public int TriangleCount { get; set; }
            public int PolygonCount { get; set; }
        }
    }
}
