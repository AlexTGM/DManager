using DownloadManager.Services;
using DownloadManager.Tools;
using Microsoft.AspNetCore.Mvc;

namespace DownloadManager.Controllers
{
    [Route("api/[controller]")]
    public class LinksController : Controller
    {
        private readonly IDownloadManager _downloadManager;
        private readonly IUrlHelperTools _urlHelperTools;

        public LinksController(IDownloadManager downloadManager, IUrlHelperTools urlHelperTools)
        {
            _downloadManager = downloadManager;
            _urlHelperTools = urlHelperTools;
        }

        [HttpGet("{url}")]
        public void Get(string url)
        {
            _downloadManager.DownloadFile(_urlHelperTools.UrlDecode(url), 8);
        }
    }
}