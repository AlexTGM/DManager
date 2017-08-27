using DownloadManager.Services;
using DownloadManager.Services.Impl;
using FluentAssertions;
using Xunit;

namespace DownloadMananger.Tests
{
    public class NameGeneratorServiceShould
    {
        [Fact]
        public void CreateNameAccordingToPattern()
        {
            INameGeneratorService nameGeneratorService = new NameGeneratorService();

            const int taskId = 0;
            const string fileName = "output";
            var name = nameGeneratorService.GenerateName("output", 0);

            name.ShouldBeEquivalentTo($"{fileName}_{taskId}");
        }
    }
}