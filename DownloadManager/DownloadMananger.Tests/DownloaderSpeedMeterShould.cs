using System;
using System.Timers;
using SystemInterface.Timers;
using DownloadManager.Models;
using DownloadManager.Services;
using DownloadManager.Services.Impl;
using DownloadManager.Tools;
using FluentAssertions;
using Moq;
using Xunit;

namespace DownloadMananger.Tests
{
    public class DownloadSpeedMeterShould
    {
        private readonly Mock<ITimer> _timerMock = new Mock<ITimer>();
        private readonly Mock<ITimerFactory> _timerFactoryMock = new Mock<ITimerFactory>();
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        private readonly IDownloadSpeedMeter _downloadSpeedMeter;

        public DownloadSpeedMeterShould()
        {
            _timerMock.Setup(m => m.Start()).Callback(() => _timerMock.SetupGet(timer => timer.Enabled).Returns(true));

            _dateTimeProviderMock.SetupSequence(m => m.GetCurrentDateTime())
                .Returns(new DateTime(2000, 1, 1, 0, 0, 0))
                .Returns(new DateTime(2000, 1, 1, 0, 0, 1))
                .Returns(new DateTime(2000, 1, 1, 0, 0, 1));

            _timerFactoryMock.Setup(m => m.Create(1000))
                .Returns(_timerMock.Object);

            _downloadSpeedMeter = new DownloadSpeedMeter(_dateTimeProviderMock.Object, _timerFactoryMock.Object);
        }

        [Fact]
        public void CreateCheckpointOnStart()
        {
            _downloadSpeedMeter.CheckpointDateTime.ShouldBeEquivalentTo(new DateTime(2000, 1, 1, 0, 0, 0));
        }

        [Fact]
        public void InitializeAllPropertiesOnStartup()
        {
            _downloadSpeedMeter.BytesDownloadedSinceLastCheckpoint.ShouldBeEquivalentTo(0L);
            _downloadSpeedMeter.BytesPerSecond.ShouldBeEquivalentTo(0D);
        }

        [Fact]
        public void AddDownloadedBytes()
        {
            _dateTimeProviderMock.SetupSequence(m => m.GetCurrentDateTime())
                .Returns(new DateTime(2000, 1, 1, 0, 0, 0))
                .Returns(new DateTime(2000, 1, 1, 0, 0, 0));

            _downloadSpeedMeter.FileDownloaderBytesDownloaded(null, new DownloadProgress(null, 100));

            _downloadSpeedMeter.BytesDownloadedSinceLastCheckpoint.ShouldBeEquivalentTo(100);
        }

        [Fact]
        public void MeasureDownloadingSpeedWhenSecondSinceLastCheckpointIsPassed()
        {
            _downloadSpeedMeter.FileDownloaderBytesDownloaded(null, new DownloadProgress(null, 1000));

            _timerMock.Raise(m => m.Elapsed += null, It.IsAny<object>(), It.IsAny<ElapsedEventArgs>());

            _downloadSpeedMeter.BytesPerSecond.ShouldBeEquivalentTo(1000D);
        }

        [Fact]
        public void ResetDownloadedBytesWhenSpeedIsMeasured()
        {
            _downloadSpeedMeter.FileDownloaderBytesDownloaded(null, new DownloadProgress(null, 1000));

            _timerMock.Raise(m => m.Elapsed += null, It.IsAny<object>(), It.IsAny<ElapsedEventArgs>());

            _downloadSpeedMeter.BytesDownloadedSinceLastCheckpoint.ShouldBeEquivalentTo(0D);
        }

        [Fact]
        public void ResetCheckpointDateTimeWhenSpeedIsMeasured()
        {
            _downloadSpeedMeter.FileDownloaderBytesDownloaded(null, new DownloadProgress(null, 1000));

            _timerMock.Raise(m => m.Elapsed += null, It.IsAny<object>(), It.IsAny<ElapsedEventArgs>());

            _downloadSpeedMeter.CheckpointDateTime.ShouldBeEquivalentTo(new DateTime(2000, 1, 1, 0, 0, 1));
        }

        [Fact]
        public void RaiseDownloadSpeedChangedEvent()
        {
            _downloadSpeedMeter.DownloadingSpeedChanged += (sender, speed) => speed.BytesPerSecond.ShouldBeEquivalentTo(1000);

            _downloadSpeedMeter.FileDownloaderBytesDownloaded(null, new DownloadProgress(null, 1000));

            _timerMock.Raise(m => m.Elapsed += null, It.IsAny<object>(), It.IsAny<ElapsedEventArgs>());
        }
    }
}