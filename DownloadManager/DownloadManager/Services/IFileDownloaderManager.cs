using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DownloadManager.Models;

namespace DownloadManager.Services
{
    public interface IFileDownloaderManager
    {
        List<Task> DownloadingFunctions { get; } 
        long TotalBytesDownloaded { get;  }
        Task DownloadFile(Uri url, IEnumerable<TaskInformation> informations);
    }
}