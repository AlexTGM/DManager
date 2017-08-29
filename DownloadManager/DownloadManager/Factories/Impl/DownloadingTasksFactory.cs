using System.Collections.Generic;
using System.Linq;
using DownloadManager.Models;
using DownloadManager.Services;
using DownloadManager.Tools;

namespace DownloadManager.Factories.Impl
{
    public class DownloadingTasksFactory : IDownloadingTasksFactory
    {
        private readonly INameGenerator _nameGeneratorService;

        public DownloadingTasksFactory(INameGenerator nameGeneratorService)
        {
            _nameGeneratorService = nameGeneratorService;
        }

        public IEnumerable<TaskInformation> Create(FileInformation fileInformation, int tasksCount)
        {
            var bytesPerTask = fileInformation.ContentLength / tasksCount;

            return Enumerable.Range(0, tasksCount).Select(taskId =>
            {
                var fileName = _nameGeneratorService.GenerateName(fileInformation.Name, taskId);

                var bytesStart = bytesPerTask * taskId;
                var bytesEnd = taskId == tasksCount - 1 ? fileInformation.ContentLength : bytesPerTask * (taskId + 1) - 1;

                return new TaskInformation(fileName, bytesStart, bytesEnd);
            });
        }
    }
}