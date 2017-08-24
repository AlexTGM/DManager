using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DownloadManager.Services.Impl
{
    public class DownloadManager : IDownloadManager
    {
        private readonly IUrlHelperTools _urlHelperTools;
        private readonly IFileInformationProvider _fileInfoProvider;
        private readonly IFileDownloader _fileDownloader;
        private readonly IFileMerger _fileMerger;
        private readonly IFileSaver _fileSaver;

        public List<Task> Tasks { get; } = new List<Task>();

        public DownloadManager(IFileInformationProvider fileInfoProvider,
            IFileDownloader fileDownloader, IFileMerger fileMerger,
            IFileSaver fileSaver, IUrlHelperTools urlHelperTools)
        {
            _fileInfoProvider = fileInfoProvider;
            _fileDownloader = fileDownloader;
            _fileMerger = fileMerger;
            _fileSaver = fileSaver;

            _urlHelperTools = urlHelperTools;
        }

        public async Task DownloadFile(string url, int threads)
        {
            var unescapedUrl = _urlHelperTools.UrlDecode(url);

            var validUrl = _fileInfoProvider.CheckIfUriHasValidFormat(unescapedUrl, out Uri uri);
            if (!validUrl) throw new FormatException("Url has wrong format!");

            var fileInfo = _fileInfoProvider.ObtainInformation(uri);

            var temporaryFilesPaths = Enumerable.Range(0, threads).Select(p => $"{fileInfo.Name}_{p}");
            var bounds = ComputeBoundsForEachTask(fileInfo.ContentLength, threads);

            for (var i = 0; i < threads; i++)
            {
                var index = i;

                Tasks.Add(Task.Run(() =>
                {
                    var bound = bounds.ElementAt(index);
                    var file = temporaryFilesPaths.ElementAt(index);

                    using (var stream = _fileDownloader.GetResponse(uri, bound[0], bound[1]).GetResponseStream())
                        _fileSaver.SaveFile(stream, file);
                }));
            }

            await Task.WhenAll(Tasks);

            _fileMerger.Merge(temporaryFilesPaths, fileInfo.Name);
        }

        private static IEnumerable<long[]> ComputeBoundsForEachTask(long contentLength, int tasksCount)
        {
            var bytesPerTask = contentLength / tasksCount;

            for (var i = 0; i < tasksCount; i++)
            {
                yield return new[]
                    {bytesPerTask * i, bytesPerTask * (i + 1) - (tasksCount == i ? 0 : 1)};
            }
        }
    }
}