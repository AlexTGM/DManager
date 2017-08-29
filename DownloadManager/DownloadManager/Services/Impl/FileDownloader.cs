using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SystemInterface.IO;
using SystemInterface.Net;
using DownloadManager.Models;

namespace DownloadManager.Services.Impl
{
    public class FileDownloader : IFileDownloader
    {
        private readonly IFile _file;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDownloadSpeedLimiter _downloadSpeedLimiter;

        public FileDownloader(IFile file, IDateTimeProvider dateTimeProvider,
            IDownloadSpeedLimiter downloadSpeedLimiter)
        {
            _file = file;
            _dateTimeProvider = dateTimeProvider;
            _downloadSpeedLimiter = downloadSpeedLimiter;

            _downloadSpeedLimiter.DownloadPerSecondThreshold = (long)2.5e+6;

            BytesDownloadedChanged += _downloadSpeedLimiter.FileDownloaderBytesDownloaded;
        }

        public async Task<long> SaveFile(IHttpWebResponse response, string fileName)
        {
            var dateStarted = DateTime.Now;

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
                        var progress = new DownloadProgress { BytesDownloaded = bytesRead, FileName = fileName };
                        BytesDownloadedChanged?.Invoke(this, progress);

                        while (_downloadSpeedLimiter.IsPaused)
                        {
                            await Task.Delay(10);
                        }
                        
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
                    }
                }
            }

            var dateEnded = DateTime.Now;
            var timeTaken = (dateEnded - dateStarted).TotalSeconds;
            var averageSpeed = totalBytesWritten / timeTaken;

            Debug.WriteLine($"{averageSpeed}");

            return totalBytesWritten;
        }

        public bool IsPaused { get; set; }

        public event EventHandler<DownloadProgress> BytesDownloadedChanged;
        public event EventHandler<DownloadSpeed> DownloadingSpeedChanged;
    }
}