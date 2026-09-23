using CommandLine;
using RD.Core;

namespace RD.Server
{
    public class CreateRobotArgs: Object
    {
        [Option("Num", Required = false, Default = 1)]
        public int Num { get; set; }
    }
}