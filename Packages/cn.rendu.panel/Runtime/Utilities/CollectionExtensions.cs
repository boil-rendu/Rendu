using System.Collections.Generic;

namespace RD.Panel
{
    /// <summary>集合扩展方法。</summary>
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> source)
        {
            if (source == null) return;
            foreach (var item in source)
            {
                hashSet.Add(item);
            }
        }
    }
}
