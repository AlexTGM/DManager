using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class FileDownloaderManagerShould
    {
        private readonly Mock<IHttpWebRequestFactory> _factoryMock;
        private readonly Mock<IHttpWebRequest> _httpWebRequestMock;
        private readonly Mock<ITasksRunner> _tasksRunnerMock;
        private readonly Mock<IFileDownloader> _fileDownloaderMock;
        private readonly Mock<IHttpWebResponse> _httpWebResponseMock;

        private readonly IFileDownloaderManager _fileDownloaderManager;

        private readonly IEnumerable<TaskInformation> _taskInformations;

        public FileDownloaderManagerShould()
        {
            _taskInformations = Enumerable.Repeat(new TaskInformation("", 0, 0), 10);

            _factoryMock = new Mock<IHttpWebRequestFactory>();
            _httpWebRequestMock = new Mock<IHttpWebRequest>();
            _tasksRunnerMock = new Mock<ITasksRunner>();
            _fileDownloaderMock = new Mock<IFileDownloader>();
            _httpWebResponseMock = new Mock<IHttpWebResponse>();

            _httpWebRequestMock.Setup(m => m.GetResponse())
                .Returns(_httpWebResponseMock.Object);

            _factoryMock.Setup(m => m.CreateGetRangeRequest(It.IsAny<Uri>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(_httpWebRequestMock.Object);

            _tasksRunnerMock.Setup(m => m.RunTasks(It.IsAny<Func<long>[]>()));

            _fileDownloaderManager = new FileDownloaderManager(_factoryMock.Object, _tasksRunnerMock.Object, _fileDownloaderMock.Object);
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

            _tasksRunnerMock.Setup(m => m.RunTasks(It.IsAny<List<Func<long>>>()))
                .Returns(() =>
                {
                    var tcs = new TaskCompletionSource<long>();
                    tcs.SetResult(bytesPerTask);
                    return Enumerable.Repeat(tcs.Task, expectedTasks);
                });

            var actual = await _fileDownloaderManager.DownloadFile(It.IsAny<Uri>(), _taskInformations.Take(expectedTasks));
            actual.ShouldBeEquivalentTo(bytesPerTask * expectedTasks);
        }
    }
}