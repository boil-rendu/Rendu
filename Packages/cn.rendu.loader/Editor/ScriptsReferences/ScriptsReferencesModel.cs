using System.Collections.Generic;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 脚本引用数据模型
    /// </summary>
    public class ScriptsReferencesModel
    {
        public Dictionary<string, HashSet<string>> References { get; set; }
        public string FourAssemblyDirectory { get; set; }

        public ScriptsReferencesModel()
        {
            References = new Dictionary<string, HashSet<string>>
            {
                { "Model", new HashSet<string>() },
                { "Hotfix", new HashSet<string>() },
                { "ModelView", new HashSet<string>() },
                { "HotfixView", new HashSet<string>() }
            };
            FourAssemblyDirectory = null;
        }
    }
}
