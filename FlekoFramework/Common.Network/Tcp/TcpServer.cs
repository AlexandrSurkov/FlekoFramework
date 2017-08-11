using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Flekosoft.Common.Network.Tcp.Internals;

namespace Flekosoft.Common.Network.Tcp
{
    public class TcpServer : PropertyChangedErrorNotifyDisposableBase
    {
        private readonly List<ListenSocket> _listenSockets = new List<ListenSocket>();
        private bool _isStarted;
        
        private Thread _waitConnectionThread;
        private readonly object _listenSocketsSyncObject = new object();
        private readonly object _connectedSocketsListSyncObject = new object();


        public TcpServer()
        {
            _waitConnectionThread = new Thread(ConnectRequestsThreadProc);
            _waitConnectionThread.Start(_listenSockets);
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
            var sockets = o as List<ListenSocket>;
            if (sockets == null) return;
            while (true)
            {
                try
                {
                    foreach (var sl in sockets)
                    {
                        try
                        {
                            lock (_listenSocketsSyncObject)
                            {
                                Thread.Sleep(100);

                                if (sl.AcceptBeginned) continue;
                                sl.Socket.BeginAccept(AcceptCallback, sl);
                                sl.AcceptBeginned = true;
                            }
                        }
                        catch (ThreadAbortException)
                        {
                            return;
                        }
                    }

                    var removeList = new List<NetworkExchangeDriver>();

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
                            OnDisconnectedEvent(new ConnectionEventArgs(connection.RemoteEndpoint));
                            connection.Dispose();
                            _connectedSockets.Remove(connection);
                        }
                    }


                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    var exc = new Exception("ConnectRequestsThreadProc Error", ex);
                    OnErrorEvent(exc);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Start server
        /// </summary>
        /// <param name="endpoints">List to local ip endpoints with will uset to wait connetion</param>
        /// <returns>True - если сервер успешно запущен. Иначе false</returns>
        public bool Start(ICollection<TcpServerLocalEndpoint> endpoints)
        {
            if (IsStarted) return false;

            Endpoints = new List<TcpServerLocalEndpoint>(endpoints).AsReadOnly();

            foreach (TcpServerLocalEndpoint address in endpoints)
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

            IsStarted = true;
            
            return true;
        }

        public void Stop()
        {
            if (_connectWaitThread != null)
            {
                if (_connectWaitThread.IsAlive)
                {
                    _connectWaitThread.Abort();
                    //_asyncEventsThread.Join(1000);
                }
                _connectWaitThread = null;
            }

            foreach (ListenSocket ls in _listenSockets)
            {
                if (ls != null)
                {
                    lock (_listenSocketsSyncObject)
                    {
                        ls.Dispose();
                    }
                }
            }
            _listenSockets.Clear();

            foreach (NetworkExchangeDriver cs in _connectedSockets)
            {
                if (cs != null)
                {
                    lock (_connectedSocketsListSyncObject)
                    {
                        cs.Dispose();
                    }
                }
            }
            _connectedSockets.Clear();

            if (IsStarted)
            {
                IsStarted = false;
                OnPropertyChanged(nameof(IsStarted));
                OnStoppedEvent();
            }
        }

        #endregion





        #region events
        public event EventHandler StartedEvent;
        private void OnStartedEvent()
        {
            StartedEvent?.Invoke(this, System.EventArgs.Empty);
        }

        public event EventHandler StoppedEvent;
        private void OnStoppedEvent()
        {
            StoppedEvent?.Invoke(this, System.EventArgs.Empty);
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

        public event EventHandler<NetworkDataEventArgs> ReceivedDataEvent;
        private void OnReceivedDataEvent(NetworkDataEventArgs e)
        {
            ReceivedDataEvent?.Invoke(this, e);
        }

        public event EventHandler<NetworkDataEventArgs> SendedDataEvent;
        private void OnSendedDataEvent(NetworkDataEventArgs e)
        {
            SendedDataEvent?.Invoke(this, e);
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

                foreach (ListenSocket ls in _listenSockets)
                {
                    if (ls != null)
                    {
                        lock (_listenSocketsSyncObject)
                        {
                            ls.Dispose();
                        }
                    }
                }
                _listenSockets.Clear();

                foreach (var connection in _connectedSockets)
                {
                    connection.Dispose();
                }
                _connectedSockets.Clear();

                if (_listenSocket != null) _listenSocket.Dispose();

                if (_connectedSocket != null) _connectedSocket.Dispose();

                StoppedEvent = null;
                StartListeningEvent = null;
                StartedEvent = null;
                DisconnectedEvent = null;
                ConnectedEvent = null;
                ReceivedDataEvent = null;
                SendedDataEvent = null;
            }
            base.Dispose(disposing);
        }

        #endregion
    }


}
