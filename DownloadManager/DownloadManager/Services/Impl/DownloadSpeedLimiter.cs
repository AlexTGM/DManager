using System;
using System.Threading;
using SystemInterface.Timers;
using DownloadManager.Models;

namespace DownloadManager.Services.Impl
{
    public class DownloadSpeedLimiter : IDownloadSpeedLimiter
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly AutoResetEvent _checkpoint = new AutoResetEvent(true);

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
                _checkpoint.WaitOne();
                CheckpointDateTime = _dateTimeProvider.GetCurrentDateTime();
                IsPaused = false;
                _checkpoint.Set();
            };

            CheckpointDateTime = _dateTimeProvider.GetCurrentDateTime();
        }

        public void FileDownloaderBytesDownloaded(object sender, DownloadProgress progress)
        {
            _checkpoint.WaitOne();

            var millisecondsPassed = (_dateTimeProvider.GetCurrentDateTime() - CheckpointDateTime).TotalMilliseconds;
            var millisecondsLeft = Math.Max(0L, (long)(1000 - millisecondsPassed));

            if (millisecondsPassed >= 1000)
            {
                BytesDownloadedSinceLastCheckpoint = 0;
                CheckpointDateTime = _dateTimeProvider.GetCurrentDateTime();
            }

            BytesDownloadedSinceLastCheckpoint += progress.BytesDownloaded;

            if (BytesDownloadedSinceLastCheckpoint >= DownloadPerSecondThreshold)
                SetupTimer(millisecondsLeft);

            _checkpoint.Set();
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