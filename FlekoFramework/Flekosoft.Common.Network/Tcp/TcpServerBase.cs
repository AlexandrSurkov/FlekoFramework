using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Flekosoft.Common.Network.Internals;
using Flekosoft.Common.Network.Tcp.Internals;

namespace Flekosoft.Common.Network.Tcp
{
    public abstract class TcpServerBase : PropertyChangedErrorNotifyDisposableBase
    {
        //private readonly List<ListenSocket> _listenSockets = new List<ListenSocket>();
        private readonly ConcurrentBag<ListenSocket> _listenSockets = new ConcurrentBag<ListenSocket>();
        private bool _isStarted;
        private bool _connectRequestThreadPaused = true;

        private readonly Thread _waitConnectionThread;
        //private readonly object _listenSocketsSyncObject = new object();
        private bool _dataTrace;

        // Define the cancellation token.
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        readonly EventWaitHandle _threadFinishedWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        //SSL/TLS
        static X509Certificate _serverCertificate = null;
        bool _clientCertificateRequired = false;
        SslProtocols _enabledSslProtocols = SslProtocols.None;
        bool _checkCertificateRevocation = false;
        EncryptionPolicy _encryptionPolicy = EncryptionPolicy.AllowNoEncryption;
        private bool _isEncrypted;


        protected TcpServerBase()
        {
            ValidateClientCertificateEvent += TcpServerBase_ValidateClientCertificate;
            SelectLocalCertificateEvent += TcpServerBase_SelectLocalCertificate;

            _waitConnectionThread = new Thread(ConnectRequestsThreadProc);
            _waitConnectionThread.Start(_cancellationTokenSource.Token);
        }

        

        #region Properties

        /// <summary>
        /// Is server started
        /// </summary>
        public bool IsStarted
        {
            get => _isStarted;
            protected set
            {
                if (_isStarted != value)
                {
                    _isStarted = value;
                    OnPropertyChanged(nameof(IsStarted));
                    if (_isStarted) OnStartedEvent();
                    else OnStoppedEvent();
                }
            }
        }

        /// <summary>
        /// List of all opened Local endpoints
        /// </summary>
        public ReadOnlyCollection<TcpServerLocalEndpoint> Endpoints { get; protected set; }

        /// <summary>
        /// Send trace events on data receive/send
        /// </summary>
        public bool DataTrace
        {
            get { return _dataTrace; }
            set
            {
                if (_dataTrace == value) return;
                _dataTrace = value;
                //lock (_listenSocketsSyncObject)
                {
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var listenSocket in _listenSockets)
                    // ReSharper restore LoopCanBeConvertedToQuery
                    {
                        foreach (var driver in listenSocket.ConnectedSockets.AsReadOnly())
                        {
                            driver.DataTrace = _dataTrace;
                        }
                    }
                }
                OnPropertyChanged(nameof(DataTrace));
            }
        }

        /// <summary>
        /// Is server use SSL/TLS encryption
        /// </summary>
        public bool IsEncrypted
        {
            get { return _isEncrypted; }
            set
            {
                if (_isEncrypted != value)
                {
                    _isEncrypted = value;
                    OnPropertyChanged(nameof(IsEncrypted));
                }
            }
        }

        /// <summary>
        /// Server X509 certificate
        /// </summary>
        public static X509Certificate ServerCertificate => _serverCertificate;

        #endregion

