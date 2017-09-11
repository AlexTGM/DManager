using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SystemInterface.IO;
using SystemInterface.Net;
using DownloadManager.Models;
using Lib.AspNetCore.ServerSentEvents;
using Microsoft.Extensions.Options;

namespace DownloadManager.Services.Impl
{
    public class FileDownloader : IFileDownloader
    {
        private AutoResetEvent autoResetEvent = new AutoResetEvent(true);

        private readonly IFile _file;
        private readonly IDownloadSpeedMeter _downloadSpeedMeter;
        private readonly IDownloadSpeedLimiter _downloadSpeedLimiter;
        private readonly IServerSentEventsService _serverSentEventsService;

        public FileDownloader(IFile file, IDownloadSpeedMeter downloadSpeedMeter, 
            IDownloadSpeedLimiter downloadSpeedLimiter, IServerSentEventsService serverSentEventsService,
            IOptions<ApplicationOptions> applicationOptions)
        {
            _file = file;
            _downloadSpeedMeter = downloadSpeedMeter;
            _downloadSpeedLimiter = downloadSpeedLimiter;
            _serverSentEventsService = serverSentEventsService;

            _downloadSpeedLimiter.DownloadPerSecondThreshold = applicationOptions.Value.BytesPerSecond;

            BytesDownloadedChanged += _downloadSpeedLimiter.FileDownloaderBytesDownloaded;
            BytesDownloadedChanged += _downloadSpeedMeter.FileDownloaderBytesDownloaded;

            TotalProgressChanged += DownloadingProgressChanged;
            _downloadSpeedMeter.DownloadingSpeedChanged += DownloadingSpeedChanged;
        }

        public void Unsubscribe()
        {
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
                        BytesDownloadedChanged?.Invoke(this, CreateProgress(taskInformation, bytesRead));

                        while (_downloadSpeedLimiter.IsPaused) await Task.Delay(10);

                        fileStream.Write(buffer, 0, bytesRead);

                        totalBytesWritten += bytesRead;
                        TotalProgressChanged?.Invoke(this, CreateTotal(taskInformation, totalBytesWritten));
                    }
                }
            }

            return totalBytesWritten;
        }

        public bool IsPaused { get; set; }
        
        public event EventHandler<DownloadProgress> BytesDownloadedChanged;
        public event EventHandler<TotalProgress> TotalProgressChanged;
        
        private void DownloadingSpeedChanged(object sender, DownloadSpeed args)
        {
            var ev = new ServerSentEvent { Id = "speed", Data = new[] { $"{args.BytesPerSecond}" } };

            SendEvent(ev);
        }

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
            autoResetEvent.WaitOne();
            _serverSentEventsService.SendEventAsync(ev);
            autoResetEvent.Set();
        }

        private TotalProgress CreateTotal(TaskInformation taskInformation, long progress)
            => new TotalProgress {TaskInformation = taskInformation, TotalBytesDownloaded = progress};

        private DownloadProgress CreateProgress(TaskInformation taskInformation, long bytesDownloaded)
            => new DownloadProgress {BytesDownloaded = bytesDownloaded, TaskInformation = taskInformation };
    }
}