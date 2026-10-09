using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    /// <summary>LinkedList 的对象池；取出即用、归还时自动 Clear</summary>
    public static class LinkedListPool<T>
    {
        private static readonly ObjectPool<LinkedList<T>> s_ListPool =
                new ObjectPool<LinkedList<T>>(null, l => l.Clear());

        /// <summary>取一个空的链表实例</summary>
        public static LinkedList<T> Get()
        {
            return s_ListPool.Get();
        }

        /// <summary>归还链表（归还前会被清空）</summary>
        public static void Release(LinkedList<T> toRelease)
        {
            s_ListPool.Release(toRelease);
        }
    }
}
