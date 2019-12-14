using System.Collections.Generic;
using System.Threading.Tasks;

namespace NAPS2.Update
{
    public interface ILatestVersionSource
    {
        Task<List<VersionInfo>> GetLatestVersionInfo();
    }
}