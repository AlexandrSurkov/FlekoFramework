﻿using System;
using System.Net;

namespace Flekosoft.Common.Network.Tcp
{
    public class TcpClient : TcpClientBase
    {
        protected override bool Poll()
        {
            return Ping.Send(DestinationIpEndPoint.Address.ToString());
        }

        protected override void ProcessByteInternal(NetworkDataEventArgs e)
        {
            OnDataReceivedEvent(e.Data, e.LocalEndPoint, e.RemoteEndPoint);

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
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
