using System.Collections.Generic;
using DownloadManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DownloadManager.Controllers
{
    [Route("api/[controller]")]
    public class LinksController : Controller
    {
        private readonly IDownloadManager _downloadManager;
        private readonly ApplicationOptions _options;

        public LinksController(IDownloadManager downloadManager,
            IOptions<ApplicationOptions> options)
        {
            _downloadManager = downloadManager;
            _options = options.Value;
        }

        [HttpPost]
        public DownloadLinkResponse Post([FromBody]DownloadLinkRequest request)
        {
            _options.ThreadsPerDownload = request.Threads ?? _options.DefaultThreadsPerDownload;
            _options.BytesPerSecond = request.Speed ?? _options.DefaultThreasholdPerSecond;


            var fileNames = _downloadManager.DownloadFile(request.Url);

            return new DownloadLinkResponse {FilesNames = fileNames};
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
        public long? Speed { get; set; }
    }
}