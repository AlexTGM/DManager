using SystemInterface.IO;
using SystemInterface.Net;
using SystemWrapper.IO;
using SystemWrapper.Net;
using DownloadManager.Services;
using DownloadManager.Services.Impl;
using Microsoft.AspNetCore.Mvc;

namespace DownloadManager.Controllers
{
    [Route("api/[controller]")]
    public class LinksController : Controller
    {
        [HttpGet("{url}")]
        public void Get(string url)
        {
            IFile file = new FileWrap();
            IFileSaver fileSaver = new FileSaver(file);
            IHttpWebRequestFactory httpWebRequestFactory = new HttpWebRequestWrapFactory();
            IFileInformationProvider fileInformationProvider = new FileInformationProvider(httpWebRequestFactory);
            IFileDownloader fileDownloader = new FileDownloader(fileInformationProvider, httpWebRequestFactory, fileSaver);

            fileDownloader.DownloadFile(url);
        }
    }
}