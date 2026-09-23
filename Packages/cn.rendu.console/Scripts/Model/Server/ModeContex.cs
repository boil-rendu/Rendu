using RD.Core;

namespace RD.Server
{
    [EntitySystemOf(typeof(ModeContext))]
    public static partial class ModeContexSystem
    {
        [EntitySystem]
        private static void Awake(this ModeContext self)
        {
            self.Mode = "";
        }
        
        [EntitySystem]
        private static void Destroy(this ModeContext self)
        {
            self.Mode = "";
        }
    }
    


    [ComponentOf(typeof(ConsoleComponent))]
    public class ModeContext: Entity, IAwake, IDestroy
    {
        public string Mode = "";
    }
}