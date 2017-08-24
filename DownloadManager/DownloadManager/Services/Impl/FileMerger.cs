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

        //public class FileMergerShould
        //{
        //    private readonly Mock<IBinaryReaderFactory> _binaryReaderFactory;
        //    private readonly Mock<IBinaryWriterFactory> _binaryWriterFactory;
        //    private readonly Mock<IFile> _file;

        //    public FileMergerShould()
        //    {
        //        _file = new Mock<IFile>();
        //        _file.Setup(m => m.Create(It.IsAny<string>()));
        //        _file.Setup(m => m.OpenRead(It.IsAny<string>()));

        //        _binaryWriterFactory = new Mock<IBinaryWriterFactory>();
        //        _binaryReaderFactory = new Mock<IBinaryReaderFactory>();
        //    }

        //    [Fact]
        //    public void MergePartialContentInOneFile()
        //    {
        //        var currentFile = 0;
        //        var files = new[] {"1", "2", "3", "4"};
        //        var contents = new[] {"a", "b", "c", "d"};

        //        var expectedContent = Encoding.UTF8.GetBytes("abcd");
        //        var actualContent = Enumerable.Empty<byte>();

        //        IFileMerger fileMerger = new FileMerger(_file.Object, _binaryReaderFactory.Object, _binaryWriterFactory.Object);

        //        var binaryReader = new Mock<IBinaryReader>();
        //        var stream = new Mock<Stream>();
        //        stream.SetupGet(m => m.Length).Returns(1);
        //        stream.SetupSet(m => m.Position = 1);
        //        binaryReader.SetupGet(m => m.BaseStream).Returns(stream.Object);
        //        binaryReader.Setup(m => m.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));

        //        var binaryWriter = new Mock<IBinaryWriter>();
        //        binaryWriter.Setup(m => m.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
        //            .Callback(() =>
        //            {
        //                stream.Object.Position = 1;
        //                actualContent = actualContent.Concat(Encoding.UTF8.GetBytes(contents[currentFile++]));
        //            });

        //        _binaryWriterFactory.Setup(m => m.Create(It.IsAny<IStream>())).Returns(binaryWriter.Object);
        //        _binaryReaderFactory.Setup(m => m.Create(It.IsAny<IStream>())).Returns(() =>
        //        {
        //            binaryReader.Reset();
        //            return binaryReader.Object;
        //        });

        //        fileMerger.Merge(files, It.IsAny<string>());
        //    }
        //}
    }
}