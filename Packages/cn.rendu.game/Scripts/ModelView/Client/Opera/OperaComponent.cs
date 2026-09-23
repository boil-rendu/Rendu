using RD.Core;
using UnityEngine;

namespace RD.Client
{
	[ComponentOf(typeof(Scene))]
	public class OperaComponent: Entity, IAwake, IUpdate
    {
        public Vector3 ClickPoint;

	    public int mapMask;

        // UE5 风格视口相机控制状态
        public bool lookHeld;
        public float camYaw;
        public float camPitch;
        public Vector3 lastMousePos;
    }
}
