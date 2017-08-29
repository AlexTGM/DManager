using System;
using SystemInterface.Timers;
using DownloadManager.Models;

namespace DownloadManager.Services
{
    public interface IDownloadSpeedLimiter
    {
        bool IsPaused { get; }
        long DownloadPerSecondThreshold { get; set; }
        long BytesDownloadedSinceLastCheckpoint { get; }
        DateTime CheckpointDateTime { get; }

        ITimer Timer { get; }

        void FileDownloaderBytesDownloaded(object sender, DownloadProgress progress);
    }
}