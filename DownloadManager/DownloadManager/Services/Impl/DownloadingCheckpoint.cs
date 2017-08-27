using System;

namespace DownloadManager.Services.Impl
{
    public class DownloadingCheckpoint : IDownloadingCheckpoint
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        private DateTime _checkpointDateTime;

        public DownloadingCheckpoint(IDateTimeProvider dateTimeProvider = null)
        {
            _dateTimeProvider = dateTimeProvider;
        }
        
        public double MillisecondsSinceLastCheckpoint { get; set; }

        public bool CheckpointReached()
        {
            var timeFromLastCheckpoint = GetCurrentDateTime() - _checkpointDateTime;

            if (!(timeFromLastCheckpoint.TotalMilliseconds >= 1000)) return false;

            MillisecondsSinceLastCheckpoint = timeFromLastCheckpoint.TotalMilliseconds;
            _checkpointDateTime = GetCurrentDateTime();
            return true;
        }

        public void Start()
        {
            _checkpointDateTime = GetCurrentDateTime();
        }

        private DateTime GetCurrentDateTime()
        {
            return _dateTimeProvider?.GetCurrentDateTime() ?? DateTime.Now;
        }
    }
}