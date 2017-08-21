using System.IO;

namespace DownloadManager.SystemWrappers
{
    public class FileWrap : IFile
    {
        public IFileStream OpenWrite(string path)
        {
            return new FileStreamWrap(File.OpenWrite(path));
        }
    }
}