using System;
using System.Threading;
using System.Threading.Tasks;
using SystemInterface.IO;
using SystemInterface.Net;
using DownloadManager.Models;
using Lib.AspNetCore.ServerSentEvents;

namespace DownloadManager.Services.Impl
{
    public class FileDownloader : IFileDownloader
    {
        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(true);

        private readonly IFile _file;
        private readonly IDownloadSpeedMeter _downloadSpeedMeter;
        private readonly IDownloadSpeedLimiter _downloadSpeedLimiter;
        private readonly IServerSentEventsService _serverSentEventsService;

        public FileDownloader(IFile file, IDownloadSpeedMeter downloadSpeedMeter, 
            IDownloadSpeedLimiter downloadSpeedLimiter, IServerSentEventsService serverSentEventsService)
        {
            _file = file;
            _downloadSpeedMeter = downloadSpeedMeter;
            _downloadSpeedLimiter = downloadSpeedLimiter;
            _serverSentEventsService = serverSentEventsService;

            BytesDownloadedChanged += _downloadSpeedLimiter.FileDownloaderBytesDownloaded;
            BytesDownloadedChanged += _downloadSpeedMeter.FileDownloaderBytesDownloaded;

            TotalProgressChanged += DownloadingProgressChanged;
            _downloadSpeedMeter.DownloadingSpeedChanged += DownloadingSpeedChanged;
        }

        public bool IsPaused { get; set; }

        public event EventHandler<DownloadProgress> BytesDownloadedChanged;
        public event EventHandler<TotalProgress> TotalProgressChanged;

        public void Unsubscribe()
        {
            _autoResetEvent?.Dispose();

            BytesDownloadedChanged -= _downloadSpeedLimiter.FileDownloaderBytesDownloaded;
            BytesDownloadedChanged -= _downloadSpeedMeter.FileDownloaderBytesDownloaded;

            TotalProgressChanged -= DownloadingProgressChanged;
            _downloadSpeedMeter.DownloadingSpeedChanged -= DownloadingSpeedChanged;
        }

        public async Task<long> SaveFile(IHttpWebResponse response, TaskInformation taskInformation)
        {
            var buffer = new byte[524288];
            var totalBytesWritten = 0L;

            using (var fileStream = _file.OpenWrite(taskInformation.FileName))
            {
                using (var stream = response.GetResponseStream())
                {
                    int bytesRead;

                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        BytesDownloadedChanged?.Invoke(this, new DownloadProgress(taskInformation, bytesRead));

                        while (_downloadSpeedLimiter.IsPaused) await Task.Delay(10);

                        fileStream.Write(buffer, 0, bytesRead);

                        totalBytesWritten += bytesRead;
                        TotalProgressChanged?.Invoke(this, new TotalProgress(taskInformation, totalBytesWritten));
                    }
                }
            }

            return totalBytesWritten;
        }
        
        private void DownloadingSpeedChanged(object sender, DownloadSpeed args) => 
            SendEvent(new ServerSentEvent {Id = "speed", Data = new[] {$"{args.BytesPerSecond}"}});

        private void DownloadingProgressChanged(object sender, TotalProgress args)
        {
            var ev = new ServerSentEvent
            {
                Id = $"{args.TaskInformation.FileName}",
                Data = new[]
                {
                    $"{args.TotalBytesDownloaded} / {args.TaskInformation.BytesEnd - args.TaskInformation.BytesStart + 1} bytes"
                }
            };

            SendEvent(ev);
        }

        private void SendEvent(ServerSentEvent ev)
        {
            _autoResetEvent.WaitOne();
            _serverSentEventsService.SendEventAsync(ev);
            _autoResetEvent.Set();
        }
    }
}