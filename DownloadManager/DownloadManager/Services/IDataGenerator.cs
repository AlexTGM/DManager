namespace DownloadManager.Services
{
    public interface IDataGenerator
    {
        string GenerateRandomData(int fileSize);
    }
}