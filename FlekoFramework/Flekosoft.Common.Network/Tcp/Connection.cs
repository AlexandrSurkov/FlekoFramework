using System.Net;

namespace Flekosoft.Common.Network.Tcp
{
    public class Connection
    {
        internal Connection(EndPoint localEndPoint, EndPoint remoteEndPoint)
        {
            LocalEndPoint = localEndPoint;
            RemoteEndPoint = remoteEndPoint;
        }

        public EndPoint LocalEndPoint { get; }
        public EndPoint RemoteEndPoint { get; }
    }
}
