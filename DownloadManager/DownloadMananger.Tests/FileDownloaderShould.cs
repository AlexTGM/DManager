using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SystemInterface.IO;
using SystemInterface.Net;
using DownloadManager.Services;
using DownloadManager.Services.Impl;
using DownloadManager.Tools;
using FluentAssertions;
using Moq;
using Xunit;

namespace DownloadMananger.Tests
{
    public class FileDownloaderShould
    {
        private readonly Mock<IFile> _fileMock = new Mock<IFile>();
        private readonly Mock<IStream> _streamMock = new Mock<IStream>();
        private readonly Mock<IFileStream> _fileStreamMock = new Mock<IFileStream>();
        private readonly Mock<IHttpWebResponse> _httpWebResponseMock = new Mock<IHttpWebResponse>();
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        private readonly Mock<IDownloadSpeedMeter> _downloadSpeedMeterMock = new Mock<IDownloadSpeedMeter>();
        private readonly Mock<IDownloadSpeedLimiter> _downloadSpeedLimiterMock = new Mock<IDownloadSpeedLimiter>();

        private readonly IFileDownloader _fileDownloader;

        public FileDownloaderShould()
        {
            _streamMock.SetupGet(m => m.Length);
            _streamMock.Setup(m => m.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));
            _fileStreamMock.Setup(m => m.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));
            _fileMock.Setup(m => m.OpenWrite(It.IsAny<string>())).Returns(_fileStreamMock.Object);
            _httpWebResponseMock.Setup(m => m.GetResponseStream()).Returns(_streamMock.Object);
            _dateTimeProviderMock.Setup(m => m.GetCurrentDateTime());

            _fileDownloader = new FileDownloader(_fileMock.Object, _downloadSpeedMeterMock.Object, _downloadSpeedLimiterMock.Object);
        }

        [Fact]
        public async Task DownloadFile()
        {
            var currentIteration = 0;
            var streamReadOutput = new[] { 10, 20, 30, 20, 10, 10, 0 };

            _streamMock.SetupGet(m => m.Length).Returns(streamReadOutput.Sum());
            _streamMock.Setup(m => m.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() => streamReadOutput[currentIteration++]);
            
            var fileSize = await _fileDownloader.SaveFile(_httpWebResponseMock.Object, It.IsAny<string>());

            fileSize.ShouldBeEquivalentTo(streamReadOutput.Sum());
        }

        [Fact]
        public void InvokeBytesDownloadedEvent()
        {
            var currentIteration = 0;
            var streamReadOutput = new[] { 10, 20, 30, 20, 10, 10, 0 };
            var totalBytesDownloaded = 0L;

            _streamMock.SetupGet(m => m.Length).Returns(streamReadOutput.Sum());
            _streamMock.Setup(m => m.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() => streamReadOutput[currentIteration++]);
            
            _fileDownloader.BytesDownloadedChanged += (sender, progress) => totalBytesDownloaded += progress.BytesDownloaded;

            _fileDownloader.SaveFile(_httpWebResponseMock.Object, It.IsAny<string>());

            totalBytesDownloaded.ShouldBeEquivalentTo(streamReadOutput.Sum());
        }
    }
}