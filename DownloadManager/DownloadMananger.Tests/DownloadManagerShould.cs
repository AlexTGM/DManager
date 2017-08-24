using System;
using SystemInterface.IO;
using SystemInterface.Net;
using DownloadManager.Services;
using Moq;
using Xunit;

namespace DownloadMananger.Tests
{
    public class DownloadManagerShould
    {
        private readonly Mock<IFileInformationProvider> _fileInfoProviderMock;
        private readonly Mock<IFileDownloader> _fileDownloaderMock;
        private readonly Mock<IFileSaver> _fileSaverMock;
        private readonly Mock<IUrlHelperTools> _urlHelperToolsMock;

        private readonly IDownloadManager _downloadManager;

        public DownloadManagerShould()
        {
            Uri uri;

            _fileInfoProviderMock = new Mock<IFileInformationProvider>();
            _fileDownloaderMock = new Mock<IFileDownloader>();
            _fileSaverMock = new Mock<IFileSaver>();
            _urlHelperToolsMock = new Mock<IUrlHelperTools>();

            var httpWebResponseMock = new Mock<IHttpWebResponse>();
            var streamMock = new Mock<IStream>();

            httpWebResponseMock.Setup(m => m.GetResponseStream()).Returns(streamMock.Object);
            _fileInfoProviderMock.Setup(m => m.ObtainInformation(It.IsAny<Uri>()));
            _fileInfoProviderMock.Setup(m => m.CheckIfUriHasValidFormat(It.IsAny<string>(), out uri)).Returns(true);
            _fileDownloaderMock.Setup(m => m.GetResponse(It.IsAny<Uri>())).Returns(httpWebResponseMock.Object);
            _fileSaverMock.Setup(m => m.SaveFile(It.IsAny<IStream>()));
            _urlHelperToolsMock.Setup(m => m.UrlDecode(It.IsAny<string>()));

            _downloadManager = new DownloadManager.Services.Impl.DownloadManager(_fileInfoProviderMock.Object,
                _fileDownloaderMock.Object, _fileSaverMock.Object, _urlHelperToolsMock.Object);
        }

        [Fact]
        public void ShouldObtainFileInformationBeforeDownloading()
        {
            _downloadManager.DownloadFile(It.IsAny<string>());

            _fileInfoProviderMock.Verify(m => m.ObtainInformation(It.IsAny<Uri>()), Times.Once);
        }
    }
}