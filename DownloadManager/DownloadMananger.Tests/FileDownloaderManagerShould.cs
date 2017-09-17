using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemInterface.Net;
using DownloadManager;
using DownloadManager.Models;
using DownloadManager.Services;
using DownloadManager.Services.Impl;
using FluentAssertions;
using Microsoft.Extensions.Options;
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
        private readonly Mock<IOptions<ApplicationOptions>> _optionsMock;

        private readonly IFileDownloaderManager _fileDownloaderManager;

        private readonly IEnumerable<TaskInformation> _taskInformations;

        public FileDownloaderManagerShould()
        {
            _taskInformations = Enumerable.Repeat(new TaskInformation("", 0, 0), 10);

            _factoryMock = new Mock<IHttpWebRequestFactory>();
            _optionsMock = new Mock<IOptions<ApplicationOptions>>();
            _httpWebRequestMock = new Mock<IHttpWebRequest>();
            _fileDownloaderMock = new Mock<IFileDownloader>();
            _httpWebResponseMock = new Mock<IHttpWebResponse>();

            _optionsMock.SetupGet(m => m.Value).Returns(new ApplicationOptions());

            _httpWebRequestMock.Setup(m => m.GetResponse())
                .Returns(_httpWebResponseMock.Object);

            _factoryMock.Setup(m => m.CreateGetRangeRequest(It.IsAny<Uri>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(_httpWebRequestMock.Object);

            _fileDownloaderMock.Setup(m => m.SaveFile(_httpWebResponseMock.Object, It.IsAny<TaskInformation>()))
                .Returns(async () => await Task.FromResult(100L));

            _fileDownloaderManager =
                new FileDownloaderManager(_factoryMock.Object, _fileDownloaderMock.Object, _optionsMock.Object);
        }

        [Fact]
        public async Task CreateTaskForEachTaskInformation()
        {
            const int expectedTasks = 6;

            await _fileDownloaderManager.DownloadFile(It.IsAny<Uri>(), _taskInformations.Take(expectedTasks));
            _fileDownloaderManager.DownloadingFunctions.Count.ShouldBeEquivalentTo(expectedTasks);
        }
    }
}