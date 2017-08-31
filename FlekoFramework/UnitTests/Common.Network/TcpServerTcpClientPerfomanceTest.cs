using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Flekosoft.Common.Network;
using Flekosoft.Common.Network.Tcp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Network
{
    [TestClass]
    public class TcpServerTcpClientPerfomanceTest
    {
        readonly object _outputLockObject = new object();
        [TestMethod]
        public void SingleClientPerfomanceTest()
        {
            var server = new TcpServer();
            server.ErrorEvent += Server_ErrorEvent;
            server.DataReceivedEvent += Server_DataReceivedEvent;

            var epList = new List<TcpServerLocalEndpoint>();
            var ipEp1 = (new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6574), 1));

            epList.Add(ipEp1);
            server.Start(epList);

            TestPerfomance(server, 15000);

            server.Dispose();
        }

        void TestPerfomance(TcpServer server, int perfomance)
        {
            var client = new TcpClient();
            client.ErrorEvent += Client_ErrorEvent;
            client.DataReceivedEvent += Client_DataReceivedEvent;
            client.Start(server.Endpoints[0].EndPoint);
            Thread.Sleep(500);
            Assert.IsTrue(client.IsConnected);

            var perfomanceList = new List<int>();
            //Client_ > Server Perfomance Test
            for (int j = 0; j < 10; j++)
            {
                var startTime = DateTime.Now;
                var now = DateTime.Now;
                int value = 1;

                if (ClientDataReceivedEvent.ContainsKey(client.ExchangeInterface.LocalEndPoint.ToString()))
                    ClientDataReceivedEvent[client.ExchangeInterface.LocalEndPoint.ToString()].Clear();
                if (ServerDataReceivedEvent.ContainsKey(client.ExchangeInterface.LocalEndPoint.ToString()))
                    ServerDataReceivedEvent[client.ExchangeInterface.LocalEndPoint.ToString()].Clear();
                var index = 1;

                while ((now - startTime).TotalSeconds < 1)
                {
                    Assert.IsTrue(client.SendData(BitConverter.GetBytes(value)));
                    while (!ServerDataReceivedEvent.ContainsKey(client.ExchangeInterface.LocalEndPoint.ToString())) { Assert.IsTrue(client.IsConnected); }
                    while (ServerDataReceivedEvent[client.ExchangeInterface.LocalEndPoint.ToString()].Count < value * sizeof(int)) { Assert.IsTrue(client.IsConnected); }
                    value++;
                    now = DateTime.Now;
                }

                for (int i = 0; i < value; i += sizeof(int))
                {
                    var val = BitConverter.GetBytes(index);
                    Assert.AreEqual(val[0], ServerDataReceivedEvent[client.ExchangeInterface.LocalEndPoint.ToString()][i + 0].Data[0]);
                    Assert.AreEqual(val[1], ServerDataReceivedEvent[client.ExchangeInterface.LocalEndPoint.ToString()][i + 1].Data[0]);
                    Assert.AreEqual(val[2], ServerDataReceivedEvent[client.ExchangeInterface.LocalEndPoint.ToString()][i + 2].Data[0]);
                    Assert.AreEqual(val[3], ServerDataReceivedEvent[client.ExchangeInterface.LocalEndPoint.ToString()][i + 3].Data[0]);
                    index++;
                }
                perfomanceList.Add(value);
            }

            var perfmance = perfomanceList[0];
            //lock (_outputLockObject)
            //{
            //    System.Diagnostics.Trace.WriteLine(
            //        $"Client {client.ExchangeInterface.LocalEndPoint} -> Server Packets per second {perfomanceList[0]}");
            //}
            for (int i = 1; i < perfomanceList.Count; i++)
            {
                //lock (_outputLockObject)
                //{
                //    System.Diagnostics.Trace.WriteLine(
                //        $"Client {client.ExchangeInterface.LocalEndPoint} -> Server Packets per second {perfomanceList[i]}");
                //}
                perfmance = (perfmance + perfomanceList[i]) / 2;
            }
            lock (_outputLockObject)
            {
                System.Diagnostics.Trace.WriteLine($"Client {client.ExchangeInterface.LocalEndPoint} -> Averange Server Packets per second {perfmance}");
            }
            Assert.IsTrue(perfmance > 500);


            perfomanceList.Clear();
            //Server>Client Perfomance Test
            for (int j = 0; j < 10; j++)
            {
                var startTime = DateTime.Now;
                var now = DateTime.Now;
                int value = 1;
                if (ClientDataReceivedEvent.ContainsKey(client.ExchangeInterface.LocalEndPoint.ToString()))
                    ClientDataReceivedEvent[client.ExchangeInterface.LocalEndPoint.ToString()].Clear();
                if (ServerDataReceivedEvent.ContainsKey(client.ExchangeInterface.LocalEndPoint.ToString()))
                    ServerDataReceivedEvent[client.ExchangeInterface.LocalEndPoint.ToString()].Clear();
                var index = 1;

                while ((now - startTime).TotalSeconds < 1)
                {
                    Assert.IsTrue(server.Write(BitConverter.GetBytes(value), client.ExchangeInterface.RemoteEndPoint, client.ExchangeInterface.LocalEndPoint));
                    while (!ClientDataReceivedEvent.ContainsKey(client.ExchangeInterface.LocalEndPoint.ToString())) { Assert.IsTrue(client.IsConnected); }
                    while (ClientDataReceivedEvent[client.ExchangeInterface.LocalEndPoint.ToString()].Count < value * sizeof(int)) { Assert.IsTrue(client.IsConnected); }
                    value++;
                    now = DateTime.Now;
                }

                for (int i = 0; i < value; i += sizeof(int))
                {
                    var val = BitConverter.GetBytes(index);
                    Assert.AreEqual(val[0], ClientDataReceivedEvent[client.ExchangeInterface.LocalEndPoint.ToString()][i + 0].Data[0]);
                    Assert.AreEqual(val[1], ClientDataReceivedEvent[client.ExchangeInterface.LocalEndPoint.ToString()][i + 1].Data[0]);
                    Assert.AreEqual(val[2], ClientDataReceivedEvent[client.ExchangeInterface.LocalEndPoint.ToString()][i + 2].Data[0]);
                    Assert.AreEqual(val[3], ClientDataReceivedEvent[client.ExchangeInterface.LocalEndPoint.ToString()][i + 3].Data[0]);
                    index++;
                }
                perfomanceList.Add(value);
            }

            perfmance = perfomanceList[0];
            //lock (_outputLockObject)
            //{
            //    System.Diagnostics.Trace.WriteLine($"Server -> Client {client.ExchangeInterface.LocalEndPoint} Packets per second {perfomanceList[0]}");
            //}
            for (int i = 1; i < perfomanceList.Count; i++)
            {
                //lock (_outputLockObject)
                //{
                //    System.Diagnostics.Trace.WriteLine($"Server -> Client {client.ExchangeInterface.LocalEndPoint} Packets per second {perfomanceList[i]}");
                //}
                perfmance = (perfmance + perfomanceList[i]) / 2;
            }

            lock (_outputLockObject)
            {
                System.Diagnostics.Trace.WriteLine($"Server -> Client {client.ExchangeInterface.LocalEndPoint} Averange Packets per second {perfmance}");
            }
            Assert.IsTrue(perfmance > perfomance);

            client.Dispose();
        }

        [TestMethod]
        public void MultiClientPerfomanceTest()
        {

            var clientsCount = 20;

            var server = new TcpServer();
            server.ErrorEvent += Server_ErrorEvent;
            server.DataReceivedEvent += Server_DataReceivedEvent;

            var epList = new List<TcpServerLocalEndpoint>();
            var ipEp1 = (new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6574), clientsCount));

            epList.Add(ipEp1);
            server.Start(epList);

            for (int i = 0; i < clientsCount; i++)
            {
                ThreadPool.QueueUserWorkItem(CallBack, server);
            }

            Thread.Sleep(30000);

            server.Dispose();
        }

        private void CallBack(object state)
        {
            try
            {
                TestPerfomance((TcpServer)state, 2500);
            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }
        }

        public Dictionary<string, List<NetworkDataEventArgs>> ClientDataReceivedEvent { get; } = new Dictionary<string, List<NetworkDataEventArgs>>();
        private void Client_DataReceivedEvent(object sender, NetworkDataEventArgs e)
        {
            if (!ClientDataReceivedEvent.ContainsKey(e.LocalEndPoint.ToString())) ClientDataReceivedEvent.Add(e.LocalEndPoint.ToString(), new List<NetworkDataEventArgs>());
            ClientDataReceivedEvent[e.LocalEndPoint.ToString()].Add(e);
        }
        public Dictionary<string, List<NetworkDataEventArgs>> ServerDataReceivedEvent { get; } = new Dictionary<string, List<NetworkDataEventArgs>>();
        private void Server_DataReceivedEvent(object sender, NetworkDataEventArgs e)
        {
            if (!ServerDataReceivedEvent.ContainsKey(e.RemoteEndPoint.ToString())) ServerDataReceivedEvent.Add(e.RemoteEndPoint.ToString(), new List<NetworkDataEventArgs>());
            ServerDataReceivedEvent[e.RemoteEndPoint.ToString()].Add(e);
        }


        private void Client_ErrorEvent(object sender, System.IO.ErrorEventArgs e)
        {
            Assert.Fail(e.GetException().ToString());
        }
        private void Server_ErrorEvent(object sender, System.IO.ErrorEventArgs e)
        {
            Assert.Fail(e.GetException().ToString());
        }
    }
}
