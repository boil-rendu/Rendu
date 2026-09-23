using RD.Core;

namespace RD.Client
{
    [ComponentOf(typeof(Scene))]
    public class FiberParentComponent: Entity, IAwake
    {
        public int ParentFiberId { get; set; }
    }
}