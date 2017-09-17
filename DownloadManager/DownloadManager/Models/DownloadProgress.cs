namespace DownloadManager.Models
{
    public class TotalProgress
    {
        public TaskInformation TaskInformation { get; set; }
        public long TotalBytesDownloaded { get; set; }

        public TotalProgress(TaskInformation taskInformation, long totalBytesDownloaded)
        {
            TaskInformation = taskInformation;
            TotalBytesDownloaded = totalBytesDownloaded;
        }
    }

    public class DownloadProgress
    {
        public TaskInformation TaskInformation { get; set; }
        public long BytesDownloaded { get; set; }

        public DownloadProgress(TaskInformation taskInformation, long bytesDownloaded)
        {
            TaskInformation = taskInformation;
            BytesDownloaded = bytesDownloaded;
        }
    }
}