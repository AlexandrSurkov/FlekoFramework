using System.Net;

namespace Flekosoft.Common.Network.Tcp.Internals
{
    public interface INetworkInterface
    {
        int Read(byte[] data);
        int Write(byte[] data);
        bool IsConnected { get; }

        IPEndPoint LocalEndpoint { get; }
        IPEndPoint RemoteEndpoint { get; }
    }
}
