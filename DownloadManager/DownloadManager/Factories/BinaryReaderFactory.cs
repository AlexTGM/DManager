using SystemInterface.IO;
using SystemWrapper.IO;

namespace DownloadManager.Factories
{
    public class BinaryReaderFactory : IBinaryReaderFactory
    {
        public IBinaryReader Create(IStream stream)
            => new BinaryReaderWrap(stream);
    }
}