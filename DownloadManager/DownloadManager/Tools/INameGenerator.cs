namespace DownloadManager.Tools
{
    public interface INameGenerator
    {
        string GenerateName(string fileName, int taskId);
    }
}