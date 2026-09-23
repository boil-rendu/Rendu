using Unity.Mathematics;

namespace RD
{
    public struct ChangePosition
    {
        public Unit Unit;
        public float3 OldPos;
    }

    public struct ChangeRotation
    {
        public Unit Unit;
    }

    public struct AfterUnitCreate
    {
        public Unit Unit;
    }
}