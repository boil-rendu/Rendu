using System.Diagnostics;
using RD.Init.Editor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 启动 MongoDB 命令
    /// </summary>
    public class StartMongoCommand : CommandBase
    {
        public override void Execute()
        {
            string arguments = @"--dbpath=db";
            RunProcess("mongod", arguments, "../Database/bin/");
        }

        private void RunProcess(string fileName, string arguments, string workingDirectory)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = false
            };
            Process.Start(psi);
        }
    }
}
