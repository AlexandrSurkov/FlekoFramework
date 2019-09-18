using System;
using System.Net;

namespace Flekosoft.Common.Network.Tcp
{
    public class TcpServer:TcpServerBase
    {
        public event EventHandler<NetworkDataEventArgs> DataReceivedEvent;
        private void OnDataReceivedEvent(byte[] data, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            DataReceivedEvent?.Invoke(this, new NetworkDataEventArgs(data, localEndPoint, remoteEndPoint));
        }

        protected override void ProcessDataInternal(NetworkDataEventArgs e)
        {
            OnDataReceivedEvent(e.Data, e.LocalEndPoint, e.RemoteEndPoint);
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
