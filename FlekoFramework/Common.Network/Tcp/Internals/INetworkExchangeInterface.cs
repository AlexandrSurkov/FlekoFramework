using System.Net;

namespace Flekosoft.Common.Network.Tcp.Internals
{
    public interface INetworkExchangeInterface
    {
        int Read(byte[] data);
        int Write(byte[] buffer, int offset, int size);
        bool IsConnected { get; }

        IPEndPoint LocalEndpoint { get; }
        IPEndPoint RemoteEndpoint { get; }
    }
}
