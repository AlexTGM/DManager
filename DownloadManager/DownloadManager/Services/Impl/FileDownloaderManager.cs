using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DownloadManager.Factories;
using DownloadManager.Models;
using Microsoft.Extensions.Options;

namespace DownloadManager.Services.Impl
{
    public class FileDownloaderManager : IFileDownloaderManager
    {
        private readonly IHttpWebRequestFactory _httpWebRequestFactory;
        private readonly IFileDownloader _fileDownloader;
        private readonly ApplicationOptions _options;

        public FileDownloaderManager(IHttpWebRequestFactory httpWebRequestFactory,
            IFileDownloader fileDownloader, IOptions<ApplicationOptions> options)
        {
            _httpWebRequestFactory = httpWebRequestFactory;
            _fileDownloader = fileDownloader;
            _options = options.Value;
        }

        public List<Task> DownloadingFunctions { get; } = new List<Task>();

        public long TotalBytesDownloaded { get; private set; }

        public async Task DownloadFile(Uri url, IEnumerable<TaskInformation> informations)
        {
            _fileDownloader.SetMaximumSpeed(_options.BytesPerSecond);

            foreach (var taskInfo in informations)
            {
                var request = _httpWebRequestFactory.CreateGetRangeRequest(url, taskInfo.BytesStart, taskInfo.BytesEnd);

                DownloadingFunctions.Add(Task.Run(() => _fileDownloader.SaveFile(request.GetResponse(), taskInfo)));
            }

            await Task.WhenAll(DownloadingFunctions);

            _fileDownloader.Unsubscribe();
        }
    }
}