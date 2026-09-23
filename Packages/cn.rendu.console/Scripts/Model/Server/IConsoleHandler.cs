using RD.Core;

namespace RD.Server
{
    public interface IConsoleHandler
    {
        Promise Run(Fiber fiber, ModeContext contex, string content);
    }
}