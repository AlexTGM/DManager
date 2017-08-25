using System;
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
        [Fact]
        public void DownloadFileToLocalStorage()
        {
            var currentIteration = 0;
            var streamReadOutput = new[] {10, 20, 30, 20, 10, 10, 0};

            var fileMock = new Mock<IFile>();
            var fileStreamMock = new Mock<IFileStream>();
            var streamMock = new Mock<IStream>();
            var factoryMock = new Mock<IHttpWebRequestFactory>();
            var httpWebRequest = new Mock<IHttpWebRequest>();
            var httpWebResponse = new Mock<IHttpWebResponse>();

            fileStreamMock.Setup(m => m.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));

            fileMock.Setup(m => m.OpenWrite(It.IsAny<string>()))
                .Returns(fileStreamMock.Object);

            streamMock.SetupGet(m => m.Length).Returns(100);
            streamMock.Setup(m => m.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() => streamReadOutput[currentIteration++]);

            httpWebResponse.Setup(m => m.GetResponseStream())
                .Returns(streamMock.Object);

            httpWebRequest.Setup(m => m.GetResponse())
                .Returns(httpWebResponse.Object);

            factoryMock.Setup(m => m.CreateGetRangeRequest(It.IsAny<Uri>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(httpWebRequest.Object);

            IFileDownloader fileDownloader = new FileDownloader(fileMock.Object, factoryMock.Object);

            var actual = fileDownloader.DownloadFile(new TaskInformation(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Uri>()));
            actual.ShouldBeEquivalentTo(streamMock.Object.Length);
        }
    }
}