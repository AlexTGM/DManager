using System;
using SystemInterface.Timers;
using DownloadManager;
using DownloadManager.Models;
using DownloadManager.Services;
using DownloadManager.Services.Impl;
using DownloadManager.Tools;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DownloadMananger.Tests
{
    public class DownloadSpeedLimiterShould
    {
        private readonly Mock<ITimer> _timerMock = new Mock<ITimer>();
        private readonly Mock<ITimerFactory> _timerFactoryMock = new Mock<ITimerFactory>();
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        private readonly Mock<IOptions<ApplicationOptions>> _optionsMock = new Mock<IOptions<ApplicationOptions>>();

        private readonly IDownloadSpeedLimiter _downloadSpeedLimiter;

        private readonly long _downloadPerSecondThreshold = 1000;
        private readonly DownloadProgress _downloadProgress = new DownloadProgress(null, 1000);

        public DownloadSpeedLimiterShould()
        {
            _timerMock.Setup(m => m.Start()).Callback(() => _timerMock.SetupGet(g => g.Enabled).Returns(true));

            _timerFactoryMock.Setup(m => m.Create()).Returns(_timerMock.Object);

            _dateTimeProviderMock.SetupSequence(m => m.GetCurrentDateTime())
                .Returns(new DateTime(2000, 1, 1, 0, 0, 0))
                .Returns(new DateTime(2000, 1, 1, 0, 0, 1))
                .Returns(new DateTime(2000, 1, 1, 0, 0, 1))
                .Returns(new DateTime(2000, 1, 1, 0, 0, 1));

            _optionsMock.SetupGet(m => m.Value).Returns(new ApplicationOptions());

            _downloadSpeedLimiter =
                new DownloadSpeedLimiter(_timerFactoryMock.Object, _dateTimeProviderMock.Object, _optionsMock.Object)
                {
                    DownloadPerSecondThreshold = _downloadPerSecondThreshold
                };
        }

        [Fact]
        public void PauseDownloadingIfLimitIsReached()
        {
            _downloadSpeedLimiter.FileDownloaderBytesDownloaded(null, _downloadProgress);

            _downloadSpeedLimiter.IsPaused.ShouldBeEquivalentTo(true);
        }

        [Fact]
        public void StartTimerIfLimitIsReached()
        {
            _downloadSpeedLimiter.FileDownloaderBytesDownloaded(null, _downloadProgress);

            _downloadSpeedLimiter.Timer.Enabled.ShouldBeEquivalentTo(true);
        }

        [Fact]
        public void SetDateTimeNowWhenLimiterInitializes()
        {
            _downloadSpeedLimiter.CheckpointDateTime.ShouldBeEquivalentTo(new DateTime(2000, 1, 1, 0, 0, 0));
        }

        [Fact]
        public void UpdateCheckpointDateTimeWhenOneSecondIsPassed()
        {
            _downloadSpeedLimiter.FileDownloaderBytesDownloaded(null, _downloadProgress);

            _downloadSpeedLimiter.CheckpointDateTime.ShouldBeEquivalentTo(new DateTime(2000, 1, 1, 0, 0, 1));
        }

        [Fact]
        public void ResetBytesDownloadedPropertyWhenOneSecondIsPassed()
        {
            _downloadSpeedLimiter.FileDownloaderBytesDownloaded(null, _downloadProgress);

            _downloadSpeedLimiter.BytesDownloadedSinceLastCheckpoint.ShouldBeEquivalentTo(0);
        }

        [Fact]
        public void PauseDownloadingForPeriodOfTime()
        {
            const double timeLeft = 100;

            _dateTimeProviderMock.SetupSequence(m => m.GetCurrentDateTime())
                .Returns(new DateTime(2000, 1, 1, 0, 0, 0).AddMilliseconds(1000 - timeLeft));

            _timerMock.SetupSet<double>(m => m.Interval = timeLeft)
                      .Callback(time => _timerMock.SetupGet(timer => timer.Interval).Returns(timeLeft));

            _downloadSpeedLimiter.FileDownloaderBytesDownloaded(null, _downloadProgress);

            _downloadSpeedLimiter.Timer.Interval.ShouldBeEquivalentTo(timeLeft);
        }

        [Fact]
        public void UnpauseDownloadingWhenTimerElapsed()
        {
            _downloadSpeedLimiter.FileDownloaderBytesDownloaded(null, _downloadProgress);

            _timerMock.Raise(m => m.Elapsed += null, null, null);

            _downloadSpeedLimiter.IsPaused.ShouldBeEquivalentTo(false);
        }

        [Fact]
        public void UpdateCheckpointDateTimeWhenTimerElapsed()
        {
            _downloadSpeedLimiter.FileDownloaderBytesDownloaded(null, _downloadProgress);

            _timerMock.Raise(m => m.Elapsed += null, null, null);

            _downloadSpeedLimiter.CheckpointDateTime.ShouldBeEquivalentTo(new DateTime(2000, 1, 1, 0, 0, 1));
        }
    }
}