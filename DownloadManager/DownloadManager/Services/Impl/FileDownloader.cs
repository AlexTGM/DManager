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
        private readonly IDownloadSpeedMeter _downloadSpeedMeter;
        private readonly IDownloadSpeedLimiter _downloadSpeedLimiter;

        public FileDownloader(IFile file, IDownloadSpeedMeter downloadSpeedMeter,
            IDownloadSpeedLimiter downloadSpeedLimiter)
        {
            _file = file;
            _downloadSpeedMeter = downloadSpeedMeter;
            _downloadSpeedLimiter = downloadSpeedLimiter;

            _downloadSpeedLimiter.DownloadPerSecondThreshold = (long)625000;

            BytesDownloadedChanged += _downloadSpeedLimiter.FileDownloaderBytesDownloaded;
            BytesDownloadedChanged += _downloadSpeedMeter.FileDownloaderBytesDownloaded;

            _downloadSpeedMeter.DownloadingSpeedChanged += PrintSpeedForTesting;
        }

        public void Unsubscribe()
        {
            BytesDownloadedChanged -= _downloadSpeedLimiter.FileDownloaderBytesDownloaded;
            BytesDownloadedChanged -= _downloadSpeedMeter.FileDownloaderBytesDownloaded;

            _downloadSpeedMeter.DownloadingSpeedChanged -= PrintSpeedForTesting;
        }

        public async Task<long> SaveFile(IHttpWebResponse response, string fileName)
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
                        BytesDownloadedChanged?.Invoke(this, CreateProgress(fileName, bytesRead));

                        while (_downloadSpeedLimiter.IsPaused) await Task.Delay(10);

                        fileStream.Write(buffer, 0, bytesRead);
                        totalBytesWritten += bytesRead;
                    }
                }
            }

            return totalBytesWritten;
        }

        public bool IsPaused { get; set; }

        public event EventHandler<DownloadProgress> BytesDownloadedChanged;

        private static void PrintSpeedForTesting(object sender, DownloadSpeed args)
        {
            Debug.WriteLine($"{DateTime.Now:O} speed: {args.BytesPerSecond * 8e-6} mbits");
        }

        private DownloadProgress CreateProgress(string fileName, long bytesDownloaded)
            => new DownloadProgress {BytesDownloaded = bytesDownloaded, FileName = fileName};
    }
}