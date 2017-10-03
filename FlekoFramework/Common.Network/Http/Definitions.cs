using System;

namespace Flekosoft.Common.Network.Http
{
    public enum HttpRequestMethod
    {
        Unknown,
        Get,
        Post,
        Put,
        Delete
    }
    public class HttpRequestArgs : EventArgs
    {
        public HttpRequestArgs(string[] request, HttpRequestMethod method)
        {
            Request = request;
            Method = method;
            Respond = string.Empty;
        }

        public string[] Request { get; }
        public HttpRequestMethod Method { get; }
        public string Respond { get; set; }
    }
}
