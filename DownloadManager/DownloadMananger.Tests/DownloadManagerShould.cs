using FluentAssertions;
using Xunit;

namespace DownloadMananger.Tests
{
    public class DownloadManagerShould
    {
        [Theory]
        [InlineData(2048)]
        [InlineData(1024)]
        [InlineData(512)]
        public void LimitDownloadSpeed(int expected)
        {
            var manager = new DownloadManager(expected);
            
            manager.MaximumSpeed.ShouldBeEquivalentTo(expected);
        }

        public void DownloadData()
        {
            
        }
    }

    public interface IDownloadManager
    {
        int MaximumSpeed { get; }
    }

    public class DownloadManager : IDownloadManager
    {
        public DownloadManager(int maximumSpeed)
        {
            MaximumSpeed = maximumSpeed;
        }

        public int MaximumSpeed { get; }
    }
}