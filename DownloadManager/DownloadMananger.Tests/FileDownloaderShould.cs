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
    public class FileDownloaderShould
    {
        private readonly Mock<IFile> _fileMock;
        private readonly Mock<IStream> _streamMock;
        private readonly Mock<IHttpWebRequestFactory> _factoryMock;
        private readonly Mock<IHttpWebRequest> _httpWebRequest;
        private readonly Mock<IHttpWebResponse> _httpWebResponse;
        private readonly Mock<IFileStream> _fileStreamMock;
        private readonly Mock<ITasksRunner> _tasksRunner;

        private readonly IFileDownloader _fileDownloader;

        private readonly IEnumerable<TaskInformation> _taskInformations;

        public FileDownloaderShould()
        {
            _taskInformations = Enumerable.Repeat(new TaskInformation("", 0, 0), 10);

            _fileMock = new Mock<IFile>();
            _fileStreamMock = new Mock<IFileStream>();
            _streamMock = new Mock<IStream>();
            _factoryMock = new Mock<IHttpWebRequestFactory>();
            _httpWebRequest = new Mock<IHttpWebRequest>();
            _httpWebResponse = new Mock<IHttpWebResponse>();
            _tasksRunner = new Mock<ITasksRunner>();

            _fileStreamMock.Setup(m => m.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));

            _fileMock.Setup(m => m.OpenWrite(It.IsAny<string>()))
                .Returns(_fileStreamMock.Object);

            _streamMock.SetupGet(m => m.Length);
            _streamMock.Setup(m => m.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));

            _httpWebResponse.Setup(m => m.GetResponseStream())
                .Returns(_streamMock.Object);

            _httpWebRequest.Setup(m => m.GetResponse())
                .Returns(_httpWebResponse.Object);

            _factoryMock.Setup(m => m.CreateGetRangeRequest(It.IsAny<Uri>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(_httpWebRequest.Object);

            _tasksRunner.Setup(m => m.RunTasks<long>(It.IsAny<Func<long>[]>()));

            _fileDownloader = new FileDownloader(_fileMock.Object, _factoryMock.Object, _tasksRunner.Object);
        }

        [Fact]
        public async Task CreateTaskForEachTaskInformation()
        {
            const int expectedTasks = 6;

            await _fileDownloader.DownloadFile(It.IsAny<Uri>(), _taskInformations.Take(expectedTasks));
            _fileDownloader.DownloadingFunctions.Count.ShouldBeEquivalentTo(expectedTasks);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task DownloadFileToLocalStorage(int expectedTasks)
        {
            var currentIteration = 0;
            var streamReadOutput = new[] {10, 20, 30, 20, 10, 10, 0};

            _streamMock.SetupGet(m => m.Length).Returns(streamReadOutput.Sum());
            _streamMock.Setup(m => m.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() => streamReadOutput[currentIteration++]);

            _tasksRunner.Setup(m => m.RunTasks(It.IsAny<List<Func<long>>>()))
                .Returns(() =>
                {
                    var tcs = new TaskCompletionSource<long>();
                    tcs.SetResult(streamReadOutput.Sum());
                    return Enumerable.Repeat(tcs.Task, expectedTasks);
                });

            var actual = await _fileDownloader.DownloadFile(It.IsAny<Uri>(), _taskInformations.Take(expectedTasks));
            actual.ShouldBeEquivalentTo(_streamMock.Object.Length * expectedTasks);
        }
    }
}