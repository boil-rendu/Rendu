using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>程序集与类型反射工具</summary>
    public static class AssemblyHelper
    {
        /// <summary>把多个程序集里的所有类型汇总为「完整类名 → Type」的字典</summary>
        public static Dictionary<string, Type> GetAssemblyTypes(params Assembly[] args)
        {
            Dictionary<string, Type> types = new Dictionary<string, Type>();

            foreach (Assembly ass in args)
            {
                foreach (Type type in ass.GetTypes())
                {
                    if (type.FullName != null)
                    {
                        types[type.FullName] = type;
                    }
                }
            }

            return types;
        }

        //获取目标程序集下指定的所有特性
        //如果没有传入程序集默认使用当前程序集
        /// <summary>取出目标程序集下所有带 TAttribute 特性的类；不传程序集时用当前（框架）程序集，其他程序集需自行传入</summary>
        public static List<Type> GetClassesWithAttribute<TAttribute>(Assembly targetAssembly = null) where TAttribute : Attribute
        {
            var classes  = new List<Type>();
            var assembly = targetAssembly ??= Assembly.GetExecutingAssembly(); //默认的程序集是当前框架 其他程序集使用请传入自己的程序集
            var types    = assembly.GetTypes();

            foreach (Type type in types)
            {
                if (type.GetCustomAttribute<TAttribute>() != null)
                {
                    classes.Add(type);
                }
            }

            return classes;
        }

        /// <summary>按名称在已加载的程序集中查找；找不到则报错并返回 null</summary>
        public static Assembly GetAssembly(string assemblyName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assemblie in assemblies)
            {
                if (assemblie.GetName().Name == assemblyName)
                {
                    return assemblie;
                }
            }

            Debug.LogError($"没有找到这个程序集 {assemblyName}");
            return null;
        }

        /// <summary>按名称批量查找已加载的程序集（找不到的会被静默忽略）</summary>
        public static Assembly[] GetAssemblys(string[] assemblysName)
        {
            var assemblies       = AppDomain.CurrentDomain.GetAssemblies();
            var targetAssemblies = new List<Assembly>();
            foreach (var assemblie in assemblies)
            {
                var assemblieName = assemblie.GetName().Name;

                foreach (var targetAssemblyName in assemblysName)
                {
                    if (assemblieName == targetAssemblyName)
                    {
                        targetAssemblies.Add(assemblie);
                    }
                }
            }

            return targetAssemblies.ToArray();
        }

        //获取指定程序集的所有type
        /// <summary>取指定程序集内的所有类型</summary>
        public static Type[] GetLogicTypes(string[] assemblyNames)
        {
            return AppDomain.CurrentDomain.GetTypesByAssemblyName(assemblyNames);
        }

        //找所有程序集的type
        /// <summary>取当前 AppDomain 内所有程序集的类型</summary>
        public static Type[] GetAllTypes()
        {
            return AppDomain.CurrentDomain.GetAllTypes();
        }
    }
}
