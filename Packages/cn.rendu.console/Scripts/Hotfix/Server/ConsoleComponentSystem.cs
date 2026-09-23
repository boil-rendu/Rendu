using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RD.Core;

namespace RD.Server
{
    [EntitySystemOf(typeof(ConsoleComponent))]
    [FriendOf(typeof(ModeContext))]
    public static partial class ConsoleComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ConsoleComponent self)
        {
            self.Start().NoContext();
        }

        
        private static async Promise Start(this ConsoleComponent self)
        {
            self.CancellationTokenSource = new CancellationTokenSource();

            // 诊断：stdin 被重定向时 ReadLine 永远等不到输入，表现为"敲命令没反应"
            Console.WriteLine($"[Console] ready. stdin redirected: {Console.IsInputRedirected}");

            while (true)
            {
                try
                {
                    ModeContext modeContex = self.GetComponent<ModeContext>();
                    string line = await Task.Factory.StartNew(() =>
                    {
                        Console.Write($"{modeContex?.Mode ?? ""}> ");
                        return Console.In.ReadLine();
                    }, self.CancellationTokenSource.Token);

                    // stdin 被重定向（Unity 编辑器 / 打包运行时没有可用 stdin）时 ReadLine 返回 null，
                    // 此时控制台不可用；若不退出会在 while(true) 里空转并反复抛 NRE 刷屏。
                    if (line == null)
                    {
                        Console.WriteLine("[Console] stdin 不可用，控制台已退出。");
                        return;
                    }

                    line = line.Trim();

                    switch (line)
                    {
                        case "":
                            break;
                        case "exit":
                            self.RemoveComponent<ModeContext>();
                            break;
                        default:
                        {
                            string[] lines = line.Split(" ");
                            string mode = modeContex == null? lines[0] : modeContex.Mode;

                            IConsoleHandler iConsoleHandler = ConsoleDispatcher.Instance.Get(mode);
                            if (iConsoleHandler == null)
                            {
                                Console.WriteLine($"unknown console mode: {mode}");

                                if (modeContex != null)
                                {
                                    self.RemoveComponent<ModeContext>();
                                }

                                break;
                            }

                            if (modeContex == null)
                            {
                                modeContex = self.AddComponent<ModeContext>();
                                modeContex.Mode = mode;
                            }
                            await iConsoleHandler.Run(self.Fiber(), modeContex, line);
                            break;
                        }
                    }


                }
                catch (Exception e)
                {
                    // Log.Console 受 Options.Console(默认 0) 门控，异常会被静默吞掉，
                    // 这里同步写标准输出，避免"敲了命令没反应"却查不到原因
                    Console.WriteLine(e);
                    Log.Console(e.ToString());
                }
            }
        }
    }
}