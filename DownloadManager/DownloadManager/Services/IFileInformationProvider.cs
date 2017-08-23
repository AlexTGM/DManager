using System;
using DownloadManager.Models;

namespace DownloadManager.Services
{
    public interface IFileInformationProvider
    {
        FileInformation ObtainInformation(Uri url);
    }
}