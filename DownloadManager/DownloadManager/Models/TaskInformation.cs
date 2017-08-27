namespace DownloadManager.Models
{
    public class TaskInformation
    {
        public TaskInformation(string fileName, long bytesStart, long bytesEnd)
        {
            FileName = fileName;
            BytesStart = bytesStart;
            BytesEnd = bytesEnd;
        }

        public long BytesStart { get; }
        public long BytesEnd { get; }
        public string FileName { get; }
    }
}