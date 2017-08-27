using System;

namespace DownloadManager.Services.Impl
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime GetCurrentDateTime() => DateTime.Now;
    }
}