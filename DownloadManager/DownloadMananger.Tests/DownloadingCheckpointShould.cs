using System;
using DownloadManager.Services;
using DownloadManager.Services.Impl;
using FluentAssertions;
using Moq;
using Xunit;

namespace DownloadMananger.Tests
{
    public class DownloadingCheckpointShould
    {
        [Fact]
        public void IndicateThatCheckpointIsReached()
        {
            var dateTimeProvider = new Mock<IDateTimeProvider>();

            dateTimeProvider.Setup(m => m.GetCurrentDateTime()).Returns(new DateTime(1, 1, 1, 0, 0, 0));

            IDownloadingCheckpoint downloadingCheckpoint = new DownloadingCheckpoint(dateTimeProvider.Object);
            downloadingCheckpoint.Start();

            dateTimeProvider.Setup(m => m.GetCurrentDateTime()).Returns(new DateTime(1, 1, 1, 0, 0, 1));

            var result = downloadingCheckpoint.CheckpointReached();

            result.ShouldBeEquivalentTo(true);
        }
    }
}