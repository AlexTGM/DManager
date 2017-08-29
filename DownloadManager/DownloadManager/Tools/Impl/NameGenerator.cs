namespace DownloadManager.Tools.Impl
{
    public class NameGenerator : INameGenerator
    {
        public string GenerateName(string fileName, int taskId) => $"{fileName}_{taskId}";
    }
}