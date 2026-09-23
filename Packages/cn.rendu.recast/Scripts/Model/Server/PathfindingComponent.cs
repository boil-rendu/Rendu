using System;
using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using RD.Core;

namespace RD.Server
{
    /// <summary>
    /// 同一块地图可能有多种寻路数据，玩家可以随时切换，怪物也可能跟玩家的寻路不一样，寻路组件应该挂在Unit上
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class PathfindingComponent: Entity, IAwake<string>, IDestroy
    {
        public const int MAX_POLYS = 256;
        
        public const int FindRandomNavPosMaxRadius = 15000;  // 随机找寻路点的最大半径
        
        public RcVec3f extents = new(15, 10, 15);
        
        public string Name;
        
        public DtNavMesh navMesh;
        
        public long[] polys = new long[MAX_POLYS];

        public IDtQueryFilter filter;
        
        public DtStraightPath[] straightPath = new DtStraightPath[MAX_POLYS];

        public DtNavMeshQuery query;
    }
}