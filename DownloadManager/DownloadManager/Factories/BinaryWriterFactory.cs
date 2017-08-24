using SystemInterface.IO;
using SystemWrapper.IO;

namespace DownloadManager.Factories
{
    public class BinaryWriterFactory : IBinaryWriterFactory
    {
        public IBinaryWriter Create(IStream stream)
            => new BinaryWriterWrap(stream);
    }
}