namespace RD.Loader.Editor
{
    public enum PlatformType
    {
        None,
        Android,
        IOS,
        Windows,
        MacOS,
        Linux,
        WebGL
    }

    public enum DevelopMode
    {
        Release = 0,
        Develop = 1,
        Stress = 2,
    }

    /// <summary>
    /// RD菜单顺序
    /// </summary>
    public static class RDMenuItemPriority
    {
        public const int BuildTool = 1001;
        public const int GlobalConfig = 1002;
        public const int ChangeDefine = 1003;
        public const int Compile = 1004;
        public const int Reload = 1005;
        public const int NavMesh = 1006;
        public const int ServerTools = 1007;
        public const int UpdateScriptsReferences = 1008;
        public const int Init = 1009;
        public const int ReGenerateProjectFiles = 1010;
        public const int ReGenerateAsmRef = 1011;
        public const int LinkSln = 1012;
    }
}
