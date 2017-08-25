using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DownloadManager.Factories;
using DownloadManager.Models;

namespace DownloadManager.Services.Impl
{
    public class DownloadManager : IDownloadManager
    {
        private readonly IFileInformationProvider _fileInfoProvider;
        private readonly IFileMerger _fileMerger;
        private readonly IFileDownloader _fileDownloader;
        private readonly IDownloadingTasksFactory _downloadingTasksFactory;

        public List<TaskInformation> Tasks { get; private set; }

        public DownloadManager(IFileInformationProvider fileInfoProvider, 
            IFileMerger fileMerger, IFileDownloader fileDownloader, 
            IDownloadingTasksFactory downloadingTasksFactory)
        {
            _fileInfoProvider = fileInfoProvider;
            _fileMerger = fileMerger;
            _fileDownloader = fileDownloader;
            _downloadingTasksFactory = downloadingTasksFactory;
        }

        public async Task DownloadFile(string url, int tasksCount)
        {
            var validUrl = _fileInfoProvider.CheckIfUriHasValidFormat(url, out Uri uri);
            if (!validUrl) throw new FormatException("Url has wrong format!");

            var fileInfo = _fileInfoProvider.ObtainInformation(uri);

            Tasks = _downloadingTasksFactory.Create(fileInfo, tasksCount);

            
                Tasks[0].FileDownloader.CurrentBytesDownloadedChanged += (sender, l) =>
                {
                    var speed = Tasks.Where(task=>task.FileDownloader.InProgress)
                    .Select(task => task.FileDownloader.CurrentDownloadingSpeed).Sum();
                    Debug.WriteLine(speed);
                };

            var tasks = Tasks.Select(taskInformation => taskInformation.StartTask()).ToList();

            await Task.WhenAll(tasks);
            _fileMerger.Merge(Tasks.Select(task => task.FileName), fileInfo.Name);
        }
    }
}