using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DownloadManager.Factories;
using DownloadManager.Models;
using Microsoft.Extensions.Options;

namespace DownloadManager.Services.Impl
{
    public class DownloadManager : IDownloadManager
    {
        private readonly IFileInformationProvider _fileInfoProvider;
        private readonly IFileMerger _fileMerger;
        private readonly IFileDownloaderManager _fileDownloaderManager;
        private readonly IDownloadingTasksFactory _downloadingTasksFactory;
        private readonly ApplicationOptions _options;

        public List<TaskInformation> Tasks { get; private set; }

        public DownloadManager(IFileInformationProvider fileInfoProvider, 
            IFileMerger fileMerger, IFileDownloaderManager fileDownloaderManager, 
            IDownloadingTasksFactory downloadingTasksFactory, IOptions<ApplicationOptions> options)
        {
            _fileInfoProvider = fileInfoProvider;
            _fileMerger = fileMerger;
            _fileDownloaderManager = fileDownloaderManager;
            _downloadingTasksFactory = downloadingTasksFactory;
            _options = options.Value;
        }

        public IEnumerable<string> DownloadFile(string url)
        {
            var validUrl = _fileInfoProvider.CheckIfUriHasValidFormat(url, out Uri uri);
            if (!validUrl) throw new FormatException("Url has wrong format!");

            var fileInfo = _fileInfoProvider.ObtainInformation(uri);
            Tasks = _downloadingTasksFactory.Create(fileInfo, _options.ThreadsPerDownload).ToList();

            Task.Run(() => _fileDownloaderManager.DownloadFile(uri, Tasks))
                .ContinueWith(t => _fileMerger.Merge(Tasks.Select(task => task.FileName), fileInfo.Name));

            return Tasks.Select(task => task.FileName);
        }
    }
}