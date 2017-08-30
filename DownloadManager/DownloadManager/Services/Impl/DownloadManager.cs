using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DownloadManager.Controllers;
using DownloadManager.Factories;
using DownloadManager.Models;

namespace DownloadManager.Services.Impl
{
    public class DownloadManager : IDownloadManager
    {
        private readonly IFileInformationProvider _fileInfoProvider;
        private readonly IFileMerger _fileMerger;
        private readonly IFileDownloaderManager _fileDownloaderManager;
        private readonly IDownloadingTasksFactory _downloadingTasksFactory;

        public List<TaskInformation> Tasks { get; private set; }

        public DownloadManager(IFileInformationProvider fileInfoProvider, 
            IFileMerger fileMerger, IFileDownloaderManager fileDownloaderManager, 
            IDownloadingTasksFactory downloadingTasksFactory)
        {
            _fileInfoProvider = fileInfoProvider;
            _fileMerger = fileMerger;
            _fileDownloaderManager = fileDownloaderManager;
            _downloadingTasksFactory = downloadingTasksFactory;
        }

        public async Task DownloadFile(string url, int tasksCount)
        {
            var validUrl = _fileInfoProvider.CheckIfUriHasValidFormat(url, out Uri uri);
            if (!validUrl) throw new FormatException("Url has wrong format!");

            var fileInfo = _fileInfoProvider.ObtainInformation(uri);
            Tasks = _downloadingTasksFactory.Create(fileInfo, tasksCount).ToList();

            //return new DownloadLinkResponse{FilesNames = Tasks.Select(task => task.FileName).ToArray()};

            await _fileDownloaderManager.DownloadFile(uri, Tasks);

            _fileMerger.Merge(Tasks.Select(task => task.FileName), fileInfo.Name);
        }
    }
}