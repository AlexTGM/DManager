using System.IO;

namespace DownloadManager.SystemWrappers
{
    public class FileStreamWrap : IFileStream
    {
        public FileStream FileStreamInstance { get; private set; }

        public FileStreamWrap(Stream stream)
        {
            Initialize(stream);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            FileStreamInstance.Write(buffer, offset, count);
        }

        public void Initialize(Stream stream)
        {
            FileStreamInstance = stream as FileStream;
        }

        public void Dispose()
        {
            FileStreamInstance?.Dispose();
        }
    }
}