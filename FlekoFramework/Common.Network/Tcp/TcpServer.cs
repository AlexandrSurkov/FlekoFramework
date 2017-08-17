using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Threading;
using Flekosoft.Common.Network.Tcp.Internals;

namespace Flekosoft.Common.Network.Tcp
{
    public class TcpServer : PropertyChangedErrorNotifyDisposableBase
    {
        private readonly List<ListenSocket> _listenSockets = new List<ListenSocket>();
        private bool _isStarted;

        private readonly Thread _waitConnectionThread;
        private readonly object _listenSocketsSyncObject = new object();
        private readonly object _connectedSocketsListSyncObject = new object();
        private readonly ObservableCollection<AsyncNetworkExchangeDriver> _connectedSockets = new ObservableCollection<AsyncNetworkExchangeDriver>();


        public TcpServer()
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

        #endregion

        #region Threads

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ConnectRequestsThreadProc(object o)
        {
            while (true)
            {
                try
                {
                    if (IsStarted)
                    {
                        lock (_listenSocketsSyncObject)
                        {
                            foreach (ListenSocket ls in _listenSockets)
                            {
                                try
                                {
                                    Thread.Sleep(100);

                                    if (ls.AcceptBeginned) continue;
                                    ls.Socket.BeginAccept(AcceptCallback, ls);
                                    ls.AcceptBeginned = true;
                                }
                                catch (ThreadAbortException)
                                {
                                    return;
                                }
                            }
                        }

                        var removeList = new List<AsyncNetworkExchangeDriver>();
                        lock (_connectedSocketsListSyncObject)
                        {
                            // ReSharper disable LoopCanBeConvertedToQuery
                            foreach (var connection in _connectedSockets)
                            // ReSharper restore LoopCanBeConvertedToQuery
                            {
                                if (!connection.IsConnected) removeList.Add(connection);
                            }

                            foreach (var connection in removeList)
                            {
                                OnDisconnectedEvent(new ConnectionEventArgs(connection., connection.RemoteEndpoint));
                                connection.Dispose();
                                _connectedSockets.Remove(connection);
                            }
                        }
                    }
                    else
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
                            foreach (AsyncNetworkExchangeDriver cs in _connectedSockets)
                            {
                                cs?.Dispose();
                            }
                            _connectedSockets.Clear();
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
        /// <returns>True - если сервер успешно запущен. Иначе false</returns>
        public bool Start(ICollection<TcpServerLocalEndpoint> endpoints)
        {
            if (IsStarted) return false;

            Endpoints = new List<TcpServerLocalEndpoint>(endpoints).AsReadOnly();

            lock (_listenSocketsSyncObject)
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
                    _listenSockets.Add(new ListenSocket(listenSocket));
                }
            }

            IsStarted = true;

            return true;
        }

        public void Stop()
        {
            IsStarted = false;
        }

        private void AcceptCallback(IAsyncResult ar)
        {

            try
            {
                var listenSocket = (ListenSocket)ar.AsyncState;

                if (listenSocket?.Socket != null)
                {
                    Socket socket = listenSocket.Socket.EndAccept(ar);
                    if (_connectedSockets.Count >= _maxClients)
                    {
                        socket.Close();
                    }
                    else
                    {
                        lock (_connectedSocketsListSyncObject)
                        {
                            try
                            {
                                _connectedSocket = new NetworkExchangeDriver(_cultureInfo);
                                _connectedSocket.DisconnectedEvent += _connectedSocket_DisconnectedEvent1;
                                _connectedSocket.ConnectedEvent += _connectedSocket_ConnectedEvent1;
                                _connectedSockets.Add(_connectedSocket);
                                _connectedSocket.ErrorEvent += _connectedSocket_ErrorEvent;
                                _connectedSocket.ReceivedDataEvent += _connectedSocket_AsyncDataEvent;
                                _connectedSocket.SendedDataEvent += _connectedSocket_SendedDataEvent;
                                _connectedSocket.StartWork(new TcpExchangeInterface(socket));

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

        //public event EventHandler<NetworkDataEventArgs> ReceivedDataEvent;
        //private void OnReceivedDataEvent(NetworkDataEventArgs e)
        //{
        //    ReceivedDataEvent?.Invoke(this, e);
        //}

        //public event EventHandler<NetworkDataEventArgs> SendedDataEvent;
        //private void OnSendedDataEvent(NetworkDataEventArgs e)
        //{
        //    SendedDataEvent?.Invoke(this, e);
        //}
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
                    foreach (AsyncNetworkExchangeDriver cs in _connectedSockets)
                    {
                        cs?.Dispose();
                    }
                    _connectedSockets.Clear();
                }

                StoppedEvent = null;
                StartListeningEvent = null;
                StartedEvent = null;
                DisconnectedEvent = null;
                ConnectedEvent = null;
                //ReceivedDataEvent = null;
                //SendedDataEvent = null;
            }
            base.Dispose(disposing);
        }

        #endregion
    }


}
