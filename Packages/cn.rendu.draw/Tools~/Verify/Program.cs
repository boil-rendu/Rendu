using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

internal static class Program
{
    // 这些用例需要 Unity 运行时(ScriptableObject/AssetDatabase/原生对象等), 只能在 Unity Test Runner 内验证。
    // 本包的用例均为纯逻辑, 无需 Unity 运行时; 预留跳过机制以备后用。
    private static readonly string[] RequiresUnityRuntime =
    {
    };

    private static int Main(string[] args)
    {
        var fixtureFilter = args.Length > 0 ? args[0] : null;
        var assembly = typeof(Program).Assembly;

        var passed = 0;
        var failures = new List<string>();
        var skipped = new List<string>();

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || !type.IsClass) continue;
            if (fixtureFilter != null && type.Name.IndexOf(fixtureFilter, StringComparison.OrdinalIgnoreCase) < 0) continue;

            var methods = new List<MethodInfo>();
            foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (m.GetCustomAttribute<TestAttribute>() == null) continue;
                methods.Add(m);
            }

            if (methods.Count == 0) continue;
            Console.WriteLine("[fixture] " + type.Name + " (" + methods.Count + " tests)");

            foreach (var m in methods)
            {
                if (Array.IndexOf(RequiresUnityRuntime, m.Name) >= 0)
                {
                    skipped.Add(type.Name + "." + m.Name);
                    continue;
                }

                object instance;
                try
                {
                    instance = Activator.CreateInstance(type);
                }
                catch (Exception e)
                {
                    failures.Add(type.Name + " <ctor> :: " + e.GetType().Name + ": " + e.Message);
                    continue;
                }

                try
                {
                    m.Invoke(instance, null);
                    passed++;
                }
                catch (TargetInvocationException e)
                {
                    var inner = e.InnerException ?? e;
                    failures.Add(type.Name + "." + m.Name + " :: " + inner.GetType().Name + ": " + inner.Message);
                }
                catch (Exception e)
                {
                    failures.Add(type.Name + "." + m.Name + " :: " + e.GetType().Name + ": " + e.Message);
                }
            }
        }

        Console.WriteLine();
        foreach (var f in failures) Console.WriteLine("FAIL  " + f);
        foreach (var s in skipped) Console.WriteLine("SKIP  " + s + "  (需 Unity Test Runner)");
        Console.WriteLine("== passed: " + passed + ", failed: " + failures.Count + ", skipped: " + skipped.Count + " ==");
        return failures.Count == 0 ? 0 : 1;
    }
}
