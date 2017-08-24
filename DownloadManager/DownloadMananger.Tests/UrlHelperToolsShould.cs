using DownloadManager.Services;
using DownloadManager.Services.Impl;
using FluentAssertions;
using Xunit;

namespace DownloadMananger.Tests
{
    public class UrlHelperToolsShould
    {
        [Fact]
        public void DecodeStringToUri()
        {
            IUrlHelperTools helperTools = new UrlHelperTools();

            var actual = helperTools.UrlDecode("http%3A//test.domain/test.file");

            actual.ShouldBeEquivalentTo("http://test.domain/test.file");
        }
    }
}