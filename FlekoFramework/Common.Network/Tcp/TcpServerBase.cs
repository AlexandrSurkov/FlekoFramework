using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Flekosoft.Common.Network.Tcp.Internals;

namespace Flekosoft.Common.Network.Tcp
{
    public abstract class TcpServerBase : PropertyChangedErrorNotifyDisposableBase
    {
        private readonly List<ListenSocket> _listenSockets = new List<ListenSocket>();
        private bool _isStarted;

        private readonly Thread _waitConnectionThread;
        private readonly object _listenSocketsSyncObject = new object();
        private readonly object _connectedSocketsListSyncObject = new object();
        private readonly Dictionary<TcpServerLocalEndpoint, List<SocketAsyncNetworkExchangeDriver>> _connectedSockets = new Dictionary<TcpServerLocalEndpoint, List<SocketAsyncNetworkExchangeDriver>>();
        private bool _dataTrace;


        protected TcpServerBase()
        {
            _waitConnectionThread = new Thread(ConnectRequestsThreadProc);
            _waitConnectionThread.Start();
        }

        #region properties

        /// <summary>
        /// Is server started
        /// </summary>
        public bool IsStarted
        {
            get { return _isStarted; }
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
                lock (_connectedSocketsListSyncObject)
                {
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var connection in _connectedSockets)
                    // ReSharper restore LoopCanBeConvertedToQuery
                    {
                        foreach (var driver in connection.Value)
                        {
                            driver.DataTrace = _dataTrace;
                        }
                    }
                }
                OnPropertyChanged(nameof(DataTrace));
            }
        }

        #endregion

        #region Threads

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ConnectRequestsThreadProc(object o)
        {
            while (true)
            {
                try
                {
                    lock (_listenSocketsSyncObject)
                    {
                        if (IsStarted)
                        {
                            foreach (ListenSocket ls in _listenSockets)
                            {
                                try
                                {
                                    Thread.Sleep(1);

                                    if (ls.AcceptBeginned) continue;
                                    ls.Socket.BeginAccept(AcceptCallback, ls);
                                    ls.AcceptBeginned = true;
                                    if (!_connectedSockets.ContainsKey(ls.TcpServerLocalEndpoint))
                                        _connectedSockets.Add(ls.TcpServerLocalEndpoint, new List<SocketAsyncNetworkExchangeDriver>());
                                }
                                catch (ThreadAbortException)
                                {
                                    return;
                                }
                            }

                            var removeList = new Dictionary<TcpServerLocalEndpoint, SocketAsyncNetworkExchangeDriver>();
                            lock (_connectedSocketsListSyncObject)
                            {
                                // ReSharper disable LoopCanBeConvertedToQuery
                                foreach (var connection in _connectedSockets)
                                // ReSharper restore LoopCanBeConvertedToQuery
                                {
                                    foreach (var driver in connection.Value)
                                    {
                                        if (!driver.ExchangeInterface.IsConnected) removeList.Add(connection.Key, driver);
                                    }
                                }

                                foreach (var connection in removeList)
                                {
                                    connection.Value.Dispose();
                                    _connectedSockets[connection.Key].Remove(connection.Value);
                                    OnDisconnectedEvent(
                                        new ConnectionEventArgs(connection.Value.ExchangeInterface.LocalEndPoint,
                                            connection.Value.ExchangeInterface.RemoteEndPoint));
                                }
                            }
                        }
                        else
                        {
                            ClearSocketLists();
                        }
                        Thread.Sleep(1);
                    }
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    OnErrorEvent(ex);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Start server
        /// </summary>
        /// <param name="endpoints">List of local ip endpoints цhich will used to wait connetion</param>
        /// <returns>True - server succesfully started else false</returns>
        public void Start(ICollection<TcpServerLocalEndpoint> endpoints)
        {
            if (IsStarted) return;

            Endpoints = new List<TcpServerLocalEndpoint>(endpoints).AsReadOnly();

            lock (_listenSocketsSyncObject)
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
                    }
                }
                catch (Exception ex)
                {
                    OnErrorEvent(ex);
                }
                IsStarted = true;
            }
        }

        public void Stop()
        {
            IsStarted = false;
        }

        public bool Write(byte[] data, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            var driver = FindDriver(localEndPoint, remoteEndPoint);
            if (driver == null) return false;
            return driver.SendData(data);
        }

        void ClearSocketLists()
        {
            lock (_listenSocketsSyncObject)
            {
                foreach (ListenSocket ls in _listenSockets)
                {
                    ls?.Dispose();
                }
                _listenSockets.Clear();
            }

            lock (_connectedSocketsListSyncObject)
            {
                foreach (var connection in _connectedSockets)
                // ReSharper restore LoopCanBeConvertedToQuery
                {
                    foreach (var driver in connection.Value)
                    {
                        driver?.Dispose();
                    }
                    connection.Value.Clear();
                }
                _connectedSockets.Clear();
            }
        }

        private SocketAsyncNetworkExchangeDriver FindDriver(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            lock (_connectedSocketsListSyncObject)
            {
                foreach (var connection in _connectedSockets)
                // ReSharper restore LoopCanBeConvertedToQuery
                {
                    foreach (var driver in connection.Value)
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

        protected abstract void ProcessDataInternal(NetworkDataEventArgs e);

        #endregion

        #region event handlers
        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                var listenSocket = (ListenSocket) ar.AsyncState;

                if (listenSocket?.Socket != null)
                {
                    Socket socket = listenSocket.Socket.EndAccept(ar);
                    if (_connectedSockets[listenSocket.TcpServerLocalEndpoint].Count >= listenSocket.TcpServerLocalEndpoint.MaxClients)
                    {
                        socket.Close();
                    }
                    else
                    {
                        lock (_connectedSocketsListSyncObject)
                        {
                            try
                            {
                                var driver = new SocketAsyncNetworkExchangeDriver();
                                driver.ErrorEvent += Driver_ErrorEvent;
                                driver.NewByteEvent += Driver_NewByteEvent;
                                driver.ReceiveDataTraceEvent += Driver_ReceiveDataTraceEvent;
                                driver.SendDataTraceEvent += Driver_SendDataTraceEvent;
                                _connectedSockets[listenSocket.TcpServerLocalEndpoint].Add(driver);
                                driver.StartExchange(new SocketNetworkExchangeInterface(socket));

                                OnConnectedEvent(new ConnectionEventArgs(driver.ExchangeInterface.LocalEndPoint,
                                    driver.ExchangeInterface.RemoteEndPoint));
                            }
                            catch (Exception ex)
                            {
                                OnErrorEvent(ex);
                            }
                        }
                    }

                    listenSocket.AcceptBeginned = false;
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
        #endregion

        #region events
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
        #endregion

        #region Disposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_waitConnectionThread != null)
                {
                    if (_waitConnectionThread.IsAlive)
                    {
                        _waitConnectionThread.Abort();
                    }
                }
                Stop();
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
