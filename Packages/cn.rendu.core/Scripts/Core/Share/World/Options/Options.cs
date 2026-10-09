using CommandLine;
using System;
using System.Collections.Generic;

namespace RD.Core
{
    /// <summary>
    /// 进程启动参数（命令行选项）的全局单例：在 <see cref="Singleton{T}"/> 基类上叠了 <c>CommandLineParser</c> 字段，
    /// 由 <c>Program</c>/<c>Game</c> 入口解析启动参数后 <c>Singleton.Instance = new Options {...}</c> 注入。
    /// 业务侧（<see cref="Options.Process"/>、<see cref="Options.Develop"/>、<see cref="Options.LogLevel"/> 等）从这里读进程级配置。
    /// </summary>
    public class Options: Singleton<Options>
    {
        /// <summary>当前进程所属的 <c>Scene</c> 名（与 <c>SceneType</c> 常量对应），默认 <c>All</c> 表示所有 Scene。</summary>
        [Option("SceneName", Required = false, Default = "All", HelpText = "define in SceneType class")]
        public string SceneName { get; set; }

        /// <summary>启动配置（StartConfig/Localhost 等）路径，定位 <c>StartSceneConfig</c> 加载源。</summary>
        [Option("StartConfig", Required = false, Default = "StartConfig/Localhost")]
        public string StartConfig { get; set; }

        /// <summary>进程号（同进程组内唯一），用于 <see cref="IdStruct"/> 编码与日志分组。</summary>
        [Option("Process", Required = false, Default = 1)]
        public int Process { get; set; }

        /// <summary>开发模式：0 正式 / 1 开发 / 2 压测，控制 debug 资源加载、EditorOnly 入口开关、压测相关功能。</summary>
        [Option("Develop", Required = false, Default = 1, HelpText = "develop mode, 0正式 1开发 2压测")]
        public int Develop { get; set; }

        /// <summary>日志等级阈值（与 <see cref="ILog"/> 的 5 级对应），低于此等级的不输出。</summary>
        [Option("LogLevel", Required = false, Default = 0)]
        public int LogLevel { get; set; }

        /// <summary>是否输出到控制台（1=开），发布时置 0 关闭 stdout。</summary>
        [Option("Console", Required = false, Default = 1)]
        public int Console { get; set; }
    }
}
