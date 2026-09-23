using System.Collections.Generic;

namespace RD.PackageManager.Editor
{
    [System.Serializable]
    public class PackageGitInfo
    {
        public string Name;
        public Dictionary<string, List<string>> ScriptsReferences;
    }
}
