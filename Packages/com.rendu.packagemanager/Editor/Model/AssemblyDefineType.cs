using System;

namespace RD.PackageManager.Editor
{
    [Flags]
    public enum AssemblyDefineType
    {
        None       = 0,
        Runtime    = 1 << 0,
        Editor     = 1 << 1,
        Hotfix     = 1 << 2,
        HotfixView = 1 << 3,
        Model      = 1 << 4,
        ModelView  = 1 << 5,
    }
}