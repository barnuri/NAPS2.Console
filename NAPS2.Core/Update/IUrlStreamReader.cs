using System.IO;

namespace NAPS2.Update
{
    public interface IUrlStreamReader
    {
        Stream OpenStream(string url);
    }
}