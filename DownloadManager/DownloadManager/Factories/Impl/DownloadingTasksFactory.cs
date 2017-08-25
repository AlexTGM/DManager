using System;
using System.Collections.Generic;
using System.Linq;
using DownloadManager.Models;

namespace DownloadManager.Factories.Impl
{
    public class DownloadingTasksFactory : IDownloadingTasksFactory
    {
        public List<TaskInformation> Create(FileInformation fileInformation, int tasksCount)
        {
            var range = Enumerable.Range(0, tasksCount);

            var bytesPerTask = (long)Math.Ceiling((double)fileInformation.ContentLength / tasksCount);

            return range.Select(taskId =>
            {
                var fileName = $"{fileInformation.Name}_{taskId}";

                var bytesStart = bytesPerTask * taskId + (taskId == 0 ? 0 : 1);
                var bytesEnd = bytesPerTask * (taskId + 1);

                var uri = fileInformation.Uri;

                return new TaskInformation(fileName, bytesStart, bytesEnd, uri);
            }).ToList();
        }
    }
}