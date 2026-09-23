// See https://aka.ms/new-console-template for more information

using System;
using System.Threading;
using RD;
using RD.Core;
using RD.Server;

Entry.Init();
            
Launcher launcher = new();
launcher.Start();
            
while (true)
{
    Thread.Sleep(1);
    try
    {
        launcher.Update();
        launcher.LateUpdate();
    }
    catch (Exception e)
    {
        Log.Error(e);
    }
}