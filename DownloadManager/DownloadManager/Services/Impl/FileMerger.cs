using System.Collections.Generic;
using SystemInterface.IO;
using DownloadManager.Factories;

namespace DownloadManager.Services.Impl
{
    public class FileMerger : IFileMerger
    {
        private readonly IFile _file;

        private readonly IBinaryReaderFactory _binaryReaderFactory;
        private readonly IBinaryWriterFactory _binaryWriterFactory;

        public FileMerger(IFile file, IBinaryReaderFactory binaryReaderFactory,
            IBinaryWriterFactory binaryWriterFactory)
        {
            _file = file;
            _binaryReaderFactory = binaryReaderFactory;
            _binaryWriterFactory = binaryWriterFactory;
        }

        public void Merge(IEnumerable<string> filesPath, string outputPath)
        {
            using (var writer = _binaryWriterFactory.Create(_file.Create(outputPath)))
            {
                foreach (var filePath in filesPath)
                {
                    using (var reader = _binaryReaderFactory.Create(_file.OpenRead(filePath)))
                    {
                        var buffer = new byte[1000000];

                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                        {
                            var count = reader.Read(buffer, 0, buffer.Length);
                            writer.Write(buffer, 0, count);
                        }
                    }
                }
            }
        }
    }
}