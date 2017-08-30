namespace DownloadManager.Models
{
    public class TotalProgress
    {
        public TaskInformation TaskInformation { get; set; }
        public long TotalBytesDownloaded { get; set; }
    }

    public class DownloadProgress
    {
        public TaskInformation TaskInformation { get; set; }
        public long BytesDownloaded { get; set; }
    }
}