using System;
using SystemInterface.Timers;
using DownloadManager.Models;

namespace DownloadManager.Services.Impl
{
    public class DownloadSpeedLimiter : IDownloadSpeedLimiter
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public bool IsPaused { get; private set; }
        public long DownloadPerSecondThreshold { get; set; } = long.MaxValue;
        public long BytesDownloadedSinceLastCheckpoint { get; private set; }

        public DateTime CheckpointDateTime { get; private set; }
        public ITimer Timer { get; }

        public DownloadSpeedLimiter(ITimerFactory timerFactory, IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;

            Timer = timerFactory.Create();
            Timer.AutoReset = false;
            Timer.Elapsed += (sender, args) =>
            {
                CheckpointDateTime = _dateTimeProvider.GetCurrentDateTime();
                IsPaused = false;
            };

            CheckpointDateTime = _dateTimeProvider.GetCurrentDateTime();
        }

        public void FileDownloaderBytesDownloaded(object sender, DownloadProgress progress)
        {
            var current = _dateTimeProvider.GetCurrentDateTime();
            var millisecondsPassed = (current - CheckpointDateTime).TotalMilliseconds;
            var millisecondsLeft = Math.Max(0L, (long)(1000 - millisecondsPassed));

            if (millisecondsPassed >= 1000)
            {
                BytesDownloadedSinceLastCheckpoint = 0;
                CheckpointDateTime = _dateTimeProvider.GetCurrentDateTime();
            }

            BytesDownloadedSinceLastCheckpoint += progress.BytesDownloaded;

            if (BytesDownloadedSinceLastCheckpoint >= DownloadPerSecondThreshold)
                SetupTimer(millisecondsLeft);
        }

        private void SetupTimer(double milliseconds)
        {
            IsPaused = true;
            BytesDownloadedSinceLastCheckpoint = 0L;

            Timer.Interval = milliseconds;
            Timer.Start();
        }
    }
}