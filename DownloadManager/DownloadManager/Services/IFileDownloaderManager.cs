using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DownloadManager.Models;

namespace DownloadManager.Services
{
    public interface IFileDownloaderManager
    {
        List<Func<long>> DownloadingFunctions { get; } 
        long TotalBytesDownloaded { get;  }
        Task<long> DownloadFile(Uri url, IEnumerable<TaskInformation> informations);
    }
}