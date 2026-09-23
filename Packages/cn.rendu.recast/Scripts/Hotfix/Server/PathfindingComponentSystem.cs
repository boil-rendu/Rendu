using System;
using System.Collections.Generic;
using System.IO;
using DotRecast.Core;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using DotRecast.Detour.Io;
using RD.Core;
using Unity.Mathematics;

namespace RD.Server
{
    [EntitySystemOf(typeof(PathfindingComponent))]
    public static partial class PathfindingComponentSystem
    {
        [EntitySystem]
        private static void Awake(this PathfindingComponent self, string name)
        {
            self.Name = name;
            byte[] navMeshBytes = NavmeshComponent.Instance.Get(name);
            if (navMeshBytes == null || navMeshBytes.Length == 0)
            {
                throw new ArgumentException("导航网格二进制字节数组为空！");
            }

            // 用 MemoryStream 读取字节数组，解析为 DtNavMesh
            using (MemoryStream ms = new MemoryStream(navMeshBytes))
            using (BinaryReader br = new BinaryReader(ms))
            {
                DtMeshSetReader reader = new DtMeshSetReader();
                self.navMesh = reader.Read(br, 6); // 6 是 DotRecast 固定的版本号

                if (self.navMesh == null)
                {
                    throw new Exception("解析导航网格失败！文件可能损坏或版本不兼容。");
                }
                self.filter = new DtQueryDefaultFilter();
                self.query = new DtNavMeshQuery(self.navMesh);
            }
            
        }

        [EntitySystem]
        private static void Destroy(this PathfindingComponent self)
        {
            self.Name = string.Empty;
            self.navMesh = null;
        }
        
        public static void Find(this PathfindingComponent self, float3 start, float3 target, List<float3> result)
        {
            if (self.navMesh == null)
            {
                throw new Exception($"self.navMes = null: {self.Scene().Name}");
            }

            RcVec3f startPos = new(-start.x, start.y, start.z);
            RcVec3f endPos = new(-target.x, target.y, target.z);

            long startRef;
            long endRef;
            RcVec3f startPt;
            RcVec3f endPt;
            
            self.query.FindNearestPoly(startPos, self.extents, self.filter, out startRef, out startPt, out _);
            self.query.FindNearestPoly(endPos, self.extents, self.filter, out endRef, out endPt, out _);

            self.query.FindPath(startRef, endRef, startPt, endPt, self.filter, self.polys, out var pathCount, PathfindingComponent.MAX_POLYS);

            if (0 >= pathCount)
            {
                return;
            }
            
            // In case of partial path, make sure the end point is clamped to the last polygon.
            RcVec3f epos = new(endPt.X, endPt.Y, endPt.Z);
            if (self.polys[pathCount - 1] != endRef)
            {
                DtStatus dtStatus = self.query.ClosestPointOnPoly(self.polys[pathCount - 1], endPt, out RcVec3f closest, out bool _);
                if (dtStatus.Succeeded())
                {
                    epos = closest;
                }
            }

            self.query.FindStraightPath(startPt, epos, self.polys, pathCount, self.straightPath, out var straightPathCount, PathfindingComponent.MAX_POLYS, DtStraightPathOptions.DT_STRAIGHTPATH_ALL_CROSSINGS);

            for (int i = 0; i < straightPathCount; ++i)
            {
                RcVec3f pos = self.straightPath[i].pos;
                result.Add(new float3(-pos.X, pos.Y, pos.Z));
            }
        }
    }
}