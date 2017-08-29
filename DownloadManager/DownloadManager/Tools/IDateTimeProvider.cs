using System;

namespace DownloadManager.Tools
{
    public interface IDateTimeProvider
    {
        DateTime GetCurrentDateTime();
    }
}