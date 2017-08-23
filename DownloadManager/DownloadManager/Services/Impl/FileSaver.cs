using System;
using SystemInterface.IO;

namespace DownloadManager.Services.Impl
{
    public class FileSaver : IFileSaver
    {
        private readonly IFile _file;

        public FileSaver(IFile file)
        {
            _file = file;
        }

        public void SaveFile(IStream responseStream)
        {
            var buffer = new byte[524288];

            using (var fileStream = _file.OpenWrite($"{DateTime.Now.Ticks}"))
            {
                int bytesRead;

                while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                }
            }
        }
    }
}