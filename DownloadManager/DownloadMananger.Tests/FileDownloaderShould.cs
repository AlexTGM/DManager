﻿using System;
using System.Linq;
using System.Threading.Tasks;
using SystemInterface.IO;
using SystemInterface.Net;
using DownloadManager;
using DownloadManager.Models;
using DownloadManager.Services;
using DownloadManager.Services.Impl;
using DownloadManager.Tools;
using FluentAssertions;
using Lib.AspNetCore.ServerSentEvents;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DownloadMananger.Tests
{
    public class FileDownloaderShould
    {
        private readonly Mock<IFile> _fileMock = new Mock<IFile>();
        private readonly Mock<IStream> _streamMock = new Mock<IStream>();
        private readonly Mock<IFileStream> _fileStreamMock = new Mock<IFileStream>();
        private readonly Mock<IHttpWebResponse> _httpWebResponseMock = new Mock<IHttpWebResponse>();
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        private readonly Mock<IDownloadSpeedMeter> _downloadSpeedMeterMock = new Mock<IDownloadSpeedMeter>();
        private readonly Mock<IDownloadSpeedLimiter> _downloadSpeedLimiterMock = new Mock<IDownloadSpeedLimiter>();
        private readonly Mock<IServerSentEventsService> _serverSentEventsServiceMock = new Mock<IServerSentEventsService>();

        private readonly IFileDownloader _fileDownloader;

        public FileDownloaderShould()
        {
            _streamMock.SetupGet(m => m.Length);
            _streamMock.Setup(m => m.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));
            _fileStreamMock.Setup(m => m.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));
            _fileMock.Setup(m => m.OpenWrite(It.IsAny<string>())).Returns(_fileStreamMock.Object);
            _httpWebResponseMock.Setup(m => m.GetResponseStream()).Returns(_streamMock.Object);
            _dateTimeProviderMock.Setup(m => m.GetCurrentDateTime());
            _serverSentEventsServiceMock.Setup(m => m.SendEventAsync(It.IsAny<ServerSentEvent>()));

            var optionsMock = new Mock<IOptions<ApplicationOptions>>();
            optionsMock.SetupGet(m => m.Value).Returns(new ApplicationOptions());

            _fileDownloader = new FileDownloader(_fileMock.Object, _downloadSpeedMeterMock.Object, 
                _downloadSpeedLimiterMock.Object, _serverSentEventsServiceMock.Object);
        }

        [Fact]
        public async Task DownloadFile()
        {
            _streamMock.SetupGet(m => m.Length).Returns(new[] { 10, 20, 30, 20, 10, 10, 0 }.Sum());
            _streamMock.SetupSequence(m => m.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(10).Returns(20).Returns(30).Returns(20).Returns(10).Returns(10).Returns(0);

            var fileSize =
                await _fileDownloader.SaveFile(_httpWebResponseMock.Object, new TaskInformation(string.Empty, 0, 0));

            fileSize.ShouldBeEquivalentTo(new[] { 10, 20, 30, 20, 10, 10, 0 }.Sum());
        }

        [Fact]
        public void InvokeBytesDownloadedEvent()
        {
            var totalBytesDownloaded = 0L;

            _streamMock.SetupGet(m => m.Length).Returns(new [] { 10, 20, 30, 20, 10, 10, 0 }.Sum());
            _streamMock.SetupSequence(m => m.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(10).Returns(20).Returns(30).Returns(20).Returns(10).Returns(10).Returns(0);

            _fileDownloader.BytesDownloadedChanged += (sender, progress) => totalBytesDownloaded += progress.BytesDownloaded;

            _fileDownloader.SaveFile(_httpWebResponseMock.Object, new TaskInformation(string.Empty, 0, 0));

            totalBytesDownloaded.ShouldBeEquivalentTo(new [] { 10, 20, 30, 20, 10, 10, 0 }.Sum());
        }
    }
}