using RD.Core;

namespace RD.Server
{
    /// <summary>
    /// 性能统计控制台命令：
    ///   P        输出热路径累计统计
    ///   P on     开启热路径统计
    ///   P off    关闭热路径统计
    ///   P reset  清空累计数据
    /// </summary>
    [ConsoleHandler(ConsoleMode.Profiler)]
    public class ProfilerConsoleHandler: IConsoleHandler
    {
        public async Promise Run(Fiber fiber, ModeContext contex, string content)
        {
            switch (content)
            {
                case "P":
                    Profiler.Dump();
                    break;
                case "P on":
                    Profiler.Enabled = true;
                    Profiler.Print("[Profiler] enabled");
                    break;
                case "P off":
                    Profiler.Enabled = false;
                    Profiler.Print("[Profiler] disabled");
                    break;
                case "P reset":
                    Profiler.Reset();
                    Profiler.Print("[Profiler] reset");
                    break;
                default:
                    Profiler.Print("usage: P / P on / P off / P reset");
                    break;
            }

            await Promise.CompletedTask;
        }
    }
}
