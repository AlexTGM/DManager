using DownloadManager.Models;

namespace DownloadManager.Services
{
    public interface IFileDownloader
    {
        long DownloadFile(TaskInformation taskInfo);
    }
}