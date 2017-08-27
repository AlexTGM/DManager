using SystemInterface.IO;
using SystemInterface.Net;

namespace DownloadManager.Services.Impl
{
    public interface IFileDownloader
    {
        long SaveFile(IHttpWebResponse response, string fileName);
    }

    public class FileDownloader : IFileDownloader
    {
        private readonly IFile _file;

        public FileDownloader(IFile file)
        {
            _file = file;
        }

        public long SaveFile(IHttpWebResponse response, string fileName)
        {
            var buffer = new byte[524288];
            var totalBytesWritten = 0L;

            using (var fileStream = _file.OpenWrite(fileName))
            {
                using (var stream = response.GetResponseStream())
                {
                    int bytesRead;

                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);

                        totalBytesWritten += bytesRead;
                    }
                }
            }

            return totalBytesWritten;
        }
    }
}