using SystemInterface.IO;
using SystemWrapper.IO;
using DownloadManager.Factories;
using DownloadManager.Factories.Impl;
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
            IHttpWebRequestFactory httpWebRequestFactory = new HttpWebRequestFactory();
            IFileInformationProvider fileInfoProvider = new FileInformationProvider(httpWebRequestFactory);
            IFile file = new FileWrap();
            IFileMerger fileMerger = new FileMerger(file, new BinaryReaderFactory(), new BinaryWriterFactory());
            IFileDownloader fileDownloader = new FileDownloader(file, httpWebRequestFactory);
            IDownloadManager downloadManager = new DManager(fileInfoProvider, fileMerger, fileDownloader, 
                new DownloadingTasksFactory(file, httpWebRequestFactory));

            downloadManager.DownloadFile(new UrlHelperTools().UrlDecode(url), 8);
        }
    }
}