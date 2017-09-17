using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class FileDownloaderManagerShould
    {
        private readonly Mock<IHttpWebRequestFactory> _factoryMock;
        private readonly Mock<IHttpWebRequest> _httpWebRequestMock;
        private readonly Mock<IFileDownloader> _fileDownloaderMock;
        private readonly Mock<IHttpWebResponse> _httpWebResponseMock;

        private readonly IFileDownloaderManager _fileDownloaderManager;

        private readonly IEnumerable<TaskInformation> _taskInformations;

        public FileDownloaderManagerShould()
        {
            _taskInformations = Enumerable.Repeat(new TaskInformation("", 0, 0), 10);

            _factoryMock = new Mock<IHttpWebRequestFactory>();
            _httpWebRequestMock = new Mock<IHttpWebRequest>();
            _fileDownloaderMock = new Mock<IFileDownloader>();
            _httpWebResponseMock = new Mock<IHttpWebResponse>();

            _httpWebRequestMock.Setup(m => m.GetResponse())
                .Returns(_httpWebResponseMock.Object);

            _factoryMock.Setup(m => m.CreateGetRangeRequest(It.IsAny<Uri>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(_httpWebRequestMock.Object);

            _fileDownloaderMock.Setup(m => m.SaveFile(_httpWebResponseMock.Object, It.IsAny<TaskInformation>()))
                .Returns(async () => await Task.FromResult(100L));

            _fileDownloaderManager = new FileDownloaderManager(_factoryMock.Object, _fileDownloaderMock.Object);
        }

        [Fact]
        public async Task CreateTaskForEachTaskInformation()
        {
            const int expectedTasks = 6;

            await _fileDownloaderManager.DownloadFile(It.IsAny<Uri>(), _taskInformations.Take(expectedTasks));
            _fileDownloaderManager.DownloadingFunctions.Count.ShouldBeEquivalentTo(expectedTasks);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task DownloadFileToLocalStorage(int expectedTasks)
        {
            const int bytesPerTask = 100;

            var actual = await _fileDownloaderManager.DownloadFile(It.IsAny<Uri>(), _taskInformations.Take(expectedTasks));
            actual.ShouldBeEquivalentTo(bytesPerTask * expectedTasks);
        }

        [Fact]
        public void ListenToFileDownloaderEvents()
        {
            var eventsData = new[] {10L, 50L, 0L, 40L};

            foreach (var loaded in eventsData)
            {
                var downloadProgress = new DownloadProgress(null, loaded);
                _fileDownloaderMock.Raise(m => m.BytesDownloadedChanged += null, _fileDownloaderMock, downloadProgress);
            }

            _fileDownloaderManager.DownloadFile(It.IsAny<Uri>(), _taskInformations.Take(1));
            _fileDownloaderManager.TotalBytesDownloaded.ShouldBeEquivalentTo(eventsData.Sum());
        }
    }
}