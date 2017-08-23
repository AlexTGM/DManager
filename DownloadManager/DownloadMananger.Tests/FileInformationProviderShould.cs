using System;
using System.Net;
using SystemInterface.Net;
using DownloadManager.Models;
using DownloadManager.Services;
using DownloadManager.Services.Impl;
using FluentAssertions;
using Moq;
using Xunit;

namespace DownloadMananger.Tests
{
    public class FileInformationProviderShould
    {
        [Fact]
        public void GetFileInformation()
        {
            const int expectedContentLength = 123;
            const string expectedAcceptRanges = "bytes";
            const string expectedFileName = "test";

            var expectation = new FileInformation
            {
                AcceptRanges = expectedAcceptRanges,
                ContentLength = expectedContentLength,
                Name = expectedFileName
            };

            var webHeaderCollection = new WebHeaderCollection {{HttpResponseHeader.AcceptRanges, expectedAcceptRanges } };

            var httpWebResponseMock = new Mock<IHttpWebResponse>();
            httpWebResponseMock.SetupGet(m => m.ContentLength).Returns(expectedContentLength);
            httpWebResponseMock.SetupGet(m => m.Headers).Returns(webHeaderCollection);

            var httpWebRequestMock = new Mock<IHttpWebRequest>();
            httpWebRequestMock.Setup(m => m.GetResponse()).Returns(httpWebResponseMock.Object);

            var httpWebRequestFactoryMock = new Mock<IHttpWebRequestFactory>();
            httpWebRequestFactoryMock.Setup(m => m.Create(It.IsAny<Uri>())).Returns(httpWebRequestMock.Object);

            IFileInformationProvider provider = new FileInformationProvider(httpWebRequestFactoryMock.Object);

            provider.ObtainInformation(new Uri($"http://test.domain/{expectedFileName}")).ShouldBeEquivalentTo(expectation);
        }
    }
}