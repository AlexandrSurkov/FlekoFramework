using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Flekosoft.Common.Network.Http;
using Flekosoft.Common.Network.Tcp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Network.Http
{
    [TestClass]
    public class HttpServerTest
    {
        private HttpRequestArgs _args;
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

            _args = null;
            Assert.IsNull(_args);
            HttpClient.SendRequest(HttpRequestMethod.Post, url, data, 1000);
            Assert.IsNotNull(_args);
            Assert.AreEqual($"POST /{data} HTTP/1.1\r\n", _args.Request[0]);
            Assert.AreEqual($"Host: 127.0.0.1:4444\r\n", _args.Request[1]);
            Assert.AreEqual($"Connection: Close\r\n", _args.Request[2]);
            Assert.AreEqual($"\r\n", _args.Request[3]);

            _args = null;
            Assert.IsNull(_args);
            HttpClient.SendRequest(HttpRequestMethod.Get, url, data, 1000);
            Assert.IsNotNull(_args);
            Assert.AreEqual($"GET /{data} HTTP/1.1\r\n", _args.Request[0]);
            Assert.AreEqual($"Host: 127.0.0.1:4444\r\n", _args.Request[1]);
            Assert.AreEqual($"Connection: Close\r\n", _args.Request[2]);
            Assert.AreEqual($"\r\n", _args.Request[3]);

            _args = null;
            Assert.IsNull(_args);
            HttpClient.SendRequest(HttpRequestMethod.Delete, url, data, 1000);
            Assert.IsNotNull(_args);
            Assert.AreEqual($"DELETE /{data} HTTP/1.1\r\n", _args.Request[0]);
            Assert.AreEqual($"Host: 127.0.0.1:4444\r\n", _args.Request[1]);
            Assert.AreEqual($"Connection: Close\r\n", _args.Request[2]);
            Assert.AreEqual($"\r\n", _args.Request[3]);

            _args = null;
            Assert.IsNull(_args);
            HttpClient.SendRequest(HttpRequestMethod.Put, url, data, 100);
            Assert.IsNotNull(_args);
            Assert.AreEqual($"PUT /{data} HTTP/1.1\r\n", _args.Request[0]);
            Assert.AreEqual($"Host: 127.0.0.1:4444\r\n", _args.Request[1]);
            Assert.AreEqual($"Connection: Close\r\n", _args.Request[2]);
            Assert.AreEqual($"\r\n", _args.Request[3]);

            server.Dispose();
        }


        [TestMethod]
        public void MultiClientsTest()
        {

            var clientsCount = 100;

            var server = new HttpServer();
            server.ErrorEvent += Server_ErrorEvent;

            var epList = new List<TcpServerLocalEndpoint>
            {
                new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444), clientsCount)
            };

            server.Start(epList);
            server.RequestEvent += Server_RequestEvent1;

            for (int i = 0; i < clientsCount; i++)
            {
                ThreadPool.QueueUserWorkItem(CallBack, server);
            }

            var startTime = DateTime.Now;
            while (_finishIndex < clientsCount)
            {
                var delta = DateTime.Now - startTime;
                if (delta.TotalMinutes > 5)
                {
                    server.Dispose();
                    Assert.Fail($"_finishIndex {_finishIndex} clientsCount {clientsCount}");
                }
            }

            server.Dispose();
            Assert.AreEqual(clientsCount, _finishIndex);
        }

        private void CallBack(object state)
        {
            try
            {
                SendClientData();
            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }
        }

        private int _finishIndex = 0;
        void SendClientData()
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    string url = "http://127.0.0.1:4444/";
                    HttpClient.SendRequest(HttpRequestMethod.Post, url, "data", 5000);
                }
                catch (Exception e)
                {
                    Assert.Fail(e.ToString());
                }
            }

            _finishIndex++;
        }

        private void Server_RequestEvent(object sender, HttpRequestArgs e)
        {
            _args = e;

            HttpServer srv = (HttpServer)sender;
            e.Respond = srv.CreateOkRespond();
        }

        private void Server_RequestEvent1(object sender, HttpRequestArgs e)
        {
            Assert.AreEqual($"POST /data HTTP/1.1\r\n", e.Request[0]);
            Assert.AreEqual($"Host: 127.0.0.1:4444\r\n", e.Request[1]);
            Assert.AreEqual($"Connection: Close\r\n", e.Request[2]);
            Assert.AreEqual($"\r\n", e.Request[3]);

            HttpServer srv = (HttpServer)sender;
            e.Respond = srv.CreateOkRespond();
        }

        private void Server_ErrorEvent(object sender, System.IO.ErrorEventArgs e)
        {
            Assert.Fail(e.GetException().ToString());
        }
    }
}
