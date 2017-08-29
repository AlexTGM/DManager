using System;
using DownloadManager.Models;

namespace DownloadManager.Services
{
    public interface IDownloadSpeedMeter
    {
        DateTime CheckpointDateTime { get; }
        double BytesPerSecond { get; }
        long BytesDownloadedSinceLastCheckpoint { get; }

        void FileDownloaderBytesDownloaded(object sender, DownloadProgress progress);

        event EventHandler<DownloadSpeed> DownloadingSpeedChanged;
    }
}