namespace DownloadManager.Services
{
    public interface IDownloadingCheckpoint
    {
        bool CheckpointReached();

        double MillisecondsSinceLastCheckpoint { get; }

        void Start();
    }
}