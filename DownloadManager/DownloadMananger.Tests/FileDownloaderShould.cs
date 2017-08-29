using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SystemInterface.IO;
using SystemInterface.Net;
using DownloadManager.Services;
using DownloadManager.Services.Impl;
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
        private readonly Mock<IDownloadSpeedLimiter> _downloadSpeedLimiterMock = new Mock<IDownloadSpeedLimiter>();

        public FileDownloaderShould()
        {
            _streamMock.SetupGet(m => m.Length);
            _streamMock.Setup(m => m.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));
            _fileStreamMock.Setup(m => m.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));
            _fileMock.Setup(m => m.OpenWrite(It.IsAny<string>())).Returns(_fileStreamMock.Object);
            _httpWebResponseMock.Setup(m => m.GetResponseStream()).Returns(_streamMock.Object);
            _dateTimeProviderMock.Setup(m => m.GetCurrentDateTime());
        }

        [Fact]
        public async Task DownloadFile()
        {
            var currentIteration = 0;
            var streamReadOutput = new[] { 10, 20, 30, 20, 10, 10, 0 };

            _streamMock.SetupGet(m => m.Length).Returns(streamReadOutput.Sum());
            _streamMock.Setup(m => m.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() => streamReadOutput[currentIteration++]);

            IFileDownloader fileDownloader = new FileDownloader(_fileMock.Object, _dateTimeProviderMock.Object, _downloadSpeedLimiterMock.Object);

            var fileSize = await fileDownloader.SaveFile(_httpWebResponseMock.Object, It.IsAny<string>());

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

            IFileDownloader fileDownloader = new FileDownloader(_fileMock.Object, _dateTimeProviderMock.Object, _downloadSpeedLimiterMock.Object);
            fileDownloader.BytesDownloadedChanged += (sender, progress) => totalBytesDownloaded += progress.BytesDownloaded;

            fileDownloader.SaveFile(_httpWebResponseMock.Object, It.IsAny<string>());

            totalBytesDownloaded.ShouldBeEquivalentTo(streamReadOutput.Sum());
        }

        [Fact]
        public void InvokeDownloadingSpeedChangedEvent()
        {
            int currentIteration1 = 0, currentIteration2 = 0;
            var streamReadOutput = new[] { 10, 20, 0 };
            var checkpointDateTimes = new[]
            {
                new DateTime(1, 1, 1, 0, 0, 0),
                new DateTime(1, 1, 1, 0, 0, 0),
                new DateTime(1, 1, 1, 0, 0, 1),
                new DateTime(1, 1, 1, 0, 0, 1),
            };

            _streamMock.SetupGet(m => m.Length).Returns(streamReadOutput.Sum());
            _streamMock.Setup(m => m.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() => streamReadOutput[currentIteration1++]);

            _dateTimeProviderMock.Setup(m => m.GetCurrentDateTime())
                .Returns(() => checkpointDateTimes[currentIteration2++]);

            var measuredSpeed = 0D;

            IFileDownloader fileDownloader = new FileDownloader(_fileMock.Object, _dateTimeProviderMock.Object, _downloadSpeedLimiterMock.Object);
            fileDownloader.DownloadingSpeedChanged += (sender, speed) => measuredSpeed = speed.BytesPerSecond;

            fileDownloader.SaveFile(_httpWebResponseMock.Object, It.IsAny<string>());

            measuredSpeed.Should().BeInRange(29, 31);
        }
    }
}