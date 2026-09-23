using System.Collections.Generic;
using RD.Core;

namespace RD.Server
{
    [ComponentOf(typeof(Scene))]
    public class RobotManagerComponent: Entity, IAwake, IDestroy
    {
        public HashSet<int> robots = new();
    }
}