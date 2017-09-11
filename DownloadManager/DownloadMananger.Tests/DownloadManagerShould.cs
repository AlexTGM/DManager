using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DownloadManager;
using DownloadManager.Factories;
using DownloadManager.Models;
using DownloadManager.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DownloadMananger.Tests
{
    public class DownloadManagerShould
    {
        private readonly Mock<IFileInformationProvider> _fileInfoProviderMock;
        private readonly Mock<IFileDownloaderManager> _fileDownloaderMock;
        private readonly Mock<IFileMerger> _fileMergerMock;
        private readonly Mock<IDownloadingTasksFactory> _downloadingTasksFactoryMock;

        private readonly IDownloadManager _downloadManager;

        public DownloadManagerShould()
        {
            Uri uri;

            _fileInfoProviderMock = new Mock<IFileInformationProvider>();
            _fileDownloaderMock = new Mock<IFileDownloaderManager>();
            _fileMergerMock = new Mock<IFileMerger>();
            _downloadingTasksFactoryMock = new Mock<IDownloadingTasksFactory>();

            _fileInfoProviderMock.Setup(m => m.ObtainInformation(It.IsAny<Uri>()));
            _fileInfoProviderMock.Setup(m => m.CheckIfUriHasValidFormat(It.IsAny<string>(), out uri)).Returns(true);
            _fileDownloaderMock.Setup(m => m.DownloadFile(It.IsAny<Uri>(), It.IsAny<TaskInformation[]>()));
            _fileMergerMock.Setup(m => m.Merge(It.IsAny<string[]>(), It.IsAny<string>()));
            _downloadingTasksFactoryMock.Setup(m => m.Create(It.IsAny<FileInformation>(), It.IsAny<int>()));

            var optionsMock = new Mock<IOptions<ApplicationOptions>>();
            optionsMock.SetupGet(m => m.Value).Returns(new ApplicationOptions());

            _downloadManager = new DownloadManager.Services.Impl.DownloadManager(_fileInfoProviderMock.Object,
                _fileMergerMock.Object, _fileDownloaderMock.Object, _downloadingTasksFactoryMock.Object, optionsMock.Object);
        }

        [Fact]
        public void ShouldMergeFilesAfterDownloading()
        {
            const string output = "output";
            var partialFiles = new[] { $"{output}_0", $"{output}_1" };

            var fileInfos = new List<TaskInformation>
                {new TaskInformation(partialFiles[0], 0, 0), new TaskInformation(partialFiles[1], 0, 0)};

            _fileInfoProviderMock.Setup(m => m.ObtainInformation(It.IsAny<Uri>()))
                .Returns(new FileInformation {Name = output});

            _downloadingTasksFactoryMock.Setup(m => m.Create(It.IsAny<FileInformation>(), It.IsAny<int>()))
                .Returns(fileInfos);

            var fileNames = _downloadManager.DownloadFile("output");

            _fileMergerMock.Verify(m => m.Merge(fileNames, output), Times.Once);
        }
    }
}