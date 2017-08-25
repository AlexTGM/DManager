using System;
using System.Threading.Tasks;

namespace DownloadManager.Models
{
    public class TaskInformation
    {
        public TaskInformation(string fileName, long bytesStart, long bytesEnd, Uri uri)
        {
            FileName = fileName;
            BytesStart = bytesStart;
            BytesEnd = bytesEnd;
            Uri = uri;
        }

        public long BytesStart { get; }
        public long BytesEnd { get; }
        public string FileName { get; }
        public Uri Uri { get; }

        public Task DownloadTask { get; set; }
    }
}