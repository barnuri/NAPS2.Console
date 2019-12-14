namespace NAPS2.Update
{
    public interface IUrlFileDownloader
    {
        void DownloadFile(string url, string targetPath);
    }
}