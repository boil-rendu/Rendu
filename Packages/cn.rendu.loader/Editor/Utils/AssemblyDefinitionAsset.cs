namespace RD.Loader.Editor
{
    public class AssemblyDefinitionAsset
    {
        public string name;
        public string rootNamespace;
        public string[] references;
        public string[] includePlatforms;
        public string[] excludePlatforms;
        public bool allowUnsafeCode;
        public bool overrideReferences;
        public string[] precompiledReferences;
        public bool autoReferenced;
        public string[] defineConstraints;
        public string[] versionDefines;
        public bool noEngineReferences;
        public string[] optionalUnityReferences;
        public string[] additionalReferences;
        public string compilerOptions;
    }
}
