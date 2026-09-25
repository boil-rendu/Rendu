#if UNITY_EDITOR

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    public class CreateScriptData
    {
        public bool             AutoRefresh;
        public bool             ShowTips;
        public string           Namespace; //命名空间
        public string           Name;
        public string           Desc;
        public EScriptComponentType ComponentTpye;
        public EScriptLifeType      LifeTpye;
        public string           ComponentPath;
        public string           SystemPath;
    }
}
#endif