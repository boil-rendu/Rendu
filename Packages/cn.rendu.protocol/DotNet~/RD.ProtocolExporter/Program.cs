using System;

namespace RD;

[EnableClass]
internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            ProtocolExporter.Export();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("Protocol Export ok!");
        return 1;
    }
}