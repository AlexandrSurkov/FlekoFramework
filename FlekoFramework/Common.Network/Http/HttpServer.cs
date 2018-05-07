using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Flekosoft.Common.Network.Tcp;

namespace Flekosoft.Common.Network.Http
{
    public class HttpServer : TcpServerBase
    {
        private readonly Dictionary<EndPoint, EndpointDataParser> _endpointDataParsers = new Dictionary<EndPoint, EndpointDataParser>();
        public event EventHandler<HttpRequestArgs> RequestEvent;

        public HttpServer()
        {
            ConnectedEvent += HttpServer_ConnectedEvent;
            DisconnectedEvent += HttpServer_DisconnectedEvent;
        }

        private void HttpServer_DisconnectedEvent(object sender, ConnectionEventArgs e)
        {
            if (_endpointDataParsers.ContainsKey(e.RemoteEndPoint)) _endpointDataParsers.Remove(e.RemoteEndPoint);
        }

        private void HttpServer_ConnectedEvent(object sender, ConnectionEventArgs e)
        {
            if (!_endpointDataParsers.ContainsKey(e.RemoteEndPoint)) _endpointDataParsers.Add(e.RemoteEndPoint, new EndpointDataParser());
        }

        public string CreateOkRespond()
        {
            var respond = "HTTP/1.1 200 OK\r\n";
            respond += "Content-Type: text/plain\r\n";
            respond += "Content-Length: 0\r\n";
            respond += "Connection: close\r\n\r\n";
            return respond;
        }

        public string CreateInternalServerErrorRespond()
        {
            var respond = "HTTP/1.1 500 Internal Server Error\r\n";
            respond += "Content-Type: text/plain\r\n";
            respond += "Content-Length: 0\r\n";
            respond += "Connection: close\r\n\r\n";
            return respond;
        }

        protected override void ProcessDataInternal(NetworkDataEventArgs e)
        {
            if (!_endpointDataParsers.ContainsKey(e.RemoteEndPoint)) _endpointDataParsers.Add(e.RemoteEndPoint, new EndpointDataParser());

            var parser = _endpointDataParsers[e.RemoteEndPoint];

            parser.NetworkReceivedString += Encoding.UTF8.GetString(e.Data);

            if (parser.ContentLen != -1)
            {
                parser.ContentIndex++;
                parser.HttpData.AddRange(e.Data);
            }

            if (parser.NetworkReceivedString.Contains("\r\n"))
            {
                parser.HttpRequest.Add(parser.NetworkReceivedString);
                parser.NetworkReceivedString = string.Empty;
            }
            else if (parser.ContentIndex == parser.ContentLen)
            {
                parser.HttpRequest.Add(Encoding.UTF8.GetString(parser.HttpData.ToArray()).Trim());
                parser.NetworkReceivedString = string.Empty;

                HttpRequestArgs args = new HttpRequestArgs(parser.HttpRequest.ToArray(), HttpRequestMethod.Unknown);
                if (parser.HttpRequest[0].Contains("GET"))
                    args = new HttpRequestArgs(parser.HttpRequest.ToArray(), HttpRequestMethod.Get);
                if (parser.HttpRequest[0].Contains("POST"))
                    args = new HttpRequestArgs(parser.HttpRequest.ToArray(), HttpRequestMethod.Post);
                if (parser.HttpRequest[0].Contains("PUT"))
                    args = new HttpRequestArgs(parser.HttpRequest.ToArray(), HttpRequestMethod.Put);
                if (parser.HttpRequest[0].Contains("DELETE"))
                    args = new HttpRequestArgs(parser.HttpRequest.ToArray(), HttpRequestMethod.Delete);

                RequestEvent?.Invoke(this, args);
                if (!string.IsNullOrEmpty(args.Respond))
                    Write(Encoding.UTF8.GetBytes(args.Respond), e.LocalEndPoint, e.RemoteEndPoint);

                parser.HttpRequest.Clear();
                parser.ContentLen = -1;
                parser.ContentIndex = 0;
                parser.HttpData.Clear();
            }


            if (parser.ContentLen == -1 && parser.HttpRequest.Count > 0 && parser.HttpRequest[parser.HttpRequest.Count - 1] == "\r\n")
            {
                foreach (var reqStr in parser.HttpRequest)
                {
                    if (reqStr.Contains("Content-Length"))
                    {
                        parser.ContentLen = int.Parse(reqStr.Split(' ')[1]);
                        break;
                    }
                }
                if (parser.ContentLen == -1)
                {
                    parser.NetworkReceivedString = string.Empty;

                    HttpRequestArgs args = new HttpRequestArgs(parser.HttpRequest.ToArray(), HttpRequestMethod.Unknown);
                    if (parser.HttpRequest[0].Contains("GET"))
                        args = new HttpRequestArgs(parser.HttpRequest.ToArray(), HttpRequestMethod.Get);
                    if (parser.HttpRequest[0].Contains("POST"))
                        args = new HttpRequestArgs(parser.HttpRequest.ToArray(), HttpRequestMethod.Post);
                    if (parser.HttpRequest[0].Contains("PUT"))
                        args = new HttpRequestArgs(parser.HttpRequest.ToArray(), HttpRequestMethod.Put);
                    if (parser.HttpRequest[0].Contains("DELETE"))
                        args = new HttpRequestArgs(parser.HttpRequest.ToArray(), HttpRequestMethod.Delete);

                    RequestEvent?.Invoke(this, args);
                    if (!string.IsNullOrEmpty(args.Respond))
                        Write(Encoding.UTF8.GetBytes(args.Respond), e.LocalEndPoint, e.RemoteEndPoint);

                    parser.HttpRequest.Clear();
                    parser.ContentLen = -1;
                    parser.ContentIndex = 0;
                    parser.HttpData.Clear();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                RequestEvent = null;
            }
            base.Dispose(disposing);
        }
    }

    class EndpointDataParser
    {
        public string NetworkReceivedString = string.Empty;
        public readonly List<string> HttpRequest = new List<string>();
        public readonly List<byte> HttpData = new List<byte>();
        public int ContentIndex;
        public int ContentLen = -1;
    }
}

//public class HttpServer : PropertyChangedErrorNotifyDisposableBase
//{
//    private HttpListener _listener;

//    private Thread _thread;
//    private string _prefix;
//    private bool _isListening;

//    public event EventHandler<HttpRequestArgs> Request;
//    public event EventHandler OnStart;
//    public event EventHandler OnStop;

//    public HttpServer()
//    {
//        Prefix = string.Empty;
//    }

//    /// <summary>
//    /// Строка идущая перед командой
//    /// </summary>
//    public string Prefix
//    {
//        get { return _prefix; }
//        set
//        {
//            if (_prefix != value)
//            {
//                _prefix = value;
//                OnPropertyChanged(nameof(Prefix));
//            }
//        }
//    }

//    public bool IsListening
//    {
//        get { return _isListening; }
//        private set
//        {
//            if (_isListening != value)
//            {
//                _isListening = value;
//                OnPropertyChanged(nameof(IsListening));
//            }
//        }
//    }

//    public void StartListen(string url, string port)
//    {
//        if (!IsListening)
//        {
//            string prefix = $"http://{url}:{port}/";
//            _listener = new HttpListener();
//            _listener.Prefixes.Clear();
//            _listener.Prefixes.Add(prefix);

//            _thread = new Thread(ThreadProc)
//            {
//                IsBackground = true,
//                InstanceName = "HttpClientListener"
//            };
//            _thread.Start();

//            IsListening = true;
//            OnStart?.Invoke(this, EventArgs.Empty);
//        }
//    }

//    private void ThreadProc()
//    {

//        try
//        {
//            _listener.Start();
//            while (IsListening)
//            {
//                Thread.Sleep(1);
//                //Ожидание входящего запроса
//                HttpListenerContext context = _listener.GetContext();

//                //Объект запроса
//                HttpListenerRequest request = context.Request;

//                //Объект ответа
//                HttpListenerResponse response = context.Response;
//                HttpRequestArgs args = null;

//                if (request.RawUrl.Contains(Prefix))
//                {
//                    if (request.HttpMethod == "GET")
//                        args = new HttpRequestArgs(request.RawUrl, HttpRequestMethod.Get);
//                    if (request.HttpMethod == "POST")
//                        args = new HttpRequestArgs(request.RawUrl, HttpRequestMethod.Post);
//                    if (request.HttpMethod == "PUT")
//                        args = new HttpRequestArgs(request.RawUrl, HttpRequestMethod.Put);
//                    if (request.HttpMethod == "DELETE")
//                        args = new HttpRequestArgs(request.RawUrl, HttpRequestMethod.Delete);

//                    Request?.Invoke(this, args);
//                }

//                string responseString = string.Empty;
//                if (args != null) responseString = args.Respond;
//                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
//                response.ContentLength64 = buffer.Length;
//                var output = response.OutputStream;
//                output.Write(buffer, 0, buffer.Length);
//                output.Close();
//            }

//        }
//        catch (HttpListenerException ex)
//        {
//            IsListening = false;
//            _listener.Stop();
//            _listener.Close();
//            OnStop?.Invoke(this,EventArgs.Empty);

//            if (ex.ErrorCode != 995)
//            {
//                Logger.Instance.AppendException(ex);
//                OnErrorEvent(ex);
//            }
//        }
//        catch (ThreadAbortException)
//        {

//        }
//        catch (Exception ex)
//        {
//            IsListening = false;
//            _listener.Stop();
//            _listener.Close();
//            OnStop?.Invoke(this, EventArgs.Empty);

//            Logger.Instance.AppendException(ex);
//            OnErrorEvent(ex);
//        }
//    }

//    public void StopListen()
//    {
//        if (IsListening)
//        {
//            IsListening = false;
//            _thread.Abort();

//            _listener.Stop();
//            _listener.Close();
//            OnStop?.Invoke(this, EventArgs.Empty);
//        }
//    }

//    #region IDisposable Members

//    protected override void Dispose(bool disposing)
//    {
//        if (disposing)
//        {
//            StopListen();
//            Request = null;
//            OnStart = null;
//            OnStop = null;
//        }
//        base.Dispose(disposing);
//    }
//    #endregion
//}
