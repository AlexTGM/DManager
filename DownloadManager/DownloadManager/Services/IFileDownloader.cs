using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DownloadManager.Models;

namespace DownloadManager.Services
{
    public interface IFileDownloaderManager
    {
        Task<long> DownloadFile(Uri url, IEnumerable<TaskInformation> informations);

        List<Func<long>> DownloadingFunctions { get; }
    }
}