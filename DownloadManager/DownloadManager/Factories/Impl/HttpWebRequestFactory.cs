using System;
using SystemInterface.Net;
using SystemWrapper.Net;

namespace DownloadManager.Factories.Impl
{
    public class HttpWebRequestFactory : HttpWebRequestWrapFactory, IHttpWebRequestFactory
    {
        public IHttpWebRequest CreateHeadRequest(string url)
        {
            return CreateHeadRequest(new Uri(url));
        }

        public IHttpWebRequest CreateHeadRequest(Uri uri)
        {
            var httpWebRequest = Create(uri);

            httpWebRequest.Method = "HEAD";

            return httpWebRequest;
        }

        public IHttpWebRequest CreateGetRangeRequest(string url, long bytesStart, long bytesEnd)
        {
            return CreateGetRangeRequest(new Uri(url), bytesStart, bytesEnd);
        }

        public IHttpWebRequest CreateGetRangeRequest(Uri uri, long bytesStart, long bytesEnd)
        {
            var httpWebRequest = Create(uri);

            httpWebRequest.Method = "GET";
            httpWebRequest.AddRange(bytesStart, bytesEnd);

            return httpWebRequest;
        }
    }
}