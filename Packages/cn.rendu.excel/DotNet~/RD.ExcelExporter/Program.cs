using System;

namespace RD;

[EnableClass]
internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            ExcelExporter.Export();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("Excel Export ok!");
        return 1;
    }
}