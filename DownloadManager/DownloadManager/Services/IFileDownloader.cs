using System;
using DownloadManager.Models;

namespace DownloadManager.Services
{
    public interface IFileDownloader
    {
        long DownloadFile(TaskInformation taskInfo);

        event EventHandler<long> CurrentBytesDownloadedChanged;

        double CurrentDownloadingSpeed { get; set; }
    }
}