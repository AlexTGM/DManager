using SystemInterface.IO;

namespace DownloadManager.Factories
{
    public interface IBinaryWriterFactory
    {
        IBinaryWriter Create(IStream stream);
    }
}