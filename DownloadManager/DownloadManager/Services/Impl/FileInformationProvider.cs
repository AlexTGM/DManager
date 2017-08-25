using System;
using System.IO;
using System.Net;
using DownloadManager.Factories;
using DownloadManager.Models;

namespace DownloadManager.Services.Impl
{
    public class FileInformationProvider : IFileInformationProvider
    {
        private readonly IHttpWebRequestFactory _requestFactory;

        public FileInformationProvider(IHttpWebRequestFactory requestFactory)
        {
            _requestFactory = requestFactory;
        }

        public bool CheckIfUriHasValidFormat(string url, out Uri uri)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out uri);
        }

        public FileInformation ObtainInformation(Uri url)
        {
            var httpWebRequest = _requestFactory.CreateHeadRequest(url);

            var test = httpWebRequest.GetResponse();

            return new FileInformation
            {
                AcceptRanges = test.Headers[HttpResponseHeader.AcceptRanges],
                ContentLength = test.ContentLength,
                Name = Path.GetFileName(url.LocalPath),
                Uri = url
            };
        }
    }
}