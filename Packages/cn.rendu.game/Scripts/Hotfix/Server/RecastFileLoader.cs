using System;
using System.IO;
using DotRecast.Detour;
using DotRecast.Detour.Io;
using MongoDB.Bson;
using RD.Core;

namespace RD.Server
{
    [Invoke]
    public class RecastFileReader : AInvokeHandler<NavmeshComponent.RecastFileLoader, byte[]>
    {
        public override byte[] Handle(NavmeshComponent.RecastFileLoader args)
        {
            string fileName = args.Name;
            string recastPath = Path.Combine(ConstValue.RecastPackagePath, "Config", "Recast", $"{fileName}.recast");
            if (!File.Exists(recastPath))
            {
                throw new FileNotFoundException(
                    $"NavMesh file not found: {fileName}（${recastPath}）. Please generate it using Rendu/Recast Tool");
            }

            Log.Debug($"寻路 | 加载导航网格：{recastPath}");
            return File.ReadAllBytes(recastPath);
        }
        
    }
    
}