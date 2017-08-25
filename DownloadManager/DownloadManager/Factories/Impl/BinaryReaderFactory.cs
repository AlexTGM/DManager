using SystemInterface.IO;
using SystemWrapper.IO;

namespace DownloadManager.Factories.Impl
{
    public class BinaryReaderFactory : IBinaryReaderFactory
    {
        public IBinaryReader Create(IStream stream)
            => new BinaryReaderWrap(stream);
    }
}