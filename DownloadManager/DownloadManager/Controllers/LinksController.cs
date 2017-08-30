using DownloadManager.Services;
using DownloadManager.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DownloadManager.Controllers
{
    [Route("api/[controller]")]
    public class LinksController : Controller
    {
        private readonly IDownloadManager _downloadManager;
        private readonly IUrlHelperTools _urlHelperTools;
        private readonly ApplicationOptions _options;

        public LinksController(IDownloadManager downloadManager, 
            IUrlHelperTools urlHelperTools, IOptions<ApplicationOptions> options)
        {
            _downloadManager = downloadManager;
            _urlHelperTools = urlHelperTools;
            _options = options.Value;
        }

        [HttpGet("{url}")]
        public void Get(string url)
        {
            var threadsPerDownload = _options.DefaultThreadsPerDownload;

            _downloadManager.DownloadFile(_urlHelperTools.UrlDecode(url), threadsPerDownload);
        }
    }
}