using System;
using SystemInterface.IO;
using SystemInterface.Net;
using DownloadManager.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace DownloadMananger.Tests
{
    public class DownloadManagerShould
    {
        private readonly Mock<IFileInformationProvider> _fileInfoProviderMock;
        private readonly Mock<IFileDownloader> _fileDownloaderMock;
        private readonly Mock<IFileMerger> _fileMergerMock;
        private readonly Mock<IFileSaver> _fileSaverMock;
        private readonly Mock<IUrlHelperTools> _urlHelperToolsMock;

        private readonly IDownloadManager _downloadManager;

        public DownloadManagerShould()
        {
            Uri uri;

            _fileInfoProviderMock = new Mock<IFileInformationProvider>();
            _fileDownloaderMock = new Mock<IFileDownloader>();
            _fileMergerMock = new Mock<IFileMerger>();
            _fileSaverMock = new Mock<IFileSaver>();
            _urlHelperToolsMock = new Mock<IUrlHelperTools>();

            var httpWebResponseMock = new Mock<IHttpWebResponse>();
            var streamMock = new Mock<IStream>();

            httpWebResponseMock.Setup(m => m.GetResponseStream()).Returns(streamMock.Object);
            _fileInfoProviderMock.Setup(m => m.ObtainInformation(It.IsAny<Uri>()));
            _fileInfoProviderMock.Setup(m => m.CheckIfUriHasValidFormat(It.IsAny<string>(), out uri)).Returns(true);
            _fileDownloaderMock.Setup(m => m.GetResponse(It.IsAny<Uri>(), It.IsAny<int>(), It.IsAny<int>())).Returns(httpWebResponseMock.Object);
            _fileMergerMock.Setup(m => m.Merge(It.IsAny<string[]>(), It.IsAny<string>()));
            _fileSaverMock.Setup(m => m.SaveFile(It.IsAny<IStream>(), It.IsAny<string>()));
            _urlHelperToolsMock.Setup(m => m.UrlDecode(It.IsAny<string>()));

            _downloadManager = new DownloadManager.Services.Impl.DownloadManager(_fileInfoProviderMock.Object,
                _fileDownloaderMock.Object, _fileMergerMock.Object, _fileSaverMock.Object, _urlHelperToolsMock.Object);
        }

        [Fact]
        public void ShouldCreateTwoDownloadingThreads()
        {
            _downloadManager.DownloadFile(It.IsAny<string>(), 2);
            _downloadManager.Tasks.Count.ShouldBeEquivalentTo(2);
        }

        [Fact]
        public void ShouldMergeFilesAfterDownloading()
        {
            var expectedPaths = new[] {"1", "2"};
            var outputPath = "output";

            _downloadManager.DownloadFile(It.IsAny<string>(), 2);

            _fileMergerMock.Verify(m => m.Merge(expectedPaths, outputPath), Times.Once);
        }
    }
}