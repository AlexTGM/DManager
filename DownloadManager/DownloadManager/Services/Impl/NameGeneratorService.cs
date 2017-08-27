namespace DownloadManager.Services.Impl
{
    public class NameGeneratorService : INameGeneratorService
    {
        public string GenerateName(string fileName, int taskId) => $"{fileName}_{taskId}";
    }
}