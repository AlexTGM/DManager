using System;
using SystemInterface.Net;
using DownloadManager.Models;

namespace DownloadManager.Services
{
    public interface IFileDownloader
    {
        long SaveFile(IHttpWebResponse response, string fileName);

        event EventHandler<DownloadProgress> BytesDownloadedChanged;
        event EventHandler<DownloadSpeed> DownloadingSpeedChanged;
    }
}