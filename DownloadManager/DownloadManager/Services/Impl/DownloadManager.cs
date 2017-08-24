using System;

namespace DownloadManager.Services.Impl
{
    public class DownloadManager : IDownloadManager
    {
        private readonly IUrlHelperTools _urlHelperTools;
        private readonly IFileInformationProvider _fileInfoProvider;
        private readonly IFileDownloader _fileDownloader;
        private readonly IFileSaver _fileSaver;

        public DownloadManager(IFileInformationProvider fileInfoProvider,
            IFileDownloader fileDownloader, IFileSaver fileSaver,
            IUrlHelperTools urlHelperTools)
        {
            _fileInfoProvider = fileInfoProvider;
            _fileDownloader = fileDownloader;
            _fileSaver = fileSaver;

            _urlHelperTools = urlHelperTools;
        }

        public void DownloadFile(string url)
        {
            var unescapedUrl = _urlHelperTools.UrlDecode(url);

            var validUrl = _fileInfoProvider.CheckIfUriHasValidFormat(unescapedUrl, out Uri uri);
            if (!validUrl) throw new FormatException("Url has wrong format!");

            var fileInfo = _fileInfoProvider.ObtainInformation(uri);

            using (var stream = _fileDownloader.GetResponse(uri).GetResponseStream())
                _fileSaver.SaveFile(stream);
        }
    }
}