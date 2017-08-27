using System;
using System.Linq;
using DownloadManager.Factories;
using DownloadManager.Factories.Impl;
using DownloadManager.Models;
using DownloadManager.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace DownloadMananger.Tests
{
    public class DownloadingTasksFactoryShould
    {
        private const long ContentLength = 10000;
        private const int TasksCount = 4;

        private readonly IDownloadingTasksFactory _downloadingTasksFactory;

        public DownloadingTasksFactoryShould()
        {
            var nameGeneratorServiceMock = new Mock<INameGeneratorService>();
            nameGeneratorServiceMock.Setup(m => m.GenerateName(It.IsAny<string>(), It.IsAny<int>()))
                .Returns((string fileName, int taskId) => $"{fileName}_{taskId}");

            _downloadingTasksFactory = new DownloadingTasksFactory(nameGeneratorServiceMock.Object);
        }

        [Fact]
        public void CreateTasksAccordingToTasksCount()
        {
            var fileInfo = new FileInformation {ContentLength = ContentLength};

            var tasks = _downloadingTasksFactory.Create(fileInfo, TasksCount);
            tasks.Count().ShouldBeEquivalentTo(TasksCount);
        }

        [Fact]
        public void ShouldGeneratePartialNamesAccordingToPattern()
        {
            const string output = "output";
            var partialFiles = new[] { $"{output}_0", $"{output}_1" };

            var fileInfo = new FileInformation { Name = output};

            var tasks = _downloadingTasksFactory.Create(fileInfo, 2);
            tasks.Select(task => task.FileName).ShouldBeEquivalentTo(partialFiles);
        }

        [Theory]
        [InlineData(10000, 10)]
        [InlineData(777, 2)]
        [InlineData(15000, 8)]
        [InlineData(7, 8)]
        public void ComputeBytesRangesCorrectly(long contentLength, int tasksCount)
        {
            var fileInfo = new FileInformation { ContentLength = contentLength };

            var tasks = _downloadingTasksFactory.Create(fileInfo, tasksCount);
            tasks.Sum(t => t.BytesEnd - t.BytesStart + 1).ShouldBeEquivalentTo(contentLength + 1);
        }
    }
}