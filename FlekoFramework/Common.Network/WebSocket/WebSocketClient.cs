using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Flekosoft.Common.Network.WebSocket
{
    public class WebSocketClient : Tcp.TcpClientBase
    {
        private EndpointDataParser _parser;

        private readonly string _path;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"> Path in GET request (GET /chat GTTP/1.1) where "/chat" is path</param>
        public WebSocketClient(string path)
        {
            PingTimeout = 1000;
            _path = path;
            ConnectedEvent += WebSocketClient_ConnectedEvent;
            DisconnectedEvent += WebSocketClient_DisconnectedEvent;
        }

        public int PingTimeout { get; set; }

        private void WebSocketClient_DisconnectedEvent(object sender, EventArgs e)
        {
            _parser = null;
        }

        private void WebSocketClient_ConnectedEvent(object sender, ConnectionEventArgs e)
        {
            _parser = new EndpointDataParser((IPEndPoint) e.RemoteEndPoint, (IPEndPoint) e.LocalEndPoint);

            var rnd = new Random(DateTime.Now.Millisecond);
            var rndBytes = new byte[16];
            rnd.NextBytes(rndBytes);
            _parser.SentKey = Convert.ToBase64String(rndBytes);

            var retStr = $"GET {_path} HTTP/1.1\r\n";
            retStr += "Upgrade: websocket\r\n";
            retStr += "Connection: Upgrade\r\n";
            retStr += $"Sec-WebSocket-Key: {_parser.SentKey}\r\n";
            retStr += $"Sec-WebSocket-Version: 13\r\n\r\n";

            Write(Encoding.UTF8.GetBytes(retStr));
            _parser.FirstConnected = true;
        }

        public void SendData(WebSocketOpcode opcode, byte[] data, bool isFinalFrame)
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

            Write(buffer.ToArray());
        }


        protected override void ProcessByteInternal(NetworkDataEventArgs e)
        {
            if (_parser.FirstConnected)
            {
                ParseHandshake(e);
            }
            else ParseData(e);
        }

        private void ParseHandshake(NetworkDataEventArgs e)
        {
            foreach (byte b in e.Data)
            {
                _parser.NetworkReceivedString += Encoding.UTF8.GetString(new[] { b });

                if (_parser.NetworkReceivedString.Contains("\r\n"))
                {
                    _parser.HttpRequest.Add(_parser.NetworkReceivedString);
                    _parser.NetworkReceivedString = string.Empty;
                }

                if (_parser.HttpRequest.Count > 0 && _parser.HttpRequest[_parser.HttpRequest.Count - 1] == "\r\n")
                {
                    _parser.NetworkReceivedString = string.Empty;

                    try
                    {
                        bool containsSwitch = false;
                        foreach (string s in _parser.HttpRequest)
                        {
                            if (s == "HTTP/1.1 101 Switching Protocols\r\n")
                            {
                                containsSwitch = true;
                                break;
                            }
                        }

                        bool containsUpgrade = false;
                        foreach (string s in _parser.HttpRequest)
                        {
                            if (s == "Upgrade: websocket\r\n")
                            {
                                containsUpgrade = true;
                                break;
                            }
                        }

                        if (containsSwitch && containsUpgrade)
                        {
                            string key = string.Empty;
                            foreach (string s in _parser.HttpRequest)
                            {
                                if (s.Contains("Sec-WebSocket-Accept"))
                                {
                                    var strings = s.Split(new[] { ": ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    key = strings[1];
                                }
                            }

                            if (key != string.Empty)
                            {
                                var acceptKey = AcceptKeyGenerator.AcceptKey(_parser.SentKey);

                                if (key == acceptKey)
                                {
                                    _parser.FirstConnected = false;
                                    _parser.IsFirstDataByte = true;

                                    HandshakeEvent?.Invoke(this, new ConnectionEventArgs(e.LocalEndPoint, e.RemoteEndPoint));
                                    return;
                                }
                            }
                            OnErrorEvent(new HandshakeException("Wrong Accept"));
                            Stop();
                            return;
                        }

                        OnErrorEvent(new HandshakeException("Respond format error"));
                        Stop();
                    }
                    catch (Exception ex)
                    {
                        OnErrorEvent(new HandshakeException($"Internal error", ex));
                        Stop();
                    }
                }
            }
        }

        private void ParseData( NetworkDataEventArgs e)
        {
            foreach (byte b in e.Data)
            {
                if (_parser.IsFirstDataByte)
                {
                    _parser.DataBuffer.Clear();
                    _parser.Fin = 0;
                    _parser.Opcode = 0;
                    _parser.Mask = 0;
                    _parser.PayloadLen1 = 0;
                    _parser.PayloadLen = 0;
                    _parser.HeaderReceived = false;
                    _parser.FrameReceived = false;
                    _parser.PayloadLenReceived = false;
                    _parser.MaskingKey[0] = 0;
                    _parser.MaskingKey[1] = 0;
                    _parser.MaskingKey[2] = 0;
                    _parser.MaskingKey[3] = 0;
                    _parser.PayloadLenLenght = 0;
                    _parser.MaskingKeyLenght = 0;
                }

                _parser.DataBuffer.AddRange(new[] { b });
                _parser.IsFirstDataByte = false;

                if (_parser.DataBuffer.Count == 2)
                {
                    _parser.Fin = (_parser.DataBuffer[0] & 0x80) >> 7;
                    _parser.Opcode = (_parser.DataBuffer[0] & 0x0F);
                    _parser.Mask = (b & 0x80) >> 7;
                    _parser.PayloadLen1 = (b & 0x7F);
                    if (_parser.PayloadLen1 < 126)
                    {
                        _parser.PayloadLen = _parser.PayloadLen1;
                        _parser.PayloadLenReceived = true;
                        _parser.PayloadLenLenght = 0;
                    }
                }

                if (_parser.PayloadLenReceived == false)
                {
                    if (_parser.PayloadLen1 == 126 && _parser.DataBuffer.Count == 4)
                    {
                        _parser.PayloadLen = (_parser.DataBuffer[2] << 8) + _parser.DataBuffer[3];
                        _parser.PayloadLenReceived = true;
                        _parser.PayloadLenLenght = 2;
                        continue;
                    }
                    if (_parser.PayloadLen1 == 127 && _parser.DataBuffer.Count == 6)
                    {
                        _parser.PayloadLen = (_parser.DataBuffer[2] << 24) + (_parser.DataBuffer[3] << 16) + (_parser.DataBuffer[4] << 8) + (_parser.DataBuffer[5] << 0);
                        _parser.PayloadLenReceived = true;
                        _parser.PayloadLenLenght = 4;
                        continue;
                    }
                }
                if (_parser.PayloadLenReceived && _parser.HeaderReceived == false)
                {
                    if (_parser.Mask == 1)
                    {
                        _parser.MaskingKeyLenght = 4;
                        if (_parser.DataBuffer.Count == 2 + _parser.PayloadLenLenght + 4)
                        {
                            _parser.MaskingKey[0] = _parser.DataBuffer[_parser.DataBuffer.Count - 4];
                            _parser.MaskingKey[1] = _parser.DataBuffer[_parser.DataBuffer.Count - 3];
                            _parser.MaskingKey[2] = _parser.DataBuffer[_parser.DataBuffer.Count - 2];
                            _parser.MaskingKey[3] = _parser.DataBuffer[_parser.DataBuffer.Count - 1];
                            _parser.HeaderReceived = true;
                        }
                    }
                    else
                    {
                        _parser.MaskingKeyLenght = 0;
                        _parser.HeaderReceived = true;
                    }
                }
                if (_parser.HeaderReceived && _parser.DataBuffer.Count == 2 + _parser.PayloadLenLenght + _parser.MaskingKeyLenght + _parser.PayloadLen)
                {
                    if (_parser.Mask == 1)
                    {
                        var j = 0;
                        for (int i = 2 + _parser.PayloadLenLenght +
                                     _parser.MaskingKeyLenght; i < _parser.DataBuffer.Count; i++, j++)
                        {
                            _parser.DataBuffer[i] = (byte)(_parser.DataBuffer[i] ^ _parser.MaskingKey[j % 4]);
                        }
                    }
                    ParseFrame(e);
                    _parser.IsFirstDataByte = true;
                }
            }
        }

        private void ParseFrame(NetworkDataEventArgs e)
        {
            var data = new List<byte>();
            var payloadStartIndex = 2 + _parser.PayloadLenLenght + _parser.MaskingKeyLenght;
            switch ((WebSocketOpcode)_parser.Opcode)
            {
                case WebSocketOpcode.Ping:
                    //Ping frame. Sent Pong Frame.
                    _parser.PollReceived = true;
                    data.AddRange(_parser.DataBuffer);
                    data[0] = (byte)(data[0] & 0xF0);
                    data[0] = (byte)(data[0] | (byte)WebSocketOpcode.Pong);
                    Write(data.ToArray());
                    break;
                case WebSocketOpcode.Pong:
                    _parser.PollReceived = true;
                    break;
                case WebSocketOpcode.ConnectionClose:
                    //Close frame. Sent Close back.
                    var connectionCloseReason = (_parser.DataBuffer[payloadStartIndex] << 8) + ((int)_parser.DataBuffer[payloadStartIndex + 1] << 0);
                    ConnectionCloseEvent?.Invoke(this, new ConnectionCloseEventArgs((ConnectionCloseReason)connectionCloseReason));
                    SendClose((ConnectionCloseReason)connectionCloseReason);
                    break;
                case WebSocketOpcode.BinaryFrame:
                case WebSocketOpcode.TextFrame:
                    DataReceivedEvent?.Invoke(this, new DataReceivedEventArgs(e.LocalEndPoint, e.RemoteEndPoint, (WebSocketOpcode)_parser.Opcode, _parser.DataBuffer.GetRange(payloadStartIndex, _parser.PayloadLen).ToArray()));
                    break;
            }
        }

        private void SendPing()
        {
            List<byte> data = new List<byte>
            {
                0x80 | (byte) WebSocketOpcode.Ping,
                0x00
            };
            Write(data.ToArray());
        }

        private void SendClose(ConnectionCloseReason connectionCloseReason)
        {
            List<byte> data = new List<byte>
            {
                0x80 | (byte) WebSocketOpcode.ConnectionClose,
                0x02,
                (byte) ((int) connectionCloseReason >> 8),
                (byte) connectionCloseReason
            };
            Write(data.ToArray());
        }

        protected override bool Poll()
        {
            if (_parser != null)
            {
                _parser.PollReceived = false;
                SendPing();
                var startTime = DateTime.Now;
                while (_parser.PollReceived == false)
                {
                    var delta = DateTime.Now - startTime;
                    if (delta.TotalMilliseconds > PingTimeout) return false;
                    Thread.Sleep(1);
                }
            }
            return true;
        }

        public event EventHandler<ConnectionEventArgs> HandshakeEvent;
        public event EventHandler<DataReceivedEventArgs> DataReceivedEvent;
        public event EventHandler<ConnectionCloseEventArgs> ConnectionCloseEvent;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                HandshakeEvent = null;
                DataReceivedEvent = null;
                ConnectionCloseEvent = null;
            }
            base.Dispose(disposing);
        }
    }
}
