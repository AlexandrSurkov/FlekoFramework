using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using Flekosoft.Common.Logging;
using Flekosoft.Common.Network.Tcp;

namespace Flekosoft.Common.Network.WebSocket
{
    public class WebSocketServer : TcpServerBase
    {
        private readonly Thread _pollCheckThread;

        private int _pollFailLimit;
        private int _pollInterval;

        private readonly Dictionary<EndPoint, EndpointDataParser> _endpointDataParsers = new Dictionary<EndPoint, EndpointDataParser>();

        private readonly object _lockObject = new object();

        public WebSocketServer()
        {
            PollFailLimit = 3;
            PollIntervalMs = 3000;

            _pollCheckThread = new Thread(PollCheckThreadFunc);
            _pollCheckThread.Start();

            ConnectedEvent += WebSocketServer_ConnectedEvent;
            DisconnectedEvent += WebSocketServer_DisconnectedEvent;
        }

        #region Thread
        private void PollCheckThreadFunc()
        {
            // var removeList = new List<EndpointDataParser>();
            while (true)
            {
                try
                {
                    Thread.Sleep(PollIntervalMs);
                    lock (_lockObject)
                    {
                        foreach (EndpointDataParser client in _endpointDataParsers.Values)
                        {
                            if (client.FirstConnected == false)
                            {
                                if (client.PollReceived) client.PollFailCount = 0;
                                else
                                {
                                    client.PollFailCount++;
                                    if (client.PollFailCount >= PollFailLimit)
                                    {
                                        //Disconnect client
                                        DisconnectClient(client.LocalEndPoint, client.RemoteEndPoint);

                                        //if (!DisconnectClient(client.LocalEndPoint, client.RemoteEndPoint))
                                        //{
                                        //    removeList.Add(client);
                                        //}
                                    }
                                }

                                //foreach (EndpointDataParser dataParser in removeList)
                                //{
                                //    if (_endpointDataParsers.ContainsKey(dataParser.RemoteEndPoint)) _endpointDataParsers.Remove(dataParser.RemoteEndPoint);
                                //}

                                client.PollReceived = false;
                            }
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnErrorEvent(ex);
                }
            }
        }
        #endregion

        /// <summary>
        /// Count of failed polls to disconnect from server
        /// </summary>
        public int PollFailLimit
        {
            get => _pollFailLimit;
            set
            {
                if (_pollFailLimit != value)
                {
                    _pollFailLimit = value;
                    OnPropertyChanged(nameof(PollFailLimit));
                }
            }
        }
        /// <summary>
        /// Poll check interval in milliseconds
        /// </summary>
        public int PollIntervalMs
        {
            get => _pollInterval;
            set
            {
                if (_pollInterval != value)
                {
                    _pollInterval = value;
                    OnPropertyChanged(nameof(PollIntervalMs));
                }
            }
        }

        private void WebSocketServer_DisconnectedEvent(object sender, ConnectionEventArgs e)
        {
            lock (_lockObject)
            {
                if (_endpointDataParsers.ContainsKey(e.RemoteEndPoint)) _endpointDataParsers.Remove(e.RemoteEndPoint);
            }
        }

        private void WebSocketServer_ConnectedEvent(object sender, ConnectionEventArgs e)
        {
            lock (_lockObject)
            {
                if (!_endpointDataParsers.ContainsKey(e.RemoteEndPoint))
                {
                    _endpointDataParsers.Add(e.RemoteEndPoint, new EndpointDataParser((IPEndPoint)e.RemoteEndPoint, (IPEndPoint)e.LocalEndPoint));
                }
                else
                {
                    _endpointDataParsers.Remove(e.RemoteEndPoint);
                    _endpointDataParsers.Add(e.RemoteEndPoint, new EndpointDataParser((IPEndPoint)e.RemoteEndPoint, (IPEndPoint)e.LocalEndPoint));
                }
            }
        }

        private void ParseHandshake(EndpointDataParser parser, NetworkDataEventArgs e)
        {
            foreach (byte b in e.Data)
            {
                parser.NetworkReceivedString += Encoding.UTF8.GetString(new[] { b });

                if (parser.NetworkReceivedString.Contains("\r\n"))
                {
                    parser.HttpRequest.Add(parser.NetworkReceivedString);
                    parser.NetworkReceivedString = string.Empty;
                }

                if (parser.HttpRequest.Count > 0 && parser.HttpRequest[parser.HttpRequest.Count - 1] == "\r\n")
                {
                    parser.NetworkReceivedString = string.Empty;

                    try
                    {
                        bool containsUpgrade = false;
                        foreach (string s in parser.HttpRequest)
                        {
                            if (s == "Upgrade: websocket\r\n")
                            {
                                containsUpgrade = true;
                                break;
                            }
                        }

                        if (containsUpgrade)
                        {
                            string key = string.Empty;
                            foreach (string s in parser.HttpRequest)
                            {
                                if (s.Contains("Sec-WebSocket-Key"))
                                {
                                    var strings = s.Split(new[] { ": ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    key = strings[1];
                                }
                            }

                            if (key != string.Empty)
                            {
                                var acceptKey = AcceptKeyGenerator.AcceptKey(key);

                                var retStr = "HTTP/1.1 101 Switching Protocols\r\n";
                                retStr += "Upgrade: websocket\r\n";
                                retStr += "Connection: Upgrade\r\n";
                                retStr += $"Sec-WebSocket-Accept: {acceptKey}\r\n\r\n";
                                Write(Encoding.UTF8.GetBytes(retStr), e.LocalEndPoint, e.RemoteEndPoint);
                                parser.FirstConnected = false;
                                parser.IsFirstDataByte = true;

                                HandshakeEvent?.Invoke(this, new ConnectionEventArgs(e.LocalEndPoint, e.RemoteEndPoint));
                                return;
                            }
                        }

                        var errorStr = "HTTP/1.1 400 Bad Request\r\n";
                        errorStr += "Content-Type: text/plain\r\n";
                        errorStr += "Content-Length: 0\r\n";
                        errorStr += "Connection: close\r\n\r\n";

                        Write(Encoding.UTF8.GetBytes(errorStr), e.LocalEndPoint, e.RemoteEndPoint);

                    }
                    catch
                    {
                        var errorStr = "HTTP/1.1 400 Bad Request\r\n";
                        errorStr += "Content-Type: text/plain\r\n";
                        errorStr += "Content-Length: 0\r\n";
                        errorStr += "Connection: close\r\n\r\n";

                        Write(Encoding.UTF8.GetBytes(errorStr), e.LocalEndPoint, e.RemoteEndPoint);
                    }
                }
            }
        }

        private void ParseData(EndpointDataParser parser, NetworkDataEventArgs e)
        {
            foreach (byte b in e.Data)
            {
                if (parser.IsFirstDataByte)
                {
                    parser.DataBuffer.Clear();
                    parser.Fin = 0;
                    parser.Opcode = 0;
                    parser.Mask = 0;
                    parser.PayloadLen1 = 0;
                    parser.PayloadLen = 0;
                    parser.HeaderReceived = false;
                    parser.FrameReceived = false;
                    parser.PayloadLenReceived = false;
                    parser.MaskingKey[0] = 0;
                    parser.MaskingKey[1] = 0;
                    parser.MaskingKey[2] = 0;
                    parser.MaskingKey[3] = 0;
                    parser.PayloadLenLenght = 0;
                    parser.MaskingKeyLenght = 0;
                }

                parser.DataBuffer.AddRange(new[] { b });
                parser.IsFirstDataByte = false;

                if (parser.DataBuffer.Count == 2)
                {
                    parser.Fin = (parser.DataBuffer[0] & 0x80) >> 7;
                    parser.Opcode = (parser.DataBuffer[0] & 0x0F);
                    parser.Mask = (b & 0x80) >> 7;
                    parser.PayloadLen1 = (b & 0x7F);
                    if (parser.PayloadLen1 < 126)
                    {
                        parser.PayloadLen = parser.PayloadLen1;
                        parser.PayloadLenReceived = true;
                        parser.PayloadLenLenght = 0;
                    }
                }

                if (parser.PayloadLenReceived == false)
                {
                    if (parser.PayloadLen1 == 126 && parser.DataBuffer.Count == 4)
                    {
                        parser.PayloadLen = (parser.DataBuffer[2] << 8) + parser.DataBuffer[3];
                        parser.PayloadLenReceived = true;
                        parser.PayloadLenLenght = 2;
                        continue;
                    }
                    if (parser.PayloadLen1 == 127 && parser.DataBuffer.Count == 6)
                    {
                        parser.PayloadLen = (parser.DataBuffer[2] << 24) + (parser.DataBuffer[3] << 16) + (parser.DataBuffer[4] << 8) + (parser.DataBuffer[5] << 0);
                        parser.PayloadLenReceived = true;
                        parser.PayloadLenLenght = 4;
                        continue;
                    }
                }
                if (parser.PayloadLenReceived && parser.HeaderReceived == false)
                {
                    if (parser.Mask == 1)
                    {
                        parser.MaskingKeyLenght = 4;
                        if (parser.DataBuffer.Count == 2 + parser.PayloadLenLenght + 4)
                        {
                            parser.MaskingKey[0] = parser.DataBuffer[parser.DataBuffer.Count - 4];
                            parser.MaskingKey[1] = parser.DataBuffer[parser.DataBuffer.Count - 3];
                            parser.MaskingKey[2] = parser.DataBuffer[parser.DataBuffer.Count - 2];
                            parser.MaskingKey[3] = parser.DataBuffer[parser.DataBuffer.Count - 1];
                            parser.HeaderReceived = true;
                        }
                    }
                    else
                    {
                        parser.MaskingKeyLenght = 0;
                        parser.HeaderReceived = true;
                    }
                }
                if (parser.HeaderReceived && parser.DataBuffer.Count == 2 + parser.PayloadLenLenght + parser.MaskingKeyLenght + parser.PayloadLen)
                {
                    if (parser.Mask == 1)
                    {
                        var j = 0;
                        for (int i = 2 + parser.PayloadLenLenght +
                                     parser.MaskingKeyLenght; i < parser.DataBuffer.Count; i++, j++)
                        {
                            parser.DataBuffer[i] = (byte)(parser.DataBuffer[i] ^ parser.MaskingKey[j % 4]);
                        }
                    }
                    ParseFrame(parser, e);
                    parser.IsFirstDataByte = true;
                }
            }
        }

        private void ParseFrame(EndpointDataParser parser, NetworkDataEventArgs e)
        {
            //AppendDebugLogMessage($"{this}: {e.RemoteEndPoint} New Frame Received");
            var startDateTime = DateTime.Now;
            var data = new List<byte>();
            var payloadStartIndex = 2 + parser.PayloadLenLenght + parser.MaskingKeyLenght;
            switch ((WebSocketOpcode)parser.Opcode)
            {
                case WebSocketOpcode.Ping:
                    //Ping frame. Sent Pong Frame.
                    parser.PollReceived = true;
                    data.AddRange(parser.DataBuffer);
                    data[0] = (byte)(data[0] & 0xF0);
                    data[0] = (byte)(data[0] | (byte)WebSocketOpcode.Pong);
                    Write(data.ToArray(), e.LocalEndPoint, e.RemoteEndPoint);
                    break;
                case WebSocketOpcode.ConnectionClose:
                    //Close frame. Sent Close back.
                    var connectionCloseReason = (parser.DataBuffer[payloadStartIndex] << 8) + ((int)parser.DataBuffer[payloadStartIndex + 1] << 0);
                    ConnectionCloseEvent?.Invoke(this, new ConnectionCloseEventArgs((ConnectionCloseReason)connectionCloseReason));
                    SendClose((ConnectionCloseReason)connectionCloseReason, e.LocalEndPoint, e.RemoteEndPoint);
                    break;
                case WebSocketOpcode.BinaryFrame:
                case WebSocketOpcode.TextFrame:
                    DataReceivedEvent?.Invoke(this, new DataReceivedEventArgs(e.LocalEndPoint, e.RemoteEndPoint, (WebSocketOpcode)parser.Opcode, parser.DataBuffer.GetRange(payloadStartIndex, parser.PayloadLen).ToArray()));
                    break;
            }
            var endDateTime = DateTime.Now;
            var delta = endDateTime - startDateTime;
            //AppendDebugLogMessage($"{this}: {e.RemoteEndPoint} Frame {(WebSocketOpcode)parser.Opcode} Parsed. Time spent {delta}");
        }

        private void SendClose(ConnectionCloseReason connectionCloseReason, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            List<byte> data = new List<byte>();
            data.Add(0x80 | (byte)WebSocketOpcode.ConnectionClose);
            data.Add(0x02);
            data.Add((byte)((int)connectionCloseReason >> 8));
            data.Add((byte)connectionCloseReason);
            Write(data.ToArray(), localEndPoint, remoteEndPoint);
        }

        protected override void ProcessDataInternal(NetworkDataEventArgs e)
        {
            EndpointDataParser parser = null;
            lock (_lockObject)
            {
                if (!_endpointDataParsers.ContainsKey(e.RemoteEndPoint)) return;
                parser = _endpointDataParsers[e.RemoteEndPoint];
            }

            if (parser.FirstConnected)
            {
                ParseHandshake(parser, e);
            }
            else ParseData(parser, e);

        }

        public void SendData(WebSocketOpcode opcode, byte[] data, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, bool isFinalFrame)
        {
            var buffer = new List<byte>();
            long len = data.LongLength;
            var b1 = (byte)(0x7F & (byte)opcode);
            if (isFinalFrame) b1 = (byte)(b1 | 0x80);
            buffer.Add(b1);
            if (len < 126)
            {
                var b2 = (byte)(0x7F & data.Length);
                buffer.Add(b2);
            }
            else if (len < 0xFFFF)
            {
                buffer.Add(0x7E);
                buffer.Add((byte)((len & 0xFF00) >> 8));
                buffer.Add((byte)((len & 0x00FF) >> 0));
            }
            else if (len < 0x7FFFFFFFFFFFFFFF)
            {
                buffer.Add(0x7F);
                buffer.Add((byte)((len & 0x7F00000000000000) >> 7 * 8));
                buffer.Add((byte)((len & 0x00FF000000000000) >> 6 * 8));
                buffer.Add((byte)((len & 0x0000FF0000000000) >> 5 * 8));
                buffer.Add((byte)((len & 0x000000FF00000000) >> 4 * 8));
                buffer.Add((byte)((len & 0x00000000FF000000) >> 3 * 8));
                buffer.Add((byte)((len & 0x0000000000FF0000) >> 2 * 8));
                buffer.Add((byte)((len & 0x000000000000FF00) >> 1 * 8));
                buffer.Add((byte)((len & 0x00000000000000FF) >> 0 * 8));
            }
            buffer.AddRange(data);

            Write(buffer.ToArray(), localEndPoint, remoteEndPoint);
        }

        public override void Stop()
        {
            var connections = GetConnections();
            foreach (var connection in connections)
            {
                SendClose(ConnectionCloseReason.NormalClose, (IPEndPoint)connection.LocalEndPoint, (IPEndPoint)connection.RemoteEndPoint);
            }
            base.Stop();
        }

        public event EventHandler<ConnectionEventArgs> HandshakeEvent;
        public event EventHandler<DataReceivedEventArgs> DataReceivedEvent;
        public event EventHandler<ConnectionCloseEventArgs> ConnectionCloseEvent;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pollCheckThread.Abort();

                HandshakeEvent = null;
                DataReceivedEvent = null;
                ConnectionCloseEvent = null;
            }
            base.Dispose(disposing);
        }
    }
}
