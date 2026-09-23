using System;

namespace RD.Core
{
    public interface IScheduler: IDisposable
    {
        void Add(int fiberId);
    }
}