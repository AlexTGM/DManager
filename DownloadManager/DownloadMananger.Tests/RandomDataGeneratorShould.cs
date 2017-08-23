using SystemInterface.IO;
using DownloadManager.Services;
using DownloadManager.Services.Impl;
using FluentAssertions;
using Moq;
using Xunit;

namespace DownloadMananger.Tests
{
    public class RandomDataGeneratorShould
    {
        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void GenerateRandomDataWithExactSize(int expected)
        {
            var fileMock = new Mock<IFile>();
            var fileStreamMock = new Mock<IFileStream>();

            fileMock.Setup(m => m.OpenWrite(It.IsAny<string>())).Returns(fileStreamMock.Object);
            fileStreamMock.Setup(m => m.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));

            IDataGenerator generator = new DataGenerator(fileMock.Object);
            generator.GenerateRandomData(expected);

            fileStreamMock.Verify(m => m.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(expected * 128));
        }

        [Theory]
        [InlineData(10, "generated_10mb")]
        [InlineData(100, "generated_100mb")]
        public void GenerateFileWithExactName(int expectedSize, string expectedName)
        {
            var fileMock = new Mock<IFile>();
            var fileStreamMock = new Mock<IFileStream>();

            fileMock.Setup(m => m.OpenWrite(It.IsAny<string>())).Returns(fileStreamMock.Object);
            fileStreamMock.Setup(m => m.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));

            IDataGenerator generator = new DataGenerator(fileMock.Object);
            var actualfileName = generator.GenerateRandomData(expectedSize);

            actualfileName.ShouldBeEquivalentTo(expectedName);
        }
    }
}