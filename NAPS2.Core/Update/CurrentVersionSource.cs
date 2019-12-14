using System;
using System.Reflection;

namespace NAPS2.Update
{
    public class CurrentVersionSource : ICurrentVersionSource
    {
        public Version GetCurrentVersion()
        {
            return Assembly.GetAssembly(typeof(CurrentVersionSource)).GetName().Version;
        }
    }
}