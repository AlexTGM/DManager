using System;
using SystemInterface.Timers;
using DownloadManager.Models;

namespace DownloadManager.Services.Impl
{
    public class DownloadSpeedMeter : IDownloadSpeedMeter
    {
        public long BytesDownloadedSinceLastCheckpoint { get; private set; }
        public double BytesPerSecond { get; private set; }

        public DateTime CheckpointDateTime { get; set; }

        public DownloadSpeedMeter(IDateTimeProvider dateTimeProvider, ITimerFactory timerFactory)
        {
            CheckpointDateTime = dateTimeProvider.GetCurrentDateTime();
            
            var timer = timerFactory.Create(1000);
            timer.Elapsed += (sender, args) =>
            {
                BytesPerSecond = BytesDownloadedSinceLastCheckpoint;
                BytesDownloadedSinceLastCheckpoint = default(long);
                CheckpointDateTime = dateTimeProvider.GetCurrentDateTime();

                DownloadingSpeedChanged?.Invoke(this, new DownloadSpeed {BytesPerSecond = BytesPerSecond});
            };
            timer.Start();
        }

        public void FileDownloaderBytesDownloaded(object sender, DownloadProgress progress)
        {
            BytesDownloadedSinceLastCheckpoint += progress.BytesDownloaded;
        }

        public event EventHandler<DownloadSpeed> DownloadingSpeedChanged;
    }
}