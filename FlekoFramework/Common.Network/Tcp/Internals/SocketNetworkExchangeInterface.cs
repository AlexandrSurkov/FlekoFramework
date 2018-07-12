using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Flekosoft.Common.Network.Internals;

namespace Flekosoft.Common.Network.Tcp.Internals
{
    internal class SocketNetworkExchangeInterface : PropertyChangedErrorNotifyDisposableBase, INetworkExchangeInterface
    {
        private readonly object _readSyncObject = new object();
        private readonly object _writeSyncObject = new object();
        private bool _isConnected;

        public SocketNetworkExchangeInterface(Socket socket)
        {
            Socket = socket;

            Socket.SendBufferSize = 262144;
            Socket.ReceiveBufferSize = 262144;
            LocalEndPoint = (IPEndPoint)Socket.LocalEndPoint;
            RemoteEndPoint = (IPEndPoint)Socket.RemoteEndPoint;
            IsConnected = true;
        }

        public int Read(byte[] data, int timeout)
        {
            try
            {
                lock (_readSyncObject) //Can't read at the same time from different threads
                {
                    if (Socket == null)
                    {
                        throw new NotConnectedException();
                    }

                    if (!Socket.Connected)
                    {
                        throw new NotConnectedException();
                    }

                    var part1 = Socket.Poll(1000, SelectMode.SelectRead);
                    var part2 = (Socket.Available == 0);
                    if ((part1 & part2) || !Socket.Connected)
                    {
                        throw new NotConnectedException();
                    }

                    Socket.ReceiveTimeout = timeout;

                    return Socket.Receive(data);
                }
            }
            catch (ThreadAbortException)
            {
               
            }
            catch (NotConnectedException)
            {
                IsConnected = false;
            }
            catch (SocketException sex)
            {
                //Skip ConnectionAborted error
                if (sex.SocketErrorCode == SocketError.ConnectionAborted)
                {
                    IsConnected = false;
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
            return 0;
        }

        public int Write(byte[] buffer, int offset, int size, int timeout)
        {
            try
            {
                lock (_writeSyncObject) //Can't write at the same time from different threads
                {
                    if (Socket == null) throw new NotConnectedException();
                    if (!Socket.Connected) throw new NotConnectedException();

                    Socket.SendTimeout = timeout;

                    SocketError err;
                    var sendedBytes = Socket.Send(buffer, offset, size, SocketFlags.None, out err);

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
            catch (ThreadAbortException)
            {

            }
            catch (SocketException sex)
            {
                //Skip ConnectionAborted error
                if (sex.SocketErrorCode == SocketError.ConnectionAborted)
                {
                    IsConnected = false;
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
        public Socket Socket { get; }

        public event EventHandler DisconnectedEvent;
        protected void OnDisconnectedEvent()
        {
            DisconnectedEvent?.Invoke(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
                Socket.Dispose();

                DisconnectedEvent = null;
            }
            base.Dispose(disposing);
        }
    }
}
