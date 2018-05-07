//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Flekosoft.Common.Network.WebSocket
//{
//    class WebSocketClient:Tcp.TcpClientBase
//    {
//        private string _path;
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="path"> Path in GET request (GET /chat GTTP/1.1) where "/chat" is path</param>
//        public WebSocketClient(string path)
//        {
//            _path = path;
//            ConnectedEvent += WebSocketClient_ConnectedEvent;
//        }

//        private void WebSocketClient_ConnectedEvent(object sender, ConnectionEventArgs e)
//        {
//            var retStr = $"GET {_path} HTTP/1.1\r\n";
//            retStr += "Upgrade: websocket\r\n";
//            retStr += "Connection: Upgrade\r\n";
//            retStr += $"Sec-WebSocket-Key: {acceptKey}\r\n";
//            retStr += $"Sec-WebSocket-Version: 13\r\n\r\n";
//            Sec - WebSocket - Version: 13

//            //            GET /chat HTTP/1.1
//            //Host: example.com:8000
//            //Upgrade: websocket
//            //Connection: Upgrade
//            //Sec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==
//            //Sec-WebSocket-Version: 13


//            //var retStr = "HTTP/1.1 101 Switching Protocols\r\n";
//            //retStr += "Upgrade: websocket\r\n";
//            //retStr += "Connection: Upgrade\r\n";
//            //retStr += $"Sec-WebSocket-Accept: {acceptKey}\r\n\r\n";
//        }

//        protected override void ProcessByteInternal(NetworkDataEventArgs e)
//        {
//            throw new NotImplementedException();
//        }

//        protected override bool Poll()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
