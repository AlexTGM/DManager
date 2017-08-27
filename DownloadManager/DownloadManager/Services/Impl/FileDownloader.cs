using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemInterface.IO;
using SystemInterface.Net;
using DownloadManager.Models;

using IHttpWebRequestFactory = DownloadManager.Factories.IHttpWebRequestFactory;

namespace DownloadManager.Services.Impl
{
    public class FileDownloader : IFileDownloader
    {
        private readonly IFile _file;
        private readonly IHttpWebRequestFactory _httpWebRequestFactory;
        private readonly ITasksRunner _tasksRunner;

        public FileDownloader(IFile file, IHttpWebRequestFactory httpWebRequestFactory, ITasksRunner tasksRunner)
        {
            _file = file;
            _httpWebRequestFactory = httpWebRequestFactory;
            _tasksRunner = tasksRunner;
        }

        public async Task<long> DownloadFile(Uri url, IEnumerable<TaskInformation> informations)
        {
            foreach (var taskInfo in informations)
            {
                var request = _httpWebRequestFactory.CreateGetRangeRequest(url, taskInfo.BytesStart, taskInfo.BytesEnd);
                DownloadingFunctions.Add(() => SaveFile(request.GetResponse(), taskInfo.FileName));
            }

            return (await Task.WhenAll(_tasksRunner.RunTasks(DownloadingFunctions))).Sum();
        }

        public List<Func<long>> DownloadingFunctions { get; } = new List<Func<long>>();

        private long SaveFile(IHttpWebResponse response, string filePath)
        {
            var buffer = new byte[524288];
            var totalBytesWritten = 0L;

            using (var fileStream = _file.OpenWrite(filePath))
            {
                using (var stream = response.GetResponseStream())
                {
                    int bytesRead;

                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);

                        totalBytesWritten += bytesRead;
                    }
                }
            }

            return totalBytesWritten;
        }
    }
}