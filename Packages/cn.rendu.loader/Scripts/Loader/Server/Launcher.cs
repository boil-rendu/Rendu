#if DOTNET

using System;
using System.Threading;
using CommandLine;
using RD.Core;

namespace RD.Server
{
    /// <summary>
    /// 服务器端启动器
    /// </summary>
    public class Launcher
    {
        public void Start()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    Log.Error(e.ExceptionObject.ToString());
                };
				
                // 命令行参数
                Parser.Default.ParseArguments<Options>(System.Environment.GetCommandLineArgs())
                        .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
                        .WithParsed((o)=>World.Instance.AddSingleton(o));
				
                World.Instance.AddSingleton<Logger>().Log = new NLogger(Options.Instance.SceneName, Options.Instance.Process, 0);

                // 日志缺失会导致运行期静默降级：进程"看起来正常"却收不到任何日志，
                // 必须在启动阶段就暴露出来。
                Log.EnsureConfigured();

                Promise.ExceptionHandler += Log.Error;
                World.Instance.AddSingleton<TimeInfo>();
                World.Instance.AddSingleton<FiberManager>();

                World.Instance.AddSingleton<CodeLoader>().Start();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public void Update()
        {
            TimeInfo.Instance.Update();
            FiberManager.Instance.Update();
        }

        public void LateUpdate()
        {
            FiberManager.Instance.LateUpdate();
        }
    }
}
#endif