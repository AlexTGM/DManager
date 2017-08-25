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
            long totalBytesWritten = 0L, bytesFromPreviousCheckpointDownloaded = 0;

            using (var fileStream = _file.OpenWrite(filePath))
            {
                int bytesRead;
                var downloadStarted = DateTime.Now;
                var checkpointTime = default(DateTime);

                while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);

                    totalBytesWritten += bytesRead;

                    var secondsFromLastCheckpoint = (DateTime.Now - checkpointTime).Seconds;

                    if (checkpointTime == default(DateTime))
                    {
                        checkpointTime = DateTime.Now;
                        bytesFromPreviousCheckpointDownloaded = bytesRead;
                        CurrentDownloadingSpeed = bytesRead * 8e-6;

                    }
                    else if (secondsFromLastCheckpoint < 1)
                    {
                        bytesFromPreviousCheckpointDownloaded += bytesRead;
                    }
                    else
                    {
                        bytesFromPreviousCheckpointDownloaded += bytesRead;
                        CurrentDownloadingSpeed = (double)bytesFromPreviousCheckpointDownloaded / secondsFromLastCheckpoint
                            * 8e-6;
                        bytesFromPreviousCheckpointDownloaded = 0;
                        checkpointTime = DateTime.Now;
                        Debug.WriteLine($"{filePath} Computed speed: {CurrentDownloadingSpeed} mbit");
                    }

                    CurrentBytesDownloadedChanged?.Invoke(this, totalBytesWritten);
                }
                
                var downloadFinished = DateTime.Now;

                Debug.WriteLine($"{filePath} started {downloadStarted.Ticks} finished {downloadFinished}");
            }

            return totalBytesWritten;
        }
    }
}