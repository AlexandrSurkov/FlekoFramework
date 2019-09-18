using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Flekosoft.Common.Network.Internals;

namespace Flekosoft.Common.Network.Igmp
{
    internal class IgmpNetworkExchangeInterface : PropertyChangedErrorNotifyDisposableBase, INetworkExchangeInterface
    {
        private readonly object _readSyncObject = new object();
        private readonly object _writeSyncObject = new object();
        private bool _isConnected;
        private readonly int _destinationPort;
        private IPAddress _multicastGroupAddress;

        public IgmpNetworkExchangeInterface(UdpClient udpClient, IPAddress multicastGroupAddress, int destinationPort)
        {
            UdpClient = udpClient;

            _destinationPort = destinationPort;
            _multicastGroupAddress = multicastGroupAddress;

            UdpClient.Client.SendBufferSize = 262144;
            UdpClient.Client.ReceiveBufferSize = 262144;
            LocalEndPoint = (IPEndPoint)UdpClient.Client.LocalEndPoint;
            RemoteEndPoint = null;
            IsConnected = true;
        }

        public int Read(byte[] data, int timeout)
        {
            try
            {
                lock (_readSyncObject) //Can't read at the same time from different threads
                {
                    if (UdpClient == null)
                    {
                        throw new NotConnectedException();
                    }

                    UdpClient.Client.ReceiveTimeout = timeout;

                    IPEndPoint rem = new IPEndPoint(IPAddress.Any, 0);
                    var dt = UdpClient.Receive(ref rem);
                    if (dt.Length > data.Length) throw new ArgumentOutOfRangeException($"data buffer is too small to read");
                    RemoteEndPoint = rem;
                    dt.CopyTo(data, 0);
                    return dt.Length;
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
                    case SocketError.Interrupted:
                        IsConnected = false;
                        break;
                    default:
                        OnErrorEvent(sex);
                        break;
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
                    if (UdpClient == null) throw new NotConnectedException();

                    UdpClient.Client.SendTimeout = timeout;

                    var buf = new byte[size];
                    int j = 0;
                    for (int i = offset; i < size + offset; i++, j++)
                    {
                        buf[j] = buffer[i];
                    }

                    var sendedBytes = UdpClient.Send(buf, size, new IPEndPoint(_multicastGroupAddress, _destinationPort));
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
                    case SocketError.Interrupted:
                        IsConnected = false;
                        break;
                    default:
                        OnErrorEvent(sex);
                        break;
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
            get => _isConnected;
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
        public IPEndPoint RemoteEndPoint { get; private set; }
        public UdpClient UdpClient { get; }

        public event EventHandler DisconnectedEvent;
        protected void OnDisconnectedEvent()
        {
            DisconnectedEvent?.Invoke(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UdpClient.Close();

                DisconnectedEvent = null;
            }
            base.Dispose(disposing);
        }
    }
}
