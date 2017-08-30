using System.Collections.Generic;
using System.Linq;
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

        [HttpPost]
        public DownloadLinkResponse Post([FromBody]DownloadLinkRequest request)
        {
            var threadsPerDownload = request.Threads ?? _options.DefaultThreadsPerDownload;

            _downloadManager.DownloadFile(_urlHelperTools.UrlDecode(request.Url), threadsPerDownload);

            return new DownloadLinkResponse {FilesNames = _downloadManager.Tasks.Select(task => task.FileName)};
        }
    }

    public class DownloadLinkResponse
    {
        public IEnumerable<string> FilesNames;
    }

    public class DownloadLinkRequest
    {
        public string Url { get; set; }
        public int? Threads { get; set; }
    }
}