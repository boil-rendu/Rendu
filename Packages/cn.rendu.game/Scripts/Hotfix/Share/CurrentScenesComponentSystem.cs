using RD.Core;

namespace RD
{
    public static partial class CurrentScenesComponentSystem
    {
        public static Scene CurrentScene(this Scene root)
        {
            return root.GetComponent<CurrentScenesComponent>()?.Scene;
        }
    }
}