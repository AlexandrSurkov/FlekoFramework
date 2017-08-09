using System;

namespace Flekosoft.Common.Network.Http
{
    public enum HttpRequestMethod
    {
        Get,
        Post,
        Put,
        Delete
    }
    public class HttpRequestArgs : EventArgs
    {
        public HttpRequestArgs(string rawUrl, HttpRequestMethod method)
        {
            RawUrl = rawUrl;
            Method = method;
            Respond = string.Empty;
        }

        public string RawUrl { get; }
        public HttpRequestMethod Method { get; }
        public string Respond { get; set; }
    }
}