        #region Threads

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ConnectRequestsThreadProc(object o)
        {
            var cancellationToken = (CancellationToken)o;

            while (true)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    //lock (_listenSocketsSyncObject)
                    {
                        if (!_connectRequestThreadPaused)
                        {
                            IsStarted = true;
                            var removeList = new List<SocketAsyncNetworkExchangeDriver>();

                            foreach (ListenSocket ls in _listenSockets)
                            {
                                try
                                {
                                    var cs = ls.ConnectedSockets.AsReadOnly();
                                    foreach (SocketAsyncNetworkExchangeDriver driver in cs)
                                    {
                                        if (!driver.ExchangeInterface.IsConnected) removeList.Add(driver);
                                    }
                                    foreach (var driver in removeList)
                                    {
                                        driver.Dispose();
                                        ls.ConnectedSockets.Remove(driver);
                                        OnDisconnectedEvent(
                                            new ConnectionEventArgs(driver.ExchangeInterface.LocalEndPoint,
                                                driver.ExchangeInterface.RemoteEndPoint));
                                    }

                                    if (ls.AcceptBegin)
                                    {
                                        continue;
                                    }

                                    ls.AcceptBegin = true; //STRONG in this order! First is Accept begin = true. Second is  Begin Accept !!!
                                    ls.Socket.BeginAccept(AcceptCallback, ls);
                                }
                                catch (ThreadAbortException)
                                {
                                    return;
                                }
                            }
                        }
                        else
                        {
                            foreach (ListenSocket ls in _listenSockets)
                            {
                                var cs = ls.ConnectedSockets.AsReadOnly();
                                foreach (SocketAsyncNetworkExchangeDriver driver in cs)
                                {
                                    OnDisconnectedEvent(
                                        new ConnectionEventArgs(driver.ExchangeInterface.LocalEndPoint,
                                            driver.ExchangeInterface.RemoteEndPoint));
                                }
                            }

                            ClearSocketLists();
                            IsStarted = false;
                        }
                    }
                    Thread.Sleep(1);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnErrorEvent(ex);
                }
            }
            _threadFinishedWaitHandle.Set();
        }

        #endregion

        #region Methods        

        /// <summary>
        /// Start server
        /// </summary>
        /// <param name="endpoints">List of local ip endpoints which will used to wait connetion</param>
        /// <returns>True - server successfully started else false</returns>
        public void Start(ICollection<TcpServerLocalEndpoint> endpoints)
        {
            Start(endpoints, null, false, SslProtocols.Default, false, EncryptionPolicy.AllowNoEncryption);
        }

        /// <summary>
        /// Start server
        /// </summary>
        /// <param name="endpoints">List of local ip endpoints which will used to wait connetion</param>
        /// <param name="serverCertificate"> path to the server x509 certificate</param>
        /// <returns>True - server successfully started else false</returns>
        public void Start(ICollection<TcpServerLocalEndpoint> endpoints,
            X509Certificate serverCertificate,
            bool clientCertificateRequired,
            SslProtocols enabledSslProtocols,
            bool checkCertificateRevocation,
            EncryptionPolicy encryptionPolicy)
        {
            if (IsStarted) return;

            if (serverCertificate != null)
            {
                _serverCertificate = serverCertificate;
                _clientCertificateRequired = clientCertificateRequired;
                _enabledSslProtocols = enabledSslProtocols;
                _checkCertificateRevocation = checkCertificateRevocation;
                _encryptionPolicy = encryptionPolicy;
                IsEncrypted = true;
            }
            else
                IsEncrypted = false;

            Endpoints = new List<TcpServerLocalEndpoint>(endpoints).AsReadOnly();

            //lock (_listenSocketsSyncObject)
            {
                try
                {
                    foreach (TcpServerLocalEndpoint address in Endpoints)
                    {
                        if (address.EndPoint.AddressFamily != AddressFamily.InterNetwork) continue;

                        // create the socket
                        var listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        //socket.Blocking = false;

                        listenSocket.Bind(address.EndPoint);

                        // start listening
                        listenSocket.Listen(16);
                        OnStartListeningEvent(new EndPointArgs(address.EndPoint));
                        _listenSockets.Add(new ListenSocket(listenSocket, address));
                        _connectRequestThreadPaused = false;
                    }
                }
                catch (SocketException sex)
                {
                    //Skip ConnectionAborted error
                    if (sex.SocketErrorCode == SocketError.AddressNotAvailable)
                    {
                    }
                    else
                    {
                        OnErrorEvent(sex);
                    }
                }
                catch (Exception ex)
                {
                    OnErrorEvent(ex);
                }
            }

            if (_connectRequestThreadPaused == false)
            {
                while (IsStarted == false)
                {
                    Thread.Sleep(1);
                }
            }
        }

        public virtual void Stop()
        {
            _connectRequestThreadPaused = true;
            _serverCertificate = null;
            _clientCertificateRequired = false;
            _enabledSslProtocols = SslProtocols.None;
            _checkCertificateRevocation = false;
            _encryptionPolicy = EncryptionPolicy.AllowNoEncryption;
            IsEncrypted = false;

            while (IsStarted)
            {
                Thread.Sleep(1);
            }
        }

        public bool Write(byte[] data, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            var driver = FindDriver(localEndPoint, remoteEndPoint);
            if (driver == null) return false;
            return driver.SendData(data);

            //return _listenSockets[0].ConnectedSockets[0].SendData(data);
        }

        public bool DisconnectClient(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            var driver = FindDriver(localEndPoint, remoteEndPoint);
            if (driver == null) return false;
            driver.ExchangeInterface.Dispose();
            return true;
        }

        void ClearSocketLists()
        {
            //lock (_listenSocketsSyncObject)
            {
                //foreach (ListenSocket ls in _listenSockets)
                //{
                //    ls?.Dispose();
                //}
                ListenSocket item;
                while (!_listenSockets.IsEmpty)
                {
                    _listenSockets.TryTake(out item);
                    item.Dispose();
                }

                //_listenSockets.Clear();
            }
        }

        private SocketAsyncNetworkExchangeDriver FindDriver(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            //lock (_listenSocketsSyncObject)
            {
                foreach (ListenSocket listenSocket in _listenSockets)
                {
                    var cs = listenSocket.ConnectedSockets.AsReadOnly();
                    foreach (SocketAsyncNetworkExchangeDriver driver in cs)
                    {
                        if (driver.ExchangeInterface.LocalEndPoint.Equals(localEndPoint) &&
                        driver.ExchangeInterface.RemoteEndPoint.Equals(remoteEndPoint))
                        {
                            return driver;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get all connections
        /// </summary>
        /// <returns>Collection of Local and Remote IpEndPoints pairs</returns>
        public ReadOnlyCollection<Connection> GetConnections()
        {
            var result = new List<Connection>();
            //lock (_listenSocketsSyncObject)
            {
                foreach (ListenSocket listenSocket in _listenSockets)
                {
                    var cs = listenSocket.ConnectedSockets.AsReadOnly();
                    foreach (SocketAsyncNetworkExchangeDriver driver in cs)
                    {
                        result.Add(new Connection(driver.ExchangeInterface.LocalEndPoint, driver.ExchangeInterface.RemoteEndPoint));
                    }
                }
            }
            return result.AsReadOnly();
        }

        protected abstract void ProcessDataInternal(NetworkDataEventArgs e);

        protected virtual X509Certificate SelectLocalCertificate(string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            if (localCertificates.Count > 0) return localCertificates[0];
            else return null;
        }

        protected virtual bool ValidateClientCertificate(X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        #endregion

        #region Event handlers
        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                //lock (_listenSocketsSyncObject)
                {
                    var listenSocket = (ListenSocket)ar.AsyncState;

                    if (listenSocket?.Socket != null)
                    {
                        Socket socket = listenSocket.Socket.EndAccept(ar);
                        if (listenSocket.ConnectedSockets.Count >= listenSocket.TcpServerLocalEndpoint.MaxClients)
                        {
                            socket.Close();
                        }
                        else
                        {
                            SocketAsyncNetworkExchangeDriver driver = null;
                            try
                            {
                                driver = new SocketAsyncNetworkExchangeDriver();
                                driver.ErrorEvent += Driver_ErrorEvent;
                                driver.NewByteEvent += Driver_NewByteEvent;
                                driver.ReceiveDataTraceEvent += Driver_ReceiveDataTraceEvent;
                                driver.SendDataTraceEvent += Driver_SendDataTraceEvent;
                                driver.DataTrace = DataTrace;
                                if (!IsEncrypted)
                                {
                                    driver.StartExchange(new SocketNetworkExchangeInterface(socket));
                                }
                                else
                                {
                                    driver.StartExchange(new EncryptedSocketNetworkExchangeInterface(socket,
                                        ServerCertificate,
                                        _clientCertificateRequired,
                                        _enabledSslProtocols,
                                        _checkCertificateRevocation,
                                        _encryptionPolicy,
                                        ValidateClientCertificateEvent,
                                        SelectLocalCertificateEvent));
                                }
                            }
                            catch (Exception ex)
                            {
                                socket.Close();
                                driver?.Dispose();
                                OnErrorEvent(ex);
                                listenSocket.AcceptBegin = false;
                                return;
                            }

                            listenSocket.ConnectedSockets.Add(driver);

                            OnConnectedEvent(new ConnectionEventArgs(driver.ExchangeInterface.LocalEndPoint,
                                driver.ExchangeInterface.RemoteEndPoint));
                        }
                    }
                    listenSocket.AcceptBegin = false;
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                OnErrorEvent(ex);
            }
        }
        private void Driver_SendDataTraceEvent(object sender, NetworkDataEventArgs e)
        {
            OnSendDataTraceEvent(e.Data, e.LocalEndPoint, e.RemoteEndPoint);
        }
        private void Driver_ReceiveDataTraceEvent(object sender, NetworkDataEventArgs e)
        {
            OnReceiveDataTraceEvent(e.Data, e.LocalEndPoint, e.RemoteEndPoint);
        }
        private void Driver_NewByteEvent(object sender, NetworkDataEventArgs e)
        {
            ProcessDataInternal(e);
        }
        private void Driver_ErrorEvent(object sender, System.IO.ErrorEventArgs e)
        {
            OnErrorEvent(e.GetException());
        }
        private X509Certificate TcpServerBase_SelectLocalCertificate(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return SelectLocalCertificate(targetHost, localCertificates, remoteCertificate, acceptableIssuers);
        }
        private bool TcpServerBase_ValidateClientCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return ValidateClientCertificate(certificate, chain, sslPolicyErrors);
        }
        #endregion

        #region Events
        public event EventHandler StartedEvent;
        private void OnStartedEvent()
        {
            StartedEvent?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler StoppedEvent;
        private void OnStoppedEvent()
        {
            StoppedEvent?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EndPointArgs> StartListeningEvent;
        private void OnStartListeningEvent(EndPointArgs e)
        {
            StartListeningEvent?.Invoke(this, e);
        }

        public event EventHandler<ConnectionEventArgs> DisconnectedEvent;
        private void OnDisconnectedEvent(ConnectionEventArgs e)
        {
            DisconnectedEvent?.Invoke(this, e);
        }


        public event EventHandler<ConnectionEventArgs> ConnectedEvent;
        private void OnConnectedEvent(ConnectionEventArgs e)
        {
            ConnectedEvent?.Invoke(this, e);
        }

        public event EventHandler<NetworkDataEventArgs> ReceiveDataTraceEvent;
        private void OnReceiveDataTraceEvent(byte[] data, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            ReceiveDataTraceEvent?.Invoke(this, new NetworkDataEventArgs(data, localEndPoint, remoteEndPoint));
        }

        public event EventHandler<NetworkDataEventArgs> SendDataTraceEvent;
        private void OnSendDataTraceEvent(byte[] data, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            SendDataTraceEvent?.Invoke(this, new NetworkDataEventArgs(data, localEndPoint, remoteEndPoint));
        }

        private event RemoteCertificateValidationCallback ValidateClientCertificateEvent;
        private event LocalCertificateSelectionCallback SelectLocalCertificateEvent;
        #endregion

        #region Disposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ValidateClientCertificateEvent = null;
                SelectLocalCertificateEvent = null;

                Stop();
                if (_waitConnectionThread != null)
                {
                    if (_waitConnectionThread.IsAlive)
                    {
                        _cancellationTokenSource.Cancel();
                        _threadFinishedWaitHandle.WaitOne(Timeout.Infinite);
                        //_waitConnectionThread.Abort();
                    }
                }

                _cancellationTokenSource.Dispose();
                _threadFinishedWaitHandle?.Dispose();

                ClearSocketLists();

                StoppedEvent = null;
                StartListeningEvent = null;
                StartedEvent = null;
                DisconnectedEvent = null;
                ConnectedEvent = null;
                ReceiveDataTraceEvent = null;
                SendDataTraceEvent = null;
            }
            base.Dispose(disposing);
        }

        #endregion
    }

}
