using System.Linq;
using SystemInterface.IO;
using SystemInterface.Net;
using DownloadManager.Services.Impl;
using FluentAssertions;
using Moq;
using Xunit;

namespace DownloadMananger.Tests
{
    class FileDownloaderShould
    {
        private readonly Mock<IFile> _fileMock;
        private readonly Mock<IStream> _streamMock;
        private readonly Mock<IFileStream> _fileStreamMock;
        private readonly Mock<IHttpWebResponse> _httpWebResponse;

        public FileDownloaderShould()
        {
            _fileMock = new Mock<IFile>();
            _streamMock = new Mock<IStream>();
            _fileStreamMock = new Mock<IFileStream>();
            _httpWebResponse = new Mock<IHttpWebResponse>();

            _streamMock.SetupGet(m => m.Length);
            _streamMock.Setup(m => m.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));
            _fileStreamMock.Setup(m => m.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));
            _fileMock.Setup(m => m.OpenWrite(It.IsAny<string>())).Returns(_fileStreamMock.Object);
            _httpWebResponse.Setup(m => m.GetResponseStream()).Returns(_streamMock.Object);
        }

        [Fact]
        public void DownloadFile()
        {
            var currentIteration = 0;
            var streamReadOutput = new[] { 10, 20, 30, 20, 10, 10, 0 };

            _streamMock.SetupGet(m => m.Length).Returns(streamReadOutput.Sum());
            _streamMock.Setup(m => m.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() => streamReadOutput[currentIteration++]);

            IFileDownloader fileDownloader = new FileDownloader(_fileMock.Object);

            var fileSize = fileDownloader.SaveFile(_httpWebResponse.Object, It.IsAny<string>());

            fileSize.ShouldBeEquivalentTo(streamReadOutput.Sum());
        }
    }
}