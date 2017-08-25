using System;
using SystemInterface.Net;

namespace DownloadManager.Factories
{
    public interface IHttpWebRequestFactory : SystemInterface.Net.IHttpWebRequestFactory
    {
        IHttpWebRequest CreateHeadRequest(string url);
        IHttpWebRequest CreateHeadRequest(Uri uri);

        IHttpWebRequest CreateGetRangeRequest(string url, long bytesStart, long bytesEnd);
        IHttpWebRequest CreateGetRangeRequest(Uri uri, long bytesStart, long bytesEnd);
    }
}