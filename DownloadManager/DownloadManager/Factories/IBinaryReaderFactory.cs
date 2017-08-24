using SystemInterface.IO;

namespace DownloadManager.Factories
{
    public interface IBinaryReaderFactory
    {
        IBinaryReader Create(IStream stream);
    }
}