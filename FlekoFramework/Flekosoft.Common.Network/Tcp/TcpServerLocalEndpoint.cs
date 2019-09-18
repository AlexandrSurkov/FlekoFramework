using System.Net;

namespace Flekosoft.Common.Network.Tcp
{
    /// <summary>
    /// Local Endpoint which will used to wait connection
    /// </summary>
    public class TcpServerLocalEndpoint
    {
        public TcpServerLocalEndpoint(IPEndPoint endPoint, int maxClients)
        {
            EndPoint = endPoint;
            MaxClients = maxClients;
        }

        /// <summary>
        /// Local IpEndpoint
        /// </summary>
        public IPEndPoint EndPoint { get; }
        /// <summary>
        /// Maximum count of clients can be connected to the endpoint
        /// </summary>
        public int MaxClients { get; }
    }
}
