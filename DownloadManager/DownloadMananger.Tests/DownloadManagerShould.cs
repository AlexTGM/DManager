using System;
using System.Linq;
using SystemInterface.IO;
using SystemInterface.Net;
using DownloadManager.Models;
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
        private readonly Mock<IUrlHelperTools> _urlHelperToolsMock;

        private readonly IDownloadManager _downloadManager;

        public DownloadManagerShould()
        {
            Uri uri;

            _fileInfoProviderMock = new Mock<IFileInformationProvider>();
            _fileDownloaderMock = new Mock<IFileDownloader>();
            _fileMergerMock = new Mock<IFileMerger>();
            _urlHelperToolsMock = new Mock<IUrlHelperTools>();

            var httpWebResponseMock = new Mock<IHttpWebResponse>();
            var streamMock = new Mock<IStream>();

            httpWebResponseMock.Setup(m => m.GetResponseStream()).Returns(streamMock.Object);
            _fileInfoProviderMock.Setup(m => m.ObtainInformation(It.IsAny<Uri>()));
            _fileInfoProviderMock.Setup(m => m.CheckIfUriHasValidFormat(It.IsAny<string>(), out uri)).Returns(true);
            _fileDownloaderMock.Setup(m => m.DownloadFile(It.IsAny<TaskInformation>()));
            _fileMergerMock.Setup(m => m.Merge(It.IsAny<string[]>(), It.IsAny<string>()));
            _urlHelperToolsMock.Setup(m => m.UrlDecode(It.IsAny<string>()));

            _downloadManager = new DownloadManager.Services.Impl.DownloadManager(_fileInfoProviderMock.Object,
                _fileMergerMock.Object, _fileDownloaderMock.Object);
        }

        [Fact]
        public void ShouldCreateTwoDownloadingThreads()
        {
            _downloadManager.DownloadFile(It.IsAny<string>(), 2);
            _downloadManager.Tasks.Count.ShouldBeEquivalentTo(2);
        }

        [Fact]
        public void ShouldCreateTasksToDownloadAllContent()
        {
            const int expectedContentLength = 10000;

            _fileInfoProviderMock.Setup(m => m.ObtainInformation(It.IsAny<Uri>()))
                .Returns(new FileInformation {ContentLength = expectedContentLength });

            _downloadManager.DownloadFile(It.IsAny<string>(), 10);

            _downloadManager.Tasks.Sum(task => task.BytesEnd - task.BytesStart)
                .ShouldBeEquivalentTo(expectedContentLength - 9);
        }

        [Fact]
        public void ShouldGeneratePartialNamesAccordingToPattern()
        {
            const string output = "output";
            var partialFiles = new[] { $"{output}_0", $"{output}_1" };

            _fileInfoProviderMock.Setup(m => m.ObtainInformation(It.IsAny<Uri>()))
                .Returns(new FileInformation { Name = output });

            _downloadManager.DownloadFile(It.IsAny<string>(), 2);

            _downloadManager.Tasks.Select(task => task.FileName).ShouldBeEquivalentTo(partialFiles);
        }

        [Fact]
        public void ShouldMergeFilesAfterDownloading()
        {
            const string output = "output";
            var partialFiles = new[] { $"{output}_0", $"{output}_1" };

            _fileInfoProviderMock.Setup(m => m.ObtainInformation(It.IsAny<Uri>()))
                .Returns(new FileInformation {Name = output});

            _downloadManager.DownloadFile("output", 2);

            _fileMergerMock.Verify(m => m.Merge(partialFiles, output), Times.Once);
        }
    }
}