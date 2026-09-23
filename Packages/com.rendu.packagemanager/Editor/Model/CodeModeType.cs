using System;

namespace RD.PackageManager.Editor
{
    [Flags]
    public enum CodeModeType
    {
        None       = 0,
        Client    = 1 << 0,
        Server    = 1 << 1,
        Share     = 1 << 2,
    }
    
}
