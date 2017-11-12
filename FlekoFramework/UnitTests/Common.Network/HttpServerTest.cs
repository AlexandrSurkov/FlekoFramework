using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Flekosoft.Common.Network;
using Flekosoft.Common.Network.Http;
using Flekosoft.Common.Network.Tcp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Network
{
    [TestClass]
    public class HttpServerTest
    {
        private HttpRequestArgs args = null;
        [TestMethod]
        public void ReceiveTest()
        {
            var server = new HttpServer();
            server.ErrorEvent += Server_ErrorEvent;


            var epList = new List<TcpServerLocalEndpoint>
            {
                new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444), 1)
            };


            string url = "http://127.0.0.1:4444/";
            string data = "data";

            server.Start(epList);
            server.RequestEvent += Server_RequestEvent;

            args = null;
            Assert.IsNull(args);
            HttpClient.SendRequest(HttpRequestMethod.Post, url, data, 1000);
            Assert.IsNotNull(args);
            Assert.AreEqual($"POST /{data} HTTP/1.1\r\n", args.Request[0]);
            Assert.AreEqual($"Host: 127.0.0.1:4444\r\n", args.Request[1]);
            Assert.AreEqual($"Connection: Close\r\n", args.Request[2]);
            Assert.AreEqual($"\r\n", args.Request[3]);

            args = null;
            Assert.IsNull(args);
            HttpClient.SendRequest(HttpRequestMethod.Get, url, data, 1000);
            Assert.IsNotNull(args);
            Assert.AreEqual($"GET /{data} HTTP/1.1\r\n", args.Request[0]);
            Assert.AreEqual($"Host: 127.0.0.1:4444\r\n", args.Request[1]);
            Assert.AreEqual($"Connection: Close\r\n", args.Request[2]);
            Assert.AreEqual($"\r\n", args.Request[3]);

            args = null;
            Assert.IsNull(args);
            HttpClient.SendRequest(HttpRequestMethod.Delete, url, data, 1000);
            Assert.IsNotNull(args);
            Assert.AreEqual($"DELETE /{data} HTTP/1.1\r\n", args.Request[0]);
            Assert.AreEqual($"Host: 127.0.0.1:4444\r\n", args.Request[1]);
            Assert.AreEqual($"Connection: Close\r\n", args.Request[2]);
            Assert.AreEqual($"\r\n", args.Request[3]);

            args = null;
            Assert.IsNull(args);
            HttpClient.SendRequest(HttpRequestMethod.Put, url, data, 100);
            Assert.IsNotNull(args);
            Assert.AreEqual($"PUT /{data} HTTP/1.1\r\n", args.Request[0]);
            Assert.AreEqual($"Host: 127.0.0.1:4444\r\n", args.Request[1]);
            Assert.AreEqual($"Connection: Close\r\n", args.Request[2]);
            Assert.AreEqual($"\r\n", args.Request[3]);

            server.Dispose();
        }

        private void Server_RequestEvent(object sender, HttpRequestArgs e)
        {
            args = e;
            HttpServer srv = (HttpServer)sender;
            e.Respond = srv.CreateOkRespond();
        }

        private void Server_ErrorEvent(object sender, System.IO.ErrorEventArgs e)
        {
            Assert.Fail(e.GetException().ToString());
        }
    }
}
