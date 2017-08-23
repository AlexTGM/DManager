using System;
using SystemInterface.IO;
using SystemInterface.Net;
using DownloadManager.Models;
using DownloadManager.Services;
using DownloadManager.Services.Impl;
using FluentAssertions;
using Moq;
using Xunit;

namespace DownloadMananger.Tests
{
    public class FileDownloaderShould
    {
        private const string Url = "http://test.domain/test.file";

        private readonly Mock<IHttpWebRequestFactory> _httpWebRequestFactoryMock;
        private readonly Mock<IFileInformationProvider> _fileInformationProviderMock;
        private readonly Mock<IFileSaver> _fileSaverMock;

        private readonly IFileDownloader _fileDownloader;

        public FileDownloaderShould()
        {
            _fileInformationProviderMock = new Mock<IFileInformationProvider>();
            _httpWebRequestFactoryMock = new Mock<IHttpWebRequestFactory>();
            _fileSaverMock = new Mock<IFileSaver>();

            _fileDownloader = new FileDownloader(_fileInformationProviderMock.Object,
                _httpWebRequestFactoryMock.Object, _fileSaverMock.Object);
        }

        [Theory]
        [InlineData("http://")]
        [InlineData("test/test.test")]
        public void ThrowExceptionIfUrlHasWrongFormat(string url)
        {
            Action action = () => _fileDownloader.DownloadFile(url);
            action.ShouldThrowExactly<FormatException>();
        }

        [Fact]
        public void DownloadFile()
        {
            const string expected = "test.file";

            var responseMock = new Mock<IHttpWebResponse>();
            responseMock.Setup(m => m.GetResponseStream()).Returns(new Mock<IStream>().Object);

            var requestMock = new Mock<IHttpWebRequest>();
            requestMock.Setup(m => m.GetResponse()).Returns(responseMock.Object);

            _fileSaverMock.Setup(m => m.SaveFile(It.IsAny<IStream>()));

            _httpWebRequestFactoryMock.Setup(m => m.Create(It.IsAny<Uri>()))
                .Returns(requestMock.Object);

            _fileInformationProviderMock.Setup(m => m.ObtainInformation(It.IsAny<Uri>()))
                .Returns(new FileInformation {Name = expected});

            var filePath = _fileDownloader.DownloadFile("http://textfiles.com/holiday/test.file");

            filePath.ShouldBeEquivalentTo(expected);
        }
    }
}