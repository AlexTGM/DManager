using SystemInterface.IO;
using DownloadManager.Factories;
using DownloadManager.Models;

namespace DownloadManager.Services.Impl
{
    public class FileDownloader : IFileDownloader
    {
        private readonly IFile _file;
        private readonly IHttpWebRequestFactory _httpWebRequestFactory;

        public FileDownloader(IFile file, IHttpWebRequestFactory httpWebRequestFactory)
        {
            _file = file;
            _httpWebRequestFactory = httpWebRequestFactory;
        }

        public long DownloadFile(TaskInformation taskInfo)
        {
            var request = _httpWebRequestFactory.CreateGetRangeRequest(taskInfo.Uri, taskInfo.BytesStart, taskInfo.BytesEnd);
            using (var stream = request.GetResponse().GetResponseStream()) return SaveFile(stream, taskInfo.FileName);
        }

        private long SaveFile(IStream responseStream, string filePath)
        {
            var buffer = new byte[524288];
            var totalBytesWritten = 0L;

            using (var fileStream = _file.OpenWrite(filePath))
            {
                int bytesRead;

                while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);

                    totalBytesWritten += bytesRead;
                }
            }

            return totalBytesWritten;
        }
    }
}