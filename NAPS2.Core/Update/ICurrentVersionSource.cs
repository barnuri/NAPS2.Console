using System;

namespace NAPS2.Update
{
    public interface ICurrentVersionSource
    {
        Version GetCurrentVersion();
    }
}