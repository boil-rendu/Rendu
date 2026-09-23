using MemoryPack;

namespace RD.Core
{
    public static class MemoryPackRegister
    {
        public static void Init()
        {
            MemoryPackFormatterProvider.Register(new MemoryPackChildrenCollectionFormatter());
            MemoryPackFormatterProvider.Register(new MemoryPackComponentsCollectionFormatter());
        }
    }
}