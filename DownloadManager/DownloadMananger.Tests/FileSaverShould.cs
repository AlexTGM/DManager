using SystemInterface.IO;
using DownloadManager.Services;
using DownloadManager.Services.Impl;
using Moq;
using Xunit;

namespace DownloadMananger.Tests
{
    public class FileSaverShould
    {
        [Fact]
        public void SaveFileFromStreamToLocalStorage()
        {
            var fileMock = new Mock<IFile>();
            var streamMock = new Mock<IStream>();

            IFileSaver fileSaver = new FileSaver(fileMock.Object);

            fileSaver.SaveFile(streamMock.Object, It.IsAny<string>());
        }
    }
}