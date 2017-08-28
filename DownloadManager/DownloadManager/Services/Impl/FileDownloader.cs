using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using SystemInterface.IO;
using SystemInterface.Net;
using DownloadManager.Models;
using Timer = System.Threading.Timer;

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

            BytesDownloadedChanged += FileDownloader_BytesDownloadedChanged;

            timer = new Timer(state =>
            {
                Write($"{DateTime.Now:O} ===== Checkpoint reached");
                _checkpoint = DateTime.Now;
                IsPaused = false;
            });

            _checkpoint = DateTime.Now;
        }

        private DateTime _checkpoint;
        private long _bytesFromLastCheckpointDownloaded;

        Timer timer = null;

        private void FileDownloader_BytesDownloadedChanged(object sender, DownloadProgress e)
        {
            var time = DateTime.Now - _checkpoint;
            var timeToWait = Math.Max(0L, (long)(1000 - time.TotalMilliseconds));

            if (time.TotalMilliseconds >= 1000)
            {
                _checkpoint = DateTime.Now;
                _bytesFromLastCheckpointDownloaded = 0;
            }

            _bytesFromLastCheckpointDownloaded += e.BytesDownloaded;

            Write($"{DateTime.Now:O} timeFromLastCheckpoint: {time.TotalMilliseconds} || timeToWait: {timeToWait} ||" +
                  $"downloaded: {_bytesFromLastCheckpointDownloaded}");

            if (_bytesFromLastCheckpointDownloaded < 1000000) return;

            IsPaused = true;
            _bytesFromLastCheckpointDownloaded = 0;

            Write($"{DateTime.Now:O} ===== Limit Is Reached!");

            timer.Change(timeToWait, Timeout.Infinite);
        }

        StringBuilder sb = new StringBuilder();

        AutoResetEvent writeResetEvent = new AutoResetEvent(true);

        private void Write(string str)
        {
            writeResetEvent.WaitOne();

            sb.AppendLine(str);

            writeResetEvent.Set();
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

                        while (IsPaused)
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

            File.AppendAllText($"{fileName}_output.txt", sb.ToString());
            return totalBytesWritten;
        }

        public bool IsPaused { get; set; }

        public event EventHandler<DownloadProgress> BytesDownloadedChanged;
        public event EventHandler<DownloadSpeed> DownloadingSpeedChanged;
    }
}