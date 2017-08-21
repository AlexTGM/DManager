namespace DownloadManager.SystemWrappers
{
    public interface IFile
    {
        IFileStream OpenWrite(string path);
    }
}