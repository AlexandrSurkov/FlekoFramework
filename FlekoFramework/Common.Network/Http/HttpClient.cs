using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Flekosoft.Common.Network.Http
{
    public static class HttpClient
    {
        public static string SendRequest(HttpRequestMethod type, string url, string data, int timeoutMs)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url + data);
            req.Timeout = timeoutMs;
            req.KeepAlive = false;
            switch (type)
            {
                case HttpRequestMethod.Get:
                    req.Method = "GET";
                    break;
                case HttpRequestMethod.Post:
                    req.Method = "POST";
                    break;
                case HttpRequestMethod.Put:
                    req.Method = "PUT";
                    break;
                case HttpRequestMethod.Delete:
                    req.Method = "DELETE";
                    break;
            }
            //try
            //{
            WebResponse resp = req.GetResponse();
            //if (resp.ContentLength > 0)
            {
                Stream stream = resp.GetResponseStream();
                if (stream != null)
                {
                    StreamReader sr = new StreamReader(stream);
                    string Out = sr.ReadToEnd();
                    sr.Close();
                    return Out;
                }
            }
            return string.Empty;
            //}
            //catch (WebException e)
            //{
            //    if (e.Status == WebExceptionStatus.ProtocolError)
            //    {
            //        Console.WriteLine("Status Code : {0}", ((HttpWebResponse)e.Response).StatusCode);
            //        Console.WriteLine("Status Description : {0}", ((HttpWebResponse)e.Response).StatusDescription);
            //        Log
            //    }
            //    return string.Empty;
            //}
            //catch (Exception ex)
            //{
            //    return string.Empty;
            //}
        }

        public static string SendJson(HttpRequestMethod type, string url, string json, Encoding jsonEncoding, int timeoutMs)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.ContentType = $"application/json; charset={jsonEncoding.BodyName}";
            req.Timeout = timeoutMs;
            switch (type)
            {
                case HttpRequestMethod.Get:
                    req.Method = "GET";
                    break;
                case HttpRequestMethod.Post:
                    req.Method = "POST";
                    break;
                case HttpRequestMethod.Put:
                    req.Method = "PUT";
                    break;
                case HttpRequestMethod.Delete:
                    req.Method = "DELETE";
                    break;
            }

            using (var streamWriter = new StreamWriter(req.GetRequestStream(), jsonEncoding))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            //try
            //{
            WebResponse resp = req.GetResponse();
            if (resp.ContentLength > 0)
            {
                Stream stream = resp.GetResponseStream();
                if (stream != null)
                {
                    StreamReader sr = new StreamReader(stream);
                    string Out = sr.ReadToEnd();
                    sr.Close();
                    return Out;
                }
            }
            return string.Empty;
            //}
            //catch (WebException e)
            //{
            //    if (e.Status == WebExceptionStatus.ProtocolError)
            //    {
            //        Console.WriteLine("Status Code : {0}", ((HttpWebResponse)e.Response).StatusCode);
            //        Console.WriteLine("Status Description : {0}", ((HttpWebResponse)e.Response).StatusDescription);
            //        Log
            //    }
            //    return string.Empty;
            //}
            //catch (Exception ex)
            //{
            //    return string.Empty;
            //}
        }
    }
}
