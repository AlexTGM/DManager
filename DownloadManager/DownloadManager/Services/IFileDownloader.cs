using System;
using System.Threading.Tasks;
using SystemInterface.Net;
using DownloadManager.Models;

namespace DownloadManager.Services
{
    public interface IFileDownloader
    {
        Task<long> SaveFile(IHttpWebResponse response, TaskInformation taskInformation);
        void SetMaximumSpeed(long maximumSpeed);
        void Unsubscribe();

        bool IsPaused { get; set; }

        event EventHandler<DownloadProgress> BytesDownloadedChanged;
        event EventHandler<TotalProgress> TotalProgressChanged;
    }
}