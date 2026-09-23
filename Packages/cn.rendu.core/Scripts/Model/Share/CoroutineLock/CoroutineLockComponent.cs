using System;
using System.Collections.Generic;
using RD.Core;

namespace RD
{
    [ComponentOf(typeof(Scene))]
    public class CoroutineLockComponent: Entity, IAwake, IScene, IUpdate
    {
        public Fiber Fiber { get; set; }
        public int SceneType { get; set; }
        
        public readonly Queue<(long, long, int)> nextFrameRun = new();
    }
}