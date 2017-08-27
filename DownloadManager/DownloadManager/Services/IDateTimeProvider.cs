using System;

namespace DownloadManager.Services
{
    public interface IDateTimeProvider
    {
        DateTime GetCurrentDateTime();
    }
}