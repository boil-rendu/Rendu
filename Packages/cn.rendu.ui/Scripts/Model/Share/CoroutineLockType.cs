using RD.Core;
namespace RD
{
    public static partial class CoroutineLockType
    {
        //这个是UI框架用的，用来区分协程锁的类型
        //你应该定义自己的协程锁类型 以免Key相同时发生冲突
        public const int UIFramework = PackageType.UI * 1000 + 1;
        public const int UIInvokeCoroutineLock = PackageType.UI * 1000 + 2;
        public const int UILoad = PackageType.UI * 1000 + 3;
        public const int UIPanel = PackageType.UI * 1000 + 4;
        public const int UIPackage = PackageType.UI * 1000 + 5;
    }
}
