using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.IO;
using DownloadManager.Models;
using DownloadManager.Services;
using DownloadManager.Services.Impl;

namespace DownloadManager.Factories.Impl
{
    public class DownloadingTasksFactory : IDownloadingTasksFactory
    {
        private readonly IFile _file;
        private readonly IHttpWebRequestFactory _httpWebRequestFactory;

        public DownloadingTasksFactory(IFile file, IHttpWebRequestFactory httpWebRequestFactory)
        {
            _file = file;
            _httpWebRequestFactory = httpWebRequestFactory;
        }

        public List<TaskInformation> Create(FileInformation fileInformation, int tasksCount)
        {
            var range = Enumerable.Range(0, tasksCount);

            var bytesPerTask = (long)Math.Ceiling((double)fileInformation.ContentLength / tasksCount);

            return range.Select(taskId =>
            {
                var fileDownloader = new FileDownloader(_file, _httpWebRequestFactory);

                var fileName = $"{fileInformation.Name}_{taskId}";

                var bytesStart = bytesPerTask * taskId + (taskId == 0 ? 0 : 1);
                var bytesEnd = bytesPerTask * (taskId + 1);

                var uri = fileInformation.Uri;

                return new TaskInformation(fileDownloader)
                    { BytesEnd = bytesEnd, BytesStart = bytesStart, Uri = uri, FileName = fileName};
            }).ToList();
        }
    }
}