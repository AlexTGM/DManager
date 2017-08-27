using System;
using System.Diagnostics;
using SystemInterface.IO;
using SystemInterface.Net;
using DownloadManager.Models;

namespace DownloadManager.Services.Impl
{
    public class FileDownloader : IFileDownloader
    {
        private readonly IFile _file;
        private readonly IDateTimeProvider _dateTimeProvider;

        public FileDownloader(IFile file, IDateTimeProvider dateTimeProvider)
        {
            _file = file;
            _dateTimeProvider = dateTimeProvider;
        }

        public long SaveFile(IHttpWebResponse response, string fileName)
        {
            var downloadStarted = DateTime.Now;

            var downloadingCheckpoint = new DownloadingCheckpoint(_dateTimeProvider);
            
            var buffer = new byte[524288];
            var totalBytesWritten = 0L;

            using (var fileStream = _file.OpenWrite(fileName))
            {
                using (var stream = response.GetResponseStream())
                {
                    downloadingCheckpoint.Start();

                    var bytesDownloadedFromLastCheckpoint = 0D;
                    int bytesRead;

                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        bytesDownloadedFromLastCheckpoint += bytesRead;

                        if (downloadingCheckpoint.CheckpointReached())
                        {
                            var downloadingSpeed = bytesDownloadedFromLastCheckpoint / 
                                downloadingCheckpoint.MillisecondsSinceLastCheckpoint * 1000;

                            bytesDownloadedFromLastCheckpoint = 0;

                            var speed = new DownloadSpeed { BytesPerSecond = downloadingSpeed, FileName = fileName };
                            DownloadingSpeedChanged?.Invoke(this, speed);
                        }

                        fileStream.Write(buffer, 0, bytesRead);
                        totalBytesWritten += bytesRead;

                        var progress = new DownloadProgress {BytesDownloaded = bytesRead, FileName = fileName};
                        BytesDownloadedChanged?.Invoke(this, progress);
                    }
                }
            }

            var downloadFinished = DateTime.Now;

            return totalBytesWritten;
        }

        public event EventHandler<DownloadProgress> BytesDownloadedChanged;
        public event EventHandler<DownloadSpeed> DownloadingSpeedChanged;
    }
}