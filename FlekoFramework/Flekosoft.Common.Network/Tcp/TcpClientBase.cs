using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Flekosoft.Common.Network.Internals;
using Flekosoft.Common.Network.Tcp.Internals;

namespace Flekosoft.Common.Network.Tcp
{
    public abstract class TcpClientBase : AsyncNetworkExchangeDriver
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

        // Define the cancellation token.
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        readonly EventWaitHandle _threadFinishedWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        protected TcpClientBase()
        {
            PollFailLimit = 3;
            PollInterval = 1000;
            ConnectInterval = 1000;
            ReadBufferSize = 1024;

            _connectThread = new Thread(ConnectThreadFunc);
            _connectThread.Start(_cancellationTokenSource.Token);
        }

        #region Properties

        public IPEndPoint DestinationIpEndPoint { get; protected set; }

        ///// <summary>
        ///// Server Ip address
        ///// </summary>
        //public string IpAddress { get; protected set; }
        ///// <summary>
        ///// Server port
        ///// </summary>
        //public int Port { get; protected set; }
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

        private void ConnectThreadFunc(object o)
        {
            var cancellationToken = (CancellationToken)o;

            while (true)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Thread.Sleep(1);
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
                    else
                    {
                        Thread.Sleep(PollInterval);
                        //Poll
                        if (!Poll())
                        {
                            _pingFailCount++;
                            if (_pingFailCount >= PollFailLimit)
                            {
                                if (IsConnected)
                                {
                                    IsConnected = false;
                                    OnDisconnectedEvent();
                                }
                            }
                        }
                        else _pingFailCount = 0;
                    }
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

        private bool ConnectToServer()
        {
            System.Net.Sockets.TcpClient client = null;
            try
            {
                _exchangeInterface?.Dispose();
                client = new System.Net.Sockets.TcpClient(DestinationIpEndPoint.Address.ToString(), DestinationIpEndPoint.Port);
                _exchangeInterface = new TcpClientNetworkExchangeInterface(client);
                _exchangeInterface.DisconnectedEvent += _exchangeInterface_DisconnectedEvent;

                IsConnected = true;
                _pingFailCount = 0;

                StartExchange(_exchangeInterface);

                OnConnectedEvent(_exchangeInterface.LocalEndPoint, _exchangeInterface.RemoteEndPoint);

                
                return true;
            }
            catch (SocketException se)
            {
                client?.Close();
                OnConnectionFailEvent(se.SocketErrorCode.ToString());
                return false;
            }
            catch (ThreadAbortException)
            {
                return false;
            }
            catch (Exception ex)
            {
                client?.Close();
                OnConnectionFailEvent(ex.Message);
                return false;
            }
        }

        private void _exchangeInterface_DisconnectedEvent(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                IsConnected = false;
                OnDisconnectedEvent();
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
        /// <param name="endPoint">Remote server ip endpoint</param>
        public void Start(IPEndPoint endPoint)
        {
            DestinationIpEndPoint = new IPEndPoint(endPoint.Address, endPoint.Port);
            IsStarted = true;
        }

        /// <summary>
        /// Disconnect and stop cilent
        /// </summary>
        public void Stop()
        {
            IsStarted = false;
            DestinationIpEndPoint = null;
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
                        _cancellationTokenSource.Cancel();
                        _threadFinishedWaitHandle.WaitOne(Timeout.Infinite);

                        //_connectThread.Abort();
                    }
                }

                _cancellationTokenSource.Dispose();
                _threadFinishedWaitHandle?.Dispose();

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
