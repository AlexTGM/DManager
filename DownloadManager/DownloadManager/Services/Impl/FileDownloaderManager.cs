using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DownloadManager.Factories;
using DownloadManager.Models;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;

namespace DownloadManager.Services.Impl
{
    public class FileDownloaderManager : IFileDownloaderManager
    {
        private readonly IHttpWebRequestFactory _httpWebRequestFactory;
        private readonly ITasksRunner _tasksRunner;
        private readonly IFileDownloader _fileDownloader;

        public FileDownloaderManager(IHttpWebRequestFactory httpWebRequestFactory, 
            ITasksRunner tasksRunner, IFileDownloader fileDownloader)
        {
            _httpWebRequestFactory = httpWebRequestFactory;
            _tasksRunner = tasksRunner;
            _fileDownloader = fileDownloader;
        }

        public List<Task<long>> DownloadingFunctions { get; } = new List<Task<long>>();

        public long TotalBytesDownloaded { get; private set; }

        public async Task<long> DownloadFile(Uri url, IEnumerable<TaskInformation> informations)
        {
            foreach (var taskInfo in informations)
            {
                var request = _httpWebRequestFactory.CreateGetRangeRequest(url, taskInfo.BytesStart, taskInfo.BytesEnd);

                DownloadingFunctions.Add(_fileDownloader.SaveFile(request.GetResponse(), taskInfo.FileName));
            }

            return (await Task.WhenAll(DownloadingFunctions)).Sum();
        }
    }
}