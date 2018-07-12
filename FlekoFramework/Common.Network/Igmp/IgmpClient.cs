using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Flekosoft.Common.Network.Internals;

namespace Flekosoft.Common.Network.Igmp
{
    public class IgmpClient : AsyncNetworkExchangeDriver
    {
        private IgmpNetworkExchangeInterface _exchangeInterface;


        /// <summary>
        /// Start client
        /// </summary>
        /// <param name="multicastAddr"> The multicast System.Net.IPAddress of the group you want to join.</param>
        /// <param name="destinationPort"> The port on which to listen for incoming connection attempts.</param>
        /// <param name="exchangeType"> type if client</param>
        public void Start(IPAddress multicastAddr, int destinationPort, ExchangeType exchangeType)
        {
            if (multicastAddr.AddressFamily != AddressFamily.InterNetwork) throw new ArgumentException("multicastAddr must be ipv4");

            var arr = IPAddress.Parse("224.0.0.0").GetAddressBytes();
            if (BitConverter.IsLittleEndian) Array.Reverse(arr);
            var startAddrRange = BitConverter.ToUInt32(arr, 0);

            arr = IPAddress.Parse("239.255.255.255").GetAddressBytes();
            if (BitConverter.IsLittleEndian) Array.Reverse(arr);
            var endAddrRange = BitConverter.ToUInt32(arr, 0);


            arr = multicastAddr.GetAddressBytes();
            if (BitConverter.IsLittleEndian) Array.Reverse(arr);
            var addr = BitConverter.ToUInt32(arr, 0);


            if (addr < startAddrRange) throw new ArgumentException("multicastAddr must be in diapason of 224.0.0.0 - 239.255.255.255");
            if (addr > endAddrRange) throw new ArgumentException("multicastAddr must be in diapason of 224.0.0.0 - 239.255.255.255");

            MulticastIpAddress = multicastAddr;

            _exchangeInterface?.Dispose();
            UdpClient client;
            switch (exchangeType)
            {
                case ExchangeType.Sender:
                    client = new UdpClient(AddressFamily.InterNetwork);
                    break;
                case ExchangeType.SenderAndReceiver:
                    client = new UdpClient(destinationPort, AddressFamily.InterNetwork);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(exchangeType), exchangeType, null);
            }
            client.JoinMulticastGroup(multicastAddr);
            Thread.Sleep(100);
            _exchangeInterface = new IgmpNetworkExchangeInterface(client, multicastAddr, destinationPort);
            _exchangeInterface.ErrorEvent += _exchangeInterface_ErrorEvent;

            switch (exchangeType)
            {
                case ExchangeType.Sender:
                    StartExchange(_exchangeInterface, false);
                    break;
                case ExchangeType.SenderAndReceiver:
                    StartExchange(_exchangeInterface);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(exchangeType), exchangeType, null);
            }


            IsStarted = true;
        }

        private void _exchangeInterface_ErrorEvent(object sender, System.IO.ErrorEventArgs e)
        {
            OnErrorEvent(e.GetException());
        }

        /// <summary>
        /// Disconnect and stop cilent
        /// </summary>
        public void Stop()
        {
            IsStarted = false;
            MulticastIpAddress = null;

            StopExchange();

            _exchangeInterface?.Dispose();
            _exchangeInterface = null;
        }

        public IPAddress MulticastIpAddress { get; protected set; }

        protected override void ProcessByteInternal(NetworkDataEventArgs e)
        {
            OnDataReceivedEvent(e.Data, e.LocalEndPoint, e.RemoteEndPoint);

        }

        public bool SendData(byte[] data)
        {
            return Write(data);
        }

        public event EventHandler<NetworkDataEventArgs> DataReceivedEvent;
        private void OnDataReceivedEvent(byte[] data, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            DataReceivedEvent?.Invoke(this, new NetworkDataEventArgs(data, localEndPoint, remoteEndPoint));
        }

        #region Disposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DataReceivedEvent = null;
                Stop();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
