using System.Collections.Generic;
using DownloadManager.Models;

namespace DownloadManager.Factories
{
    public interface IDownloadingTasksFactory
    {
        IEnumerable<TaskInformation> Create(FileInformation fileInformation, int tasksCount);
    }
}