using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DownloadManager.Models;

namespace DownloadManager.Services.Impl
{
    public class DownloadManager : IDownloadManager
    {
        private readonly IFileInformationProvider _fileInfoProvider;
        private readonly IFileMerger _fileMerger;
        private readonly IFileDownloader _fileDownloader;

        public List<TaskInformation> Tasks { get; private set; }

        public DownloadManager(IFileInformationProvider fileInfoProvider, 
            IFileMerger fileMerger, IFileDownloader fileDownloader)
        {
            _fileInfoProvider = fileInfoProvider;
            _fileMerger = fileMerger;
            _fileDownloader = fileDownloader;
        }

        public async Task DownloadFile(string url, int tasksCount)
        {
            var validUrl = _fileInfoProvider.CheckIfUriHasValidFormat(url, out Uri uri);
            if (!validUrl) throw new FormatException("Url has wrong format!");

            var fileInfo = _fileInfoProvider.ObtainInformation(uri);

            Tasks = GenerateTasksInformations(fileInfo, tasksCount);
            Tasks.ForEach(task => task.DownloadTask = Task.Run(() => _fileDownloader.DownloadFile(task)));

            await Task.WhenAll(Tasks.Select(task => task.DownloadTask));
            _fileMerger.Merge(Tasks.Select(task => task.FileName), fileInfo.Name);
        }
        
        private static List<TaskInformation> GenerateTasksInformations(FileInformation fileInformation, int tasksCount)
            => Enumerable.Range(0, tasksCount).Select(taskId =>
            {
                var bytesPerTask = (long)Math.Ceiling((double)fileInformation.ContentLength / tasksCount);

                var fileName = $"{fileInformation.Name}_{taskId}";
                var bytesStart = bytesPerTask * taskId + (taskId == 0 ? 0 : 1);
                var bytesEnd = bytesPerTask * (taskId + 1);

                return new TaskInformation(fileName, bytesStart, bytesEnd, fileInformation.Uri);
            }).ToList();
    }
}