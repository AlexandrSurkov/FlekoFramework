using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading;
using Flekosoft.Common.Network;
using Flekosoft.Common.Network.Igmp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Network
{
    [TestClass]
    public class IgmpClientTest
    {
        [TestMethod]
        public void IgmpClient_AddressSpaceTest()
        {
            var client = new IgmpClient();

            var arr = IPAddress.Parse("224.0.0.0").GetAddressBytes();
            if (BitConverter.IsLittleEndian) Array.Reverse(arr);
            var validStartAddrRange = BitConverter.ToUInt32(arr, 0);

            arr = IPAddress.Parse("224.0.0.0").GetAddressBytes();
            if (BitConverter.IsLittleEndian) Array.Reverse(arr);
            var validStopAddrRange = BitConverter.ToUInt32(arr, 0);

            for (uint i = validStartAddrRange; i <= validStopAddrRange; i++)
            {

                var ipAddr = BitConverter.GetBytes(i);
                if (BitConverter.IsLittleEndian) Array.Reverse(ipAddr);

                client.Start(new IPAddress(ipAddr), 2000, ExchangeType.Sender);
                client.Stop();
            }

            client.Dispose();
        }


        [TestMethod]
        public void IgmpClient_CreateStartStopDisposeTest()
        {
            var client = new IgmpClient();
            client.ErrorEvent += Client_ErrorEvent;
            Assert.IsFalse(client.DataTrace);
            Assert.IsFalse(client.IsStarted);
            Assert.IsFalse(client.IsDisposed);

            ClientPropertyChangedEventArgs = null;
            StoppedEventColled = false;
            StartedEventColled = false;
            Assert.IsNull(ClientPropertyChangedEventArgs);
            Assert.IsFalse(StoppedEventColled);
            Assert.IsFalse(StartedEventColled);

            //Check just properties and events
            client.Start(IPAddress.Parse("224.0.0.0"), 2000, ExchangeType.Sender);
            Assert.IsFalse(client.DataTrace);
            Assert.IsTrue(client.IsStarted);
            Assert.IsFalse(client.IsDisposed);

            Assert.IsNull(ClientPropertyChangedEventArgs);
            Assert.IsFalse(StoppedEventColled);
            Assert.IsFalse(StartedEventColled);
            client.Stop();

            Assert.IsFalse(client.DataTrace);
            Assert.IsFalse(client.IsStarted);
            Assert.IsFalse(client.IsDisposed);

            Assert.IsNull(ClientPropertyChangedEventArgs);
            Assert.IsFalse(StoppedEventColled);
            Assert.IsFalse(StartedEventColled);

            client.PropertyChanged += Client_PropertyChanged;
            client.StartedEvent += Client_StartedEvent;
            client.StoppedEvent += Client_StoppedEvent;

            //Start with events
            client.Start(IPAddress.Parse("239.255.255.255"), 2000, ExchangeType.SenderAndReceiver);
            Assert.IsFalse(client.DataTrace);
            Assert.IsTrue(client.IsStarted);
            Assert.IsFalse(client.IsDisposed);

            Assert.AreEqual("IsStarted", ClientPropertyChangedEventArgs?.PropertyName);
            Assert.IsFalse(StoppedEventColled);
            Assert.IsTrue(StartedEventColled);

            ClientPropertyChangedEventArgs = null;
            StoppedEventColled = false;
            StartedEventColled = false;
            Assert.IsNull(ClientPropertyChangedEventArgs);
            Assert.IsFalse(StoppedEventColled);
            Assert.IsFalse(StartedEventColled);

            client.Stop();
            Assert.IsFalse(client.DataTrace);
            Assert.IsFalse(client.IsStarted);
            Assert.IsFalse(client.IsDisposed);

            Assert.AreEqual("IsStarted", ClientPropertyChangedEventArgs?.PropertyName);
            Assert.IsTrue(StoppedEventColled);
            Assert.IsFalse(StartedEventColled);

            client.Start(IPAddress.Parse("224.0.0.0"), 2000, ExchangeType.Sender);
            client.Dispose();
            Assert.IsFalse(client.IsStarted);
            Assert.IsTrue(client.IsDisposed);

        }

        [TestMethod]
        public void IgmpClient_PropertiesTest()
        {
            var client = new IgmpClient();
            client.ErrorEvent += Client_ErrorEvent;
            client.PropertyChanged += Client_PropertyChanged;

            ClientPropertyChangedEventArgs = null;
            Assert.IsNull(ClientPropertyChangedEventArgs);
            var intVal = client.ReadBufferSize + 1;
            client.ReadBufferSize = intVal;
            Assert.AreEqual("ReadBufferSize", ClientPropertyChangedEventArgs?.PropertyName);
            Assert.AreEqual(intVal, client.ReadBufferSize);

            client.DataTrace = false;
            ClientPropertyChangedEventArgs = null;
            Assert.IsNull(ClientPropertyChangedEventArgs);
            client.DataTrace = !client.DataTrace;
            Assert.AreEqual("DataTrace", ClientPropertyChangedEventArgs?.PropertyName);

            Assert.IsTrue(client.DataTrace);

            client.Start(IPAddress.Parse("224.0.0.0"), 2000, ExchangeType.Sender);
            Assert.AreEqual(IPAddress.Parse("224.0.0.0"), client.MulticastIpAddress);

            client.Dispose();
        }

        [TestMethod]
        public void IgmpClient_SendReceive2instancesTest()
        {
            var ipaddr = IPAddress.Parse("224.0.0.0");

            var client1 = new IgmpClient();
            client1.DataReceivedEvent += Client1_DataReceivedEvent;
            client1.ErrorEvent += Client_ErrorEvent;
            client1.Start(ipaddr, 2000, ExchangeType.Sender);

            var client2 = new IgmpClient();
            client2.DataReceivedEvent += Client2_DataReceivedEvent;
            client2.ErrorEvent += Client_ErrorEvent;
            client2.Start(ipaddr, 2000, ExchangeType.SenderAndReceiver);

            Thread.Sleep(100);

            for (int i = 1; i < 10; i++)
            {
                var data = BitConverter.GetBytes(i);
                Client2DataReceivedEvent.Clear();
                Assert.AreEqual(0, Client2DataReceivedEvent.Count);

                Client1DataReceivedEvent.Clear();
                Assert.AreEqual(0, Client1DataReceivedEvent.Count);

                client1.SendData(data);

                Thread.Sleep(100);

                Assert.AreEqual(1, Client2DataReceivedEvent.Count);
                Assert.AreEqual(data.Length, Client2DataReceivedEvent[0].Data.Length);
                Assert.AreEqual(0, Client1DataReceivedEvent.Count);
                for (int j = 0; j < data.Length; j++)
                {
                    Assert.AreEqual(data[j], Client2DataReceivedEvent[0].Data[j]);
                }
            }

            client2.Dispose();
            client1.Dispose();
        }

        [TestMethod]
        public void IgmpClient_SendReceive1instanceTest()
        {
            var ipaddr = IPAddress.Parse("224.0.0.0");

            var client1 = new IgmpClient();
            client1.DataReceivedEvent += Client1_DataReceivedEvent;
            client1.ErrorEvent += Client_ErrorEvent;
            client1.Start(ipaddr, 2000, ExchangeType.SenderAndReceiver);

            Thread.Sleep(100);

            for (int i = 1; i < 10; i++)
            {
                var data = BitConverter.GetBytes(i);
                Client1DataReceivedEvent.Clear();
                Assert.AreEqual(0, Client1DataReceivedEvent.Count);

                client1.SendData(data);

                Thread.Sleep(100);
                Assert.AreEqual(1, Client1DataReceivedEvent.Count);
                Assert.AreEqual(data.Length, Client1DataReceivedEvent[0].Data.Length);
                for (int j = 0; j < data.Length; j++)
                {
                    Assert.AreEqual(data[j], Client1DataReceivedEvent[0].Data[j]);
                }
            }

            client1.Dispose();
        }

        //[TestMethod]
        //public void DataTraceTest()
        //{
        //    var server = new TcpServer();

        //    var epList = new List<TcpServerLocalEndpoint>();
        //    var ipEp1 = (new TcpServerLocalEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2901), 1));

        //    epList.Add(ipEp1);
        //    server.DataReceivedEvent += Server_DataReceivedEvent;
        //    server.Start(epList);

        //    var client = new TcpClient();
        //    client.DataReceivedEvent += Client_DataReceivedEvent;
        //    client.ReceiveDataTraceEvent += Client_ReceiveDataTraceEvent;
        //    client.SendDataTraceEvent += Client_SendDataTraceEvent;

        //    client.Start(ipEp1.EndPoint);
        //    Thread.Sleep(200);

        //    client.DataTrace = false;
        //    server.DataTrace = false;

        //    for (int i = 1; i < 10; i++)
        //    {
        //        var data = BitConverter.GetBytes(i);
        //        ServerDataReceivedEvent.Clear();
        //        ClientSendDataTraceEvent.Clear();
        //        Assert.AreEqual(0, ServerDataReceivedEvent.Count);
        //        Assert.AreEqual(0, ClientSendDataTraceEvent.Count);
        //        client.SendData(data);
        //        Thread.Sleep(100);
        //        Assert.AreEqual(data.Length, ServerDataReceivedEvent.Count);
        //        Assert.AreEqual(0, ClientSendDataTraceEvent.Count);
        //        for (int j = 0; j < data.Length; j++)
        //        {
        //            Assert.AreEqual(data[j], ServerDataReceivedEvent[j].Data[0]);
        //            Assert.AreEqual(client.ExchangeInterface.LocalEndPoint, ServerDataReceivedEvent[j].RemoteEndPoint);
        //            Assert.AreEqual(client.ExchangeInterface.RemoteEndPoint, ServerDataReceivedEvent[j].LocalEndPoint);
        //        }
        //    }

        //    for (int i = 1; i < 10; i++)
        //    {
        //        var data = BitConverter.GetBytes(i);
        //        ClientDataReceivedEvent.Clear();
        //        ClientReceiveDataTraceEvent.Clear();
        //        Assert.AreEqual(0, ClientDataReceivedEvent.Count);
        //        Assert.AreEqual(0, ClientReceiveDataTraceEvent.Count);
        //        server.Write(data, client.ExchangeInterface.RemoteEndPoint, client.ExchangeInterface.LocalEndPoint);
        //        Thread.Sleep(100);
        //        Assert.AreEqual(data.Length, ClientDataReceivedEvent.Count);
        //        Assert.AreEqual(0, ClientReceiveDataTraceEvent.Count);
        //        for (int j = 0; j < data.Length; j++)
        //        {
        //            Assert.AreEqual(data[j], ClientDataReceivedEvent[j].Data[0]);
        //            Assert.AreEqual(ipEp1.EndPoint, ClientDataReceivedEvent[j].RemoteEndPoint);
        //        }
        //    }


        //    client.DataTrace = true;
        //    server.DataTrace = true;

        //    for (int i = 1; i < 10; i++)
        //    {
        //        var data = BitConverter.GetBytes(i);
        //        ServerDataReceivedEvent.Clear();
        //        ClientSendDataTraceEvent.Clear();
        //        Assert.AreEqual(0, ServerDataReceivedEvent.Count);
        //        Assert.AreEqual(0, ClientSendDataTraceEvent.Count);
        //        client.SendData(data);
        //        Thread.Sleep(100);
        //        Assert.AreEqual(data.Length, ServerDataReceivedEvent.Count);
        //        Assert.AreEqual(1, ClientSendDataTraceEvent.Count);
        //        for (int j = 0; j < data.Length; j++)
        //        {
        //            Assert.AreEqual(data[j], ServerDataReceivedEvent[j].Data[0]);
        //            Assert.AreEqual(client.ExchangeInterface.LocalEndPoint, ServerDataReceivedEvent[j].RemoteEndPoint);
        //            Assert.AreEqual(client.ExchangeInterface.RemoteEndPoint, ServerDataReceivedEvent[j].LocalEndPoint);

        //            Assert.AreEqual(data[j], ClientSendDataTraceEvent[0].Data[j]);
        //        }


        //        Assert.AreEqual(ipEp1.EndPoint, ClientSendDataTraceEvent[0].RemoteEndPoint);
        //    }

        //    for (int i = 1; i < 10; i++)
        //    {
        //        var data = BitConverter.GetBytes(i);
        //        ClientDataReceivedEvent.Clear();
        //        ClientReceiveDataTraceEvent.Clear();
        //        Assert.AreEqual(0, ClientDataReceivedEvent.Count);
        //        Assert.AreEqual(0, ClientReceiveDataTraceEvent.Count);
        //        server.Write(data, client.ExchangeInterface.RemoteEndPoint, client.ExchangeInterface.LocalEndPoint);
        //        Thread.Sleep(100);
        //        Assert.AreEqual(data.Length, ClientDataReceivedEvent.Count);
        //        Assert.AreEqual(1, ClientReceiveDataTraceEvent.Count);
        //        for (int j = 0; j < data.Length; j++)
        //        {
        //            Assert.AreEqual(data[j], ClientDataReceivedEvent[j].Data[0]);
        //            Assert.AreEqual(ipEp1.EndPoint, ClientDataReceivedEvent[j].RemoteEndPoint);

        //            Assert.AreEqual(data[j], ClientReceiveDataTraceEvent[0].Data[j]);
        //        }

        //        Assert.AreEqual(ipEp1.EndPoint, ClientReceiveDataTraceEvent[0].RemoteEndPoint);
        //    }

        //    server.Dispose();
        //}



        public List<NetworkDataEventArgs> ClientSendDataTraceEvent { get; } = new List<NetworkDataEventArgs>();
        private void Client_SendDataTraceEvent(object sender, NetworkDataEventArgs e)
        {
            ClientSendDataTraceEvent.Add(e);
        }

        public List<NetworkDataEventArgs> ClientReceiveDataTraceEvent { get; } = new List<NetworkDataEventArgs>();
        private void Client_ReceiveDataTraceEvent(object sender, NetworkDataEventArgs e)
        {
            ClientReceiveDataTraceEvent.Add(e);
        }

        public List<NetworkDataEventArgs> Client1DataReceivedEvent { get; } = new List<NetworkDataEventArgs>();
        private void Client1_DataReceivedEvent(object sender, NetworkDataEventArgs e)
        {
            Client1DataReceivedEvent.Add(e);
        }

        public List<NetworkDataEventArgs> Client2DataReceivedEvent { get; } = new List<NetworkDataEventArgs>();
        private void Client2_DataReceivedEvent(object sender, NetworkDataEventArgs e)
        {
            Client2DataReceivedEvent.Add(e);
        }





        public bool StoppedEventColled { get; set; }

        private void Client_StoppedEvent(object sender, System.EventArgs e)
        {
            StoppedEventColled = true;
        }

        public bool StartedEventColled { get; set; }

        private void Client_StartedEvent(object sender, System.EventArgs e)
        {
            StartedEventColled = true;
        }

        public PropertyChangedEventArgs ClientPropertyChangedEventArgs { get; set; }

        private void Client_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ClientPropertyChangedEventArgs = e;
        }


        private void Client_ErrorEvent(object sender, System.IO.ErrorEventArgs e)
        {
            Assert.Fail(e.GetException().ToString());
        }
    }
}
