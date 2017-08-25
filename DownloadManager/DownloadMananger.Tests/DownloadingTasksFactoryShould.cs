using System;
using System.Linq;
using DownloadManager.Factories;
using DownloadManager.Factories.Impl;
using DownloadManager.Models;
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
            _downloadingTasksFactory = new DownloadingTasksFactory(TODO);
        }

        [Fact]
        public void CreateTasksAccordingToTasksCount()
        {
            var fileInfo = new FileInformation {ContentLength = ContentLength};

            var tasks = _downloadingTasksFactory.Create(fileInfo, TasksCount);
            tasks.Count.ShouldBeEquivalentTo(TasksCount);
        }

        [Fact]
        public void ShouldGeneratePartialNamesAccordingToPattern()
        {
            const string output = "output";
            var partialFiles = new[] { $"{output}_0", $"{output}_1" };

            var fileInfo = new FileInformation { ContentLength = ContentLength, Name = output};

            var tasks = _downloadingTasksFactory.Create(fileInfo, 2);
            tasks.Select(task => task.FileName).ShouldBeEquivalentTo(partialFiles);
        }

        [Fact]
        public void ComputeBytesRangesCorrectly()
        {
            var fileInfo = new FileInformation { ContentLength = ContentLength };

            var tasks = _downloadingTasksFactory.Create(fileInfo, TasksCount);
            tasks.Last().BytesEnd.ShouldBeEquivalentTo(ContentLength);
        }
    }
}