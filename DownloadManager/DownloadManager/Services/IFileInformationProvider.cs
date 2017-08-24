using System;
using DownloadManager.Models;

namespace DownloadManager.Services
{
    public interface IFileInformationProvider
    {
        bool CheckIfUriHasValidFormat(string url, out Uri uri);
        FileInformation ObtainInformation(Uri url);
    }
}