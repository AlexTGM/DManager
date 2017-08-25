namespace DownloadManager.Services
{
    public interface IUrlHelperTools
    {
        string UrlDecode(string url);
        string UrlEncode(string url);
    }
}