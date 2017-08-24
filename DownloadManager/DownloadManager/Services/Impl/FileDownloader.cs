using System;
using SystemInterface.Net;

namespace DownloadManager.Services.Impl
{
    public class FileDownloader : IFileDownloader
    {
        private readonly IHttpWebRequestFactory _httpWebRequestFactory;

        public FileDownloader(IHttpWebRequestFactory httpWebRequestFactory)
        {
            _httpWebRequestFactory = httpWebRequestFactory;
        }

        public IHttpWebResponse GetResponse(Uri url)
        {
            return _httpWebRequestFactory.Create(url).GetResponse();
        }
    }
}