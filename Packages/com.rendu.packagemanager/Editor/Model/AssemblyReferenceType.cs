using System;

namespace RD.PackageManager.Editor
{
    [Flags]
    public enum AssemblyReferenceType
    {
        None       = 0,
        Hotfix     = 1 << 0,
        HotfixView = 1 << 1,
        Model      = 1 << 2,
        ModelView  = 1 << 3,
    }
}
