using System;
using System.Net;
using System.Net.Sockets;
using Flekosoft.Common.Network.Internals;

namespace Flekosoft.Common.Network.Tcp.Internals
{
    class TcpClientNetworkExchangeInterface : PropertyChangedErrorNotifyDisposableBase, INetworkExchangeInterface
    {

        private readonly object _readSyncObject = new object();
        private readonly object _writeSyncObject = new object();
        private bool _isConnected;
        readonly NetworkStream _netStream;

        public TcpClientNetworkExchangeInterface(System.Net.Sockets.TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            _netStream = TcpClient.GetStream();

            LocalEndPoint = (IPEndPoint)TcpClient.Client.LocalEndPoint;
            RemoteEndPoint = (IPEndPoint)TcpClient.Client.RemoteEndPoint;

            IsConnected = true;
        }

        public int Read(byte[] data, int timeout)
        {
            try
            {
                lock (_readSyncObject) //Can't read at the same time from different threads
                {
                    if (!TcpClient.Client.Connected) throw new NotConnectedException();
                    if (_netStream == null) throw new NotConnectedException();

                    var part1 = TcpClient.Client.Poll(1000, SelectMode.SelectRead);
                    var part2 = (TcpClient.Client.Available == 0);
                    if ((part1 & part2) || !TcpClient.Client.Connected)
                    {
                        throw new NotConnectedException();
                    }

                    if (_netStream.CanRead && _netStream.DataAvailable)
                    {
                        TcpClient.ReceiveTimeout = timeout;
                        return _netStream.Read(data, 0, data.Length);
                    }
                }
            }
            catch (NotConnectedException)
            {
                IsConnected = false;
            }
            catch (Exception ex)
            {
                OnErrorEvent(ex);
            }
            return 0;
        }

        public int Write(byte[] buffer, int offset, int size, int timeout)
        {
            try
            {
                lock (_writeSyncObject) //Can't write at the same time from different threads
                {
                    if (!TcpClient.Client.Connected) throw new NotConnectedException();
                    if (_netStream == null) throw new NotConnectedException();

                    TcpClient.SendTimeout = timeout;

                    SocketError err;
                    var sendedBytes = TcpClient.Client.Send(buffer, offset, size, SocketFlags.None, out err);
                    if (err != SocketError.Success)
                    {
                        Exception ex;
                        if (err == SocketError.ConnectionAborted) ex = new NotConnectedException();
                        else ex = new NetworkWriteException(err.ToString());
                        throw ex;
                    }
                    return sendedBytes;
                }
            }
            catch (NotConnectedException)
            {
                IsConnected = false;
            }
            catch (Exception ex)
            {
                OnErrorEvent(ex);
            }
            return 0;
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            private set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged(nameof(IsConnected));
                    if (!_isConnected) OnDisconnectedEvent();
                }
            }
        }
        public IPEndPoint LocalEndPoint { get; }
        public IPEndPoint RemoteEndPoint { get; }
        public System.Net.Sockets.TcpClient TcpClient { get; }

        public event EventHandler DisconnectedEvent;
        protected void OnDisconnectedEvent()
        {
            DisconnectedEvent?.Invoke(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (TcpClient != null)
                {
                    if (TcpClient.Client.Connected)
                    {
                        TcpClient?.Close();
                    }
                }

                DisconnectedEvent = null;
            }
            base.Dispose(disposing);
        }
    }
}
