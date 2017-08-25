using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Flekosoft.Common.Network.Tcp.Internals;

namespace Flekosoft.Common.Network.Tcp
{
    public abstract class TcpClient : AsyncNetworkExchangeDriver
    {
        private bool _isConnected;

        private TcpClientNetworkExchangeInterface _exchangeInterface;

        //private System.Net.Sockets.TcpClient _client;
        //NetworkStream _netStream;

        private int _pingFailCount;

        private readonly Thread _connectThread;

        private int _pollFailLimit;
        private int _pollInterval;
        private int _connectInterval;

        protected TcpClient()
        {
            PollFailLimit = 3;
            PollInterval = 1000;
            ConnectInterval = 1000;
            ReadBufferSize = 1024;

            _connectThread = new Thread(ConnectThreadFunc);
            _connectThread.Start();
        }

        #region Properties

        /// <summary>
        /// Server Ip address
        /// </summary>
        public string IpAddress { get; protected set; }
        /// <summary>
        /// Server port
        /// </summary>
        public int Port { get; protected set; }
        /// <summary>
        /// Is connected to server
        /// </summary>
        public bool IsConnected
        {
            get { return _isConnected; }
            private set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged(nameof(IsConnected));
                }
            }
        }

        /// <summary>
        /// Count of failed polls to disconnect from server
        /// </summary>
        public int PollFailLimit
        {
            get { return _pollFailLimit; }
            set
            {
                if (_pollFailLimit != value)
                {
                    _pollFailLimit = value;
                    OnPropertyChanged(nameof(PollFailLimit));
                }
            }
        }
        /// <summary>
        /// Poll interval in milliseconds
        /// </summary>
        public int PollInterval
        {
            get { return _pollInterval; }
            set
            {
                if (_pollInterval != value)
                {
                    _pollInterval = value;
                    OnPropertyChanged(nameof(PollInterval));
                }
            }
        }
        /// <summary>
        /// Reconnect Interval in milliseconds
        /// </summary>
        public int ConnectInterval
        {
            get { return _connectInterval; }
            set
            {
                if (_connectInterval != value)
                {
                    _connectInterval = value;
                    OnPropertyChanged(nameof(ConnectInterval));
                }
            }
        }
        ///// <summary>
        ///// Socket read buffer size
        ///// </summary>
        //public int ReadBufferSize
        //{
        //    get { return _readBufferSize; }
        //    set
        //    {
        //        if (_readBufferSize != value)
        //        {
        //            _readBufferSize = value;
        //            lock (_readBufferSyncObject)
        //            {
        //                _readBuffer = new byte[_readBufferSize];
        //            }
        //            OnPropertyChanged(nameof(ReadBufferSize));
        //        }
        //    }
        //}

        #endregion

        #region Threads

        private void ConnectThreadFunc()
        {
            while (true)
            {
                try
                {
                    if (!IsStarted)
                    {
                        if (IsConnected)
                        {
                            DisconnectFromServer();
                        }
                        continue;
                    }

                    if (!IsConnected)
                    {
                        try
                        {
                            if (!IsDisposed)
                            {
                                OnReconnectingEvent();
                                if (ConnectToServer())
                                {
                                    _pingFailCount = 0;
                                }
                                else Thread.Sleep(ConnectInterval);
                            }
                        }
                        // ReSharper disable UnusedVariable
                        catch (Exception ex)
                        // ReSharper restore UnusedVariable
                        {
                            OnErrorEvent(ex);
                        }
                    }
                    else
                    {
                        Thread.Sleep(PollInterval);
                        //Poll
                        if (!Poll())
                        {
                            _pingFailCount++;
                            if (_pingFailCount >= PollFailLimit)
                                DisconnectFromServer();
                        }
                        else _pingFailCount = 0;
                    }
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
        }
        #endregion

        #region Methods

        private bool ConnectToServer()
        {
            try
            {
                _exchangeInterface?.Dispose();

                var client = new System.Net.Sockets.TcpClient(IpAddress, Port);
                _exchangeInterface = new TcpClientNetworkExchangeInterface(client);

                IsConnected = true;
                _pingFailCount = 0;
                OnConnectedEvent(_exchangeInterface.LocalEndpoint, _exchangeInterface.RemoteEndpoint);

                StartExchange(_exchangeInterface);
                return true;
            }
            catch (SocketException se)
            {
                OnConnectionFailEvent(se.SocketErrorCode.ToString());
                return false;
            }
            catch (Exception ex)
            {
                OnConnectionFailEvent(ex.Message);
                return false;
            }
        }
        private void DisconnectFromServer()
        {
            StopExchange();

            _exchangeInterface?.Dispose();
            _exchangeInterface = null;

            if (IsConnected)
            {
                IsConnected = false;
                OnDisconnectedEvent();
            }
        }

        /// <summary>
        /// Start client
        /// </summary>
        /// <param name="ipAddress">Remote server ip address</param>
        /// <param name="port"> Remote server port</param>
        public void Start(string ipAddress, int port)
        {
            IpAddress = ipAddress;
            Port = port;
            IsStarted = true;
        }

        /// <summary>
        /// Disconnect and stop cilent
        /// </summary>
        public void Stop()
        {
            IsStarted = false;
            IpAddress = string.Empty;
            Port = 0;
        }

        protected abstract bool Poll();

        #endregion

        #region events
        /// <summary>
        /// Client connected
        /// </summary>
        public event EventHandler<ConnectionEventArgs> ConnectedEvent;
        protected void OnConnectedEvent(EndPoint localEndPoint, EndPoint remoteEndPoint)
        {
            ConnectedEvent?.Invoke(this, new ConnectionEventArgs(localEndPoint, remoteEndPoint));
        }

        public event EventHandler DisconnectedEvent;
        protected void OnDisconnectedEvent()
        {
            DisconnectedEvent?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ReconnectingEvent;
        protected void OnReconnectingEvent()
        {
            ReconnectingEvent?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<ConnectionFailEventArgs> ConnectionFailEvent;
        protected void OnConnectionFailEvent(string result)
        {
            ConnectionFailEvent?.Invoke(this, new ConnectionFailEventArgs(result));
        }

        #endregion

        #region Dispodable
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_connectThread != null)
                {
                    if (_connectThread.IsAlive)
                    {
                        _connectThread.Abort();
                    }
                }

                //if (_readFromStreamThread != null)
                //{
                //    if (_readFromStreamThread.IsAlive)
                //    {
                //        _readFromStreamThread.Abort();
                //    }
                //}

                //if (_processDataThread != null)
                //{
                //    if (_processDataThread.IsAlive)
                //    {
                //        _processDataThread.Abort();
                //    }
                //}

                DisconnectFromServer();

                ConnectedEvent = null;
                ConnectionFailEvent = null;
                DisconnectedEvent = null;
                ReconnectingEvent = null;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
