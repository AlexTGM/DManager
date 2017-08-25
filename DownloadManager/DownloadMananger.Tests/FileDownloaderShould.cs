using System;
using SystemInterface.IO;
using SystemInterface.Net;
using DownloadManager.Models;
using DownloadManager.Services;
using DownloadManager.Services.Impl;
using FluentAssertions;
using Moq;
using Xunit;

using IHttpWebRequestFactory = DownloadManager.Factories.IHttpWebRequestFactory;

namespace DownloadMananger.Tests
{
    public class FileDownloaderShould
    {
        private readonly Mock<IFile> _fileMock;
        private readonly Mock<IStream> _streamMock;
        private readonly Mock<IFileStream> _fileStreamMock;
        private readonly Mock<IHttpWebRequest> _httpWebRequest;
        private readonly Mock<IHttpWebResponse> _httpWebResponse;
        private readonly Mock<IHttpWebRequestFactory> _httpWebRequestFactoryMock;

        private readonly IFileDownloader _fileDownloader;

        private readonly int[] _streamOutput = { 10, 20, 30, 20, 10, 10, 0 };

        public FileDownloaderShould()
        {
            var currentIteration = 0;

            _fileMock = new Mock<IFile>();
            _streamMock = new Mock<IStream>();
            _fileStreamMock = new Mock<IFileStream>();
            _httpWebRequest = new Mock<IHttpWebRequest>();
            _httpWebResponse = new Mock<IHttpWebResponse>();
            _httpWebRequestFactoryMock = new Mock<IHttpWebRequestFactory>();

            _fileStreamMock.Setup(m => m.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));

            _fileMock.Setup(m => m.OpenWrite(It.IsAny<string>()))
                .Returns(_fileStreamMock.Object);

            _streamMock.SetupGet(m => m.Length).Returns(100);
            _streamMock.Setup(m => m.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() => _streamOutput[currentIteration++]);

            _httpWebResponse.Setup(m => m.GetResponseStream())
                .Returns(_streamMock.Object);

            _httpWebRequest.Setup(m => m.GetResponse())
                .Returns(_httpWebResponse.Object);

            _httpWebRequestFactoryMock.Setup(m => m.CreateGetRangeRequest(It.IsAny<Uri>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(_httpWebRequest.Object);

            _fileDownloader = new FileDownloader(_fileMock.Object, _httpWebRequestFactoryMock.Object);
        }

        [Fact]
        public void DownloadFileToLocalStorage()
        {
            var actual = _fileDownloader.DownloadFile(new TaskInformation(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Uri>()));
            actual.ShouldBeEquivalentTo(_streamMock.Object.Length);
        }

        [Fact]
        public void UpdateProgressInformation()
        {
            var eventFired = 0;

            _fileDownloader.CurrentBytesDownloadedChanged += (sender, currentDownloaded) => eventFired++;
            _fileDownloader.DownloadFile(new TaskInformation(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Uri>()));

            eventFired.ShouldBeEquivalentTo(_streamOutput.Length - 1);
        }

        [Fact]
        public void MeasureCurrentDownloadingSpeed()
        {
            _fileDownloader.DownloadFile(new TaskInformation(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Uri>()));
            _fileDownloader.CurrentDownloadingSpeed.Should().BeInRange(99*8e-6, 100*8e-6);
        }
    }
}