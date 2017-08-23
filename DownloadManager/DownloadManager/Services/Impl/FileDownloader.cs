using System;
using SystemInterface.Net;

namespace DownloadManager.Services.Impl
{
    public class FileDownloader : IFileDownloader
    {
        private readonly IFileInformationProvider _fileInformationProvider;
        private readonly IHttpWebRequestFactory _httpWebRequestFactory;
        private readonly IFileSaver _fileSaver;

        public FileDownloader(IFileInformationProvider fileInformationProvider,
            IHttpWebRequestFactory httpWebRequestFactory, IFileSaver fileSaver = null)
        {
            _fileInformationProvider = fileInformationProvider;
            _httpWebRequestFactory = httpWebRequestFactory;
            _fileSaver = fileSaver;
        }

        public string DownloadFile(string url)
        {
            var unescapedUrl = Uri.UnescapeDataString(url);

            if (!ValidUrlProvided(unescapedUrl, out Uri uri))
                throw new FormatException("url has wrong format");

            var fileInfo = _fileInformationProvider.ObtainInformation(uri);

            var httpWebRequest = _httpWebRequestFactory.Create(uri);

            using (var stream = httpWebRequest.GetResponse().GetResponseStream())
                _fileSaver.SaveFile(stream);

            return fileInfo.Name;
        }

        private static bool ValidUrlProvided(string url, out Uri uri)
            => Uri.TryCreate(url, UriKind.Absolute, out uri);
    }
}