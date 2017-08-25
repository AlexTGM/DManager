using System;
using System.Diagnostics;
using SystemInterface.IO;
using DownloadManager.Factories;
using DownloadManager.Models;

namespace DownloadManager.Services.Impl
{
    public class FileDownloader : IFileDownloader
    {
        private readonly IFile _file;
        private readonly IHttpWebRequestFactory _httpWebRequestFactory;

        private long _totalBytesWritten;

        public FileDownloader(IFile file, IHttpWebRequestFactory httpWebRequestFactory)
        {
            _file = file;
            _httpWebRequestFactory = httpWebRequestFactory;
        }

        public long DownloadFile(TaskInformation taskInfo)
        {
            var request = _httpWebRequestFactory.CreateGetRangeRequest(taskInfo.Uri, taskInfo.BytesStart, taskInfo.BytesEnd);
            using (var stream = request.GetResponse().GetResponseStream())
                return SaveFile(stream, taskInfo.FileName);
        }

        public event EventHandler<long> CurrentBytesDownloadedChanged;
        public double CurrentDownloadingSpeed { get; set; }

        private long SaveFile(IStream responseStream, string filePath)
        {
            var buffer = new byte[524288];
            long bytesFromPreviousCheckpointDownloaded = 0;

            using (var fileStream = _file.OpenWrite(filePath))
            {
                int bytesRead;
                var downloadStarted = DateTime.Now;
                var checkpointTime = downloadStarted;

                while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);

                    _totalBytesWritten += bytesRead;

                    var millisecondsFromLastCheckpoint = (DateTime.Now - checkpointTime).TotalMilliseconds;

                    bytesFromPreviousCheckpointDownloaded += bytesRead;

                    if (millisecondsFromLastCheckpoint >= 1000)
                    { 
                        CurrentDownloadingSpeed = bytesFromPreviousCheckpointDownloaded * 8e-6 / millisecondsFromLastCheckpoint * 1000;
                        checkpointTime = DateTime.Now;
                        bytesFromPreviousCheckpointDownloaded = 0;
                        //Debug.WriteLine($"{filePath} Computed speed: {CurrentDownloadingSpeed:F} mbit || {current:F} mbit");
                    }

                    CurrentBytesDownloadedChanged?.Invoke(this, _totalBytesWritten);
                }
                
                var downloadFinished = DateTime.Now;

                if ((downloadFinished - downloadStarted).Seconds < 1)
                    CurrentDownloadingSpeed = _totalBytesWritten * 8e-6;

                //Debug.WriteLine($"{filePath} started {downloadStarted.Ticks} finished {downloadFinished}");
            }

            return _totalBytesWritten;
        }
    }
}