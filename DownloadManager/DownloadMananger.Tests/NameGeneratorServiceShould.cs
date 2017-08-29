using DownloadManager.Services;
using DownloadManager.Services.Impl;
using DownloadManager.Tools;
using DownloadManager.Tools.Impl;
using FluentAssertions;
using Xunit;

namespace DownloadMananger.Tests
{
    public class NameGeneratorServiceShould
    {
        [Fact]
        public void CreateNameAccordingToPattern()
        {
            INameGenerator nameGeneratorService = new NameGenerator();

            const int taskId = 0;
            const string fileName = "output";
            var name = nameGeneratorService.GenerateName("output", 0);

            name.ShouldBeEquivalentTo($"{fileName}_{taskId}");
        }
    }
}