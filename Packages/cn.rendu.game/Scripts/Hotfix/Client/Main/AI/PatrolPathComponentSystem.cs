using Unity.Mathematics;
using RD.Core;

namespace RD.Client
{
    public static partial class PatrolPathComponentSystem
    {
        public static float3 GetCurrent(this PatrolPathComponent self)
        {
            return self.path[self.Index];
        }
        
        public static void MoveNext(this PatrolPathComponent self)
        {
            self.Index = ++self.Index % self.path.Length;
        }
    }
}