using System.Collections.Generic;
using DownloadManager.Models;

namespace DownloadManager.Factories
{
    public interface IDownloadingTasksFactory
    {
        List<TaskInformation> Create(FileInformation fileInformation, int tasksCount);
    }
}