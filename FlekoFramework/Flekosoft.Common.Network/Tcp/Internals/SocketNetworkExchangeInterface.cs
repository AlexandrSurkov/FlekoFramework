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
        private readonly object _disposeLock = new object();
        private bool _isSocketDisposed = false;

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
                    if (_isSocketDisposed) return 0;

                    if (Socket == null)
                    {
                        throw new NotConnectedException();
                    }

                    if (!Socket.Connected)
                    {
                        throw new NotConnectedException();
                    }

                    var part1 = Socket.Poll(1000, SelectMode.SelectRead);
                    var dataAvailable = Socket.Available;
                    var part2 = (dataAvailable == 0);
                    if ((part1 & part2) || !Socket.Connected)
                    {
                        throw new NotConnectedException();
                    }

                    Socket.ReceiveTimeout = timeout;

                    if (dataAvailable > 0)
                        return Socket.Receive(data);
                    else
                        return 0;
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
                switch (sex.SocketErrorCode)
                {
                    case SocketError.ConnectionAborted:
                    case SocketError.ConnectionReset:
                    case SocketError.ConnectionRefused:
                    case SocketError.Interrupted:
                    case SocketError.TimedOut:
                        IsConnected = false;
                        break;
                    default:
                        OnErrorEvent(sex);
                        break;
                }
            }
            catch (ObjectDisposedException)
            {

            }
            catch (Exception ex)
            {
                var type = ex.GetType();
                if (ex.InnerException != null && ex.InnerException.GetType() != typeof(ThreadAbortException))
                {
                    OnErrorEvent(ex);
                }
            }
            return 0;
        }

        public int Write(byte[] buffer, int offset, int size, int timeout)
        {
            try
            {
                lock (_writeSyncObject) //Can't write at the same time from different threads
                {
                    if (_isSocketDisposed) return 0;
                    if (Socket == null) throw new NotConnectedException();
                    if (!Socket.Connected) throw new NotConnectedException();

                    Socket.SendTimeout = timeout;

                    SocketError err;
                    var sendedBytes = Socket.Send(buffer, offset, size, SocketFlags.None, out err);

                    if (err != SocketError.Success)
                    {
                        Exception ex;
                        switch (err)
                        {
                            case SocketError.ConnectionAborted:
                            case SocketError.ConnectionReset:
                            case SocketError.ConnectionRefused:
                            case SocketError.Interrupted:
                            case SocketError.TimedOut:
                                ex = new NotConnectedException();
                                break;
                            default:
                                ex = new NetworkWriteException(err.ToString());
                                break;
                        }
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
                switch (sex.SocketErrorCode)
                {
                    case SocketError.ConnectionAborted:
                    case SocketError.ConnectionReset:
                    case SocketError.ConnectionRefused:
                    case SocketError.Interrupted:
                    case SocketError.TimedOut:
                        IsConnected = false;
                        break;
                    default:
                        OnErrorEvent(sex);
                        break;
                }
            }
            catch (ObjectDisposedException)
            {

            }
            catch (Exception ex)
            {
                var type = ex.GetType();
                if (ex.InnerException != null && ex.InnerException.GetType() != typeof(ThreadAbortException))
                {
                    OnErrorEvent(ex);
                }
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
                IsConnected = false;
                lock (_disposeLock)
                {
                    if (!_isSocketDisposed)
                    {
                        try
                        {
                            Socket.Shutdown(SocketShutdown.Both);
                            Socket.Close();
                            Socket.Dispose();
                        }
                        catch (ObjectDisposedException)
                        {

                        }
                        _isSocketDisposed = true;
                    }
                }

                DisconnectedEvent = null;
            }
            base.Dispose(disposing);
        }
    }
}
