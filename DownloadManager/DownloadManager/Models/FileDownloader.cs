using System;
using System.Threading.Tasks;
using DownloadManager.Services;

namespace DownloadManager.Models
{
    public class TaskInformation
    {
        public readonly IFileDownloader FileDownloader;

        public TaskInformation(IFileDownloader fileDownloader)
        {
            FileDownloader = fileDownloader;
        }

        public void Initialize(string fileName, long bytesStart, long bytesEnd, Uri uri)
        {
            FileName = fileName;
            BytesStart = bytesStart;
            BytesEnd = bytesEnd;
            Uri = uri;
        }

        public Task StartTask()
        {
            return Task.Run(() => FileDownloader.DownloadFile(this));
        }

        public long BytesStart { get; set; }
        public long BytesEnd { get; set; }
        public string FileName { get; set; }
        public Uri Uri { get; set; }
    }
}