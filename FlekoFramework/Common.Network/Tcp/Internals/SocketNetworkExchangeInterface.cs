using System;
using System.Net;
using System.Net.Sockets;

namespace Flekosoft.Common.Network.Tcp.Internals
{
    public class SocketNetworkExchangeInterface : PropertyChangedErrorNotifyDisposableBase, INetworkExchangeInterface
    {
        private readonly object _readSyncObject = new object();
        private readonly object _writeSyncObject = new object();

        public SocketNetworkExchangeInterface(Socket socket)
        {
            Socket = socket;

            Socket.SendTimeout = 1000;
            Socket.ReceiveTimeout = 1000;

            Socket.SendBufferSize = 262144;
            Socket.ReceiveBufferSize = 262144;
        }

        public int Read(byte[] data)
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

                    return Socket.Receive(data);
                }
            }
            catch (NotConnectedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                OnErrorEvent(ex);
            }
            return 0;
        }

        public int Write(byte[] buffer, int offset, int size)
        {
            lock (_writeSyncObject) //Can't write at the same time from different threads
            {
                if (Socket == null) throw new NotConnectedException();
                if (!Socket.Connected) throw new NotConnectedException();

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

        public bool IsConnected
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return Socket.Connected; }
        }
        public IPEndPoint LocalEndpoint
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return (IPEndPoint)Socket.LocalEndPoint; }
        }
        public IPEndPoint RemoteEndpoint
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return (IPEndPoint)Socket.RemoteEndPoint; }
        }
        public Socket Socket { get; }


        //public byte[] Read(int count, int timeoutMs)
        //{
        //    try
        //    {
        //        lock (_readSyncObject) //Can't read at the same time from different threads
        //        {
        //            if (Socket == null)
        //            {
        //                throw new NotConnectedException();
        //            }
        //            if (!Socket.Connected)
        //            {
        //                throw new NotConnectedException();
        //            }

        //            var part1 = Socket.Poll(1000, SelectMode.SelectRead);
        //            var part2 = (Socket.Available == 0);
        //            if ((part1 & part2) || !Socket.Connected)
        //            {
        //                throw new NotConnectedException();
        //            }

        //            Socket.ReceiveTimeout = timeoutMs;
        //            var buf = new byte[count];

        //            var cnt = Socket.Receive(buf);

        //            var result = new byte[cnt];
        //            for (int i = 0; i < result.Length; i++)
        //            {
        //                result[i] = buf[i];
        //            }
        //            return result;
        //        }
        //    }
        //    catch (NotConnectedException)
        //    {
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        OnErrorEvent(ex);
        //    }
        //    return new byte[0];
        //}

        //public int Write(byte[] data)
        //{
        //    lock (_writeSyncObject) //Can't write at the same time from different threads
        //    {
        //        if (Socket == null) throw new NotConnectedException();
        //        if (!Socket.Connected) throw new NotConnectedException();

        //        Socket.SendTimeout = 500;

        //        SocketError err;

        //        var sendedBytes = Socket.Send(data, 0, data.Length, SocketFlags.None, out err);

        //        if (err != SocketError.Success)
        //        {
        //            Exception ex;
        //            if (err == SocketError.ConnectionAborted) ex = new NotConnectedException();
        //            else ex = new NetworkWriteException(err.ToString());
        //            throw ex;
        //        }

        //        if (data.Length != sendedBytes)
        //        {
        //            Exception ex = new NetworkWriteException("Bytes to send " +
        //                                                       data.Length.ToString(CultureInfo.CurrentCulture) + " but sent " +
        //                                                       sendedBytes.ToString(CultureInfo.CurrentCulture) + " bytes ");
        //            throw ex;
        //        }
        //        return sendedBytes;
        //    }
        //}

        //#region IDisposable Members

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        if (Socket != null)
        //        {
        //            if (Socket.Connected)
        //            {
        //                Socket.Shutdown(SocketShutdown.Both);
        //                Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger,
        //                    new LingerOption(false, 0));
        //                Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
        //            }
        //            Socket.Close();
        //        }
        //    }
        //    base.Dispose(disposing);
        //}
        //#endregion
    }
}
