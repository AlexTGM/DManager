using SystemInterface.IO;
using SystemInterface.Net;
using SystemWrapper.IO;
using SystemWrapper.Net;
using DownloadManager.Factories;
using DownloadManager.Services;
using DownloadManager.Services.Impl;
using Microsoft.AspNetCore.Mvc;

using DManager = DownloadManager.Services.Impl.DownloadManager;

namespace DownloadManager.Controllers
{
    [Route("api/[controller]")]
    public class LinksController : Controller
    {
        [HttpGet("{url}")]
        public void Get(string url)
        {
            IHttpWebRequestFactory httpWebRequestWrapFactory = new HttpWebRequestWrapFactory();
            IFileInformationProvider fileInformationProvider = new FileInformationProvider(httpWebRequestWrapFactory);
            IFile file = new FileWrap();
            IFileSaver fileSaver = new FileSaver(file);
            IFileDownloader fileDownloader = new FileDownloader(httpWebRequestWrapFactory);
            IFileMerger fileMerger = new FileMerger(file, new BinaryReaderFactory(), new BinaryWriterFactory());
            IDownloadManager downloadManager = new DManager(fileInformationProvider, fileDownloader, fileMerger, fileSaver, new UrlHelperTools());

            downloadManager.DownloadFile(url, 2);
        }
    }
}