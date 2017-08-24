using System;
using SystemInterface.Net;

namespace DownloadManager.Services
{
    public interface IFileDownloader
    {
        IHttpWebResponse GetResponse(Uri path);
    }
}