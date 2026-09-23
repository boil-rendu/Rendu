using System;

namespace RD
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class IgnoreCircularDependencyAttribute : Attribute
    {
    }
}