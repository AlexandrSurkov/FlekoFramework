using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Flekosoft.Common.Network.Tcp;

namespace Flekosoft.Common.Network.Http
{
    public class HttpServer : TcpServerBase
    {
        private string _networkReceivedString = string.Empty;
        private readonly List<string> _httpRequest = new List<string>();
        private readonly List<byte> _httpData = new List<byte>();
        private int _contentIndex = 0;
        int _contentLen = -1;

        public event EventHandler<HttpRequestArgs> RequestEvent;

        public string CreateOkRespond()
        {
            var respond = "HTTP/1.1 200 OK\r\n";
            respond += "Content-Type: text/plain\r\n";
            respond += "Content-Length: 0\r\n";
            respond += "Connection: close\r\n\r\n";
            return respond;
        }

        protected override void ProcessDataInternal(NetworkDataEventArgs e)
        {
            _networkReceivedString += Encoding.UTF8.GetString(e.Data);

            if (_contentLen != -1)
            {
                _contentIndex++;
                _httpData.AddRange(e.Data);
            }

            if (_networkReceivedString.Contains("\r\n"))
            {
                _httpRequest.Add(_networkReceivedString);
                _networkReceivedString = string.Empty;
            }
            else if (_contentIndex == _contentLen)
            {
                _httpRequest.Add(Encoding.UTF8.GetString(_httpData.ToArray()).Trim());
                _networkReceivedString = string.Empty;

                HttpRequestArgs args = new HttpRequestArgs(_httpRequest.ToArray(), HttpRequestMethod.Unknown);
                if (_httpRequest[0].Contains("GET"))
                    args = new HttpRequestArgs(_httpRequest.ToArray(), HttpRequestMethod.Get);
                if (_httpRequest[0].Contains("POST"))
                    args = new HttpRequestArgs(_httpRequest.ToArray(), HttpRequestMethod.Post);
                if (_httpRequest[0].Contains("PUT"))
                    args = new HttpRequestArgs(_httpRequest.ToArray(), HttpRequestMethod.Put);
                if (_httpRequest[0].Contains("DELETE"))
                    args = new HttpRequestArgs(_httpRequest.ToArray(), HttpRequestMethod.Delete);

                RequestEvent?.Invoke(this, args);
                if (!string.IsNullOrEmpty(args.Respond))
                    Write(Encoding.UTF8.GetBytes(args.Respond), e.LocalEndPoint, e.RemoteEndPoint);

                _httpRequest.Clear();
                _contentLen = -1;
                _contentIndex = 0;
                _httpData.Clear();
            }


            if (_contentLen == -1 && _httpRequest.Count > 0 && _httpRequest[_httpRequest.Count - 1] == "\r\n")
            {
                foreach (var reqStr in _httpRequest)
                {
                    if (reqStr.Contains("Content-Length"))
                    {
                        _contentLen = int.Parse(reqStr.Split(' ')[1]);
                        break;
                    }
                }
                if (_contentLen == -1)
                {
                    _networkReceivedString = string.Empty;

                    HttpRequestArgs args = new HttpRequestArgs(_httpRequest.ToArray(), HttpRequestMethod.Unknown);
                    if (_httpRequest[0].Contains("GET"))
                        args = new HttpRequestArgs(_httpRequest.ToArray(), HttpRequestMethod.Get);
                    if (_httpRequest[0].Contains("POST"))
                        args = new HttpRequestArgs(_httpRequest.ToArray(), HttpRequestMethod.Post);
                    if (_httpRequest[0].Contains("PUT"))
                        args = new HttpRequestArgs(_httpRequest.ToArray(), HttpRequestMethod.Put);
                    if (_httpRequest[0].Contains("DELETE"))
                        args = new HttpRequestArgs(_httpRequest.ToArray(), HttpRequestMethod.Delete);

                    RequestEvent?.Invoke(this, args);
                    if (!string.IsNullOrEmpty(args.Respond))
                        Write(Encoding.UTF8.GetBytes(args.Respond), e.LocalEndPoint, e.RemoteEndPoint);

                    _httpRequest.Clear();
                    _contentLen = -1;
                    _contentIndex = 0;
                    _httpData.Clear();
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
//                Name = "HttpClientListener"
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
