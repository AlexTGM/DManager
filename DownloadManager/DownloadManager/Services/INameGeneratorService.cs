namespace DownloadManager.Services
{
    public interface INameGeneratorService
    {
        string GenerateName(string fileName, int taskId);
    }
}