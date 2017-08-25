using System;

namespace DownloadManager.Services.Impl
{
    public class UrlHelperTools : IUrlHelperTools
    {
        public string UrlDecode(string url)
            => Uri.UnescapeDataString(url);

        public string UrlEncode(string url)
            => Uri.EscapeDataString(url);
    }
}