using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemInterface.IO;
using SystemInterface.Net;
using DownloadManager.Factories;
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
        private readonly Mock<IDownloadingTasksFactory> _downloadingTasksFactoryMock;

        private readonly IDownloadManager _downloadManager;

        public DownloadManagerShould()
        {
            Uri uri;

            _fileInfoProviderMock = new Mock<IFileInformationProvider>();
            _fileDownloaderMock = new Mock<IFileDownloader>();
            _fileMergerMock = new Mock<IFileMerger>();
            _downloadingTasksFactoryMock = new Mock<IDownloadingTasksFactory>();

            _fileInfoProviderMock.Setup(m => m.ObtainInformation(It.IsAny<Uri>()));
            _fileInfoProviderMock.Setup(m => m.CheckIfUriHasValidFormat(It.IsAny<string>(), out uri)).Returns(true);
            _fileDownloaderMock.Setup(m => m.DownloadFile(It.IsAny<TaskInformation>()));
            _fileMergerMock.Setup(m => m.Merge(It.IsAny<string[]>(), It.IsAny<string>()));
            _downloadingTasksFactoryMock.Setup(m => m.Create(It.IsAny<FileInformation>(), It.IsAny<int>()));

            _downloadManager = new DownloadManager.Services.Impl.DownloadManager(_fileInfoProviderMock.Object,
                _fileMergerMock.Object, _fileDownloaderMock.Object, _downloadingTasksFactoryMock.Object);
        }

        [Fact]
        public async Task ShouldMergeFilesAfterDownloading()
        {
            const string output = "output";
            var partialFiles = new[] { $"{output}_0", $"{output}_1" };

            _fileInfoProviderMock.Setup(m => m.ObtainInformation(It.IsAny<Uri>()))
                .Returns(new FileInformation {Name = output});

            _downloadingTasksFactoryMock.Setup(m => m.Create(It.IsAny<FileInformation>(), It.IsAny<int>()))
                .Returns(new List<TaskInformation>
                {
                    new TaskInformation(partialFiles[0], 0, 0, null),
                    new TaskInformation(partialFiles[1], 0, 0, null)
                });

            await _downloadManager.DownloadFile("output", 2);

            _fileMergerMock.Verify(m => m.Merge(partialFiles, output), Times.Once);
        }
    }
}