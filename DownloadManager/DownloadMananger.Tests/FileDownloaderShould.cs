using System;
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
        private readonly Mock<IHttpWebRequestFactory> _httpWebRequestFactoryMock;

        private readonly IFileDownloader _fileDownloader;

        public FileDownloaderShould()
        {
            _httpWebRequestFactoryMock = new Mock<IHttpWebRequestFactory>();

            _fileDownloader = new FileDownloader(_httpWebRequestFactoryMock.Object);
        }

        [Fact]
        public void DownloadFile()
        {
            var responseMock = new Mock<IHttpWebResponse>();

            var requestMock = new Mock<IHttpWebRequest>();
            requestMock.Setup(m => m.GetResponse()).Returns(responseMock.Object);

            _httpWebRequestFactoryMock.Setup(m => m.Create(It.IsAny<Uri>())).Returns(requestMock.Object);

            var response = _fileDownloader.GetResponse(It.IsAny<Uri>(), It.IsAny<int>(), It.IsAny<int>());

            response.ShouldBeEquivalentTo(responseMock.Object);
        }
    }
}