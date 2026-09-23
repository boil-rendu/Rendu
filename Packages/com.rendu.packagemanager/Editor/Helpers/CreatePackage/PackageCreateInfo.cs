namespace RD.PackageManager.Editor
{
    public class PackageCreateInfo
    {
        public string PackagePath;
        public string PackageName;
        public string FolderPath;
        
        public PackageInfo PackageInfo;
        
      
        
        public PackageCreateInfo(PackageInfo packageInfo)
        {
            PackageInfo = packageInfo;
        }
        
    }
}