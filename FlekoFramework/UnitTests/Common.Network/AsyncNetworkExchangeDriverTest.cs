using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Flekosoft.Common.Network;
using Flekosoft.Common.Network.Tcp.Internals;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flekosoft.Common;

namespace Flekosoft.UnitTests.Common.Network
{
    class Driver : AsyncNetworkExchangeDriver
    {
        public List<byte> ReceivedBytes { get; } = new List<byte>();

        public bool WriteData(byte[] data)
        {
            return Write(data);
        }

        public void Start(INetworkExchangeInterface networkInterface)
        {
            StartExchange(networkInterface);
        }

        public void Stop()
        {
            StopExchange();
        }

        protected override void ProcessByteInternal(NetworkDataEventArgs e)
        {
            ReceivedBytes.AddRange(e.Data);
        }
    }

    class SenderInterface : DisposableBase, INetworkExchangeInterface
    {
        private readonly ConcurrentQueue<byte> _senderToReceiverQueue;
        private readonly ConcurrentQueue<byte> _receiverToSenderQueue;

        public SenderInterface(ConcurrentQueue<byte> senderToReceiverQueue, ConcurrentQueue<byte> receiverToSenderQueue)
        {
            _senderToReceiverQueue = senderToReceiverQueue;
            _receiverToSenderQueue = receiverToSenderQueue;
        }

        public int Read(byte[] data, int timeout)
        {
            int i;
            for (i = 0; i < data.Length; i++)
            {
                if (!_receiverToSenderQueue.TryDequeue(out data[i])) break;
            }
            return i;
        }

        public int Write(byte[] buffer, int offset, int size, int timeout)
        {
            var count = 0;
            for (int i = offset; i < (size + offset); i++)
            {
                _senderToReceiverQueue.Enqueue(buffer[i]);
                count++;
            }
            return count;
        }

        public bool IsConnected { get; set; }
        public IPEndPoint LocalEndPoint { get; } = new IPEndPoint(1234, 1234);
        public IPEndPoint RemoteEndPoint { get; } = new IPEndPoint(4321, 4321);

        public event EventHandler DisconnectedEvent;

        public void SendDisconnctedEvent()
        {
            DisconnectedEvent?.Invoke(this,EventArgs.Empty);
        }
    }

    class ReceiverInterface : DisposableBase, INetworkExchangeInterface
    {
        private readonly ConcurrentQueue<byte> _senderToReceiverQueue;
        private readonly ConcurrentQueue<byte> _receiverToSenderQueue;

        public ReceiverInterface(ConcurrentQueue<byte> senderToReceiverQueue, ConcurrentQueue<byte> receiverToSenderQueue)
        {
            _senderToReceiverQueue = senderToReceiverQueue;
            _receiverToSenderQueue = receiverToSenderQueue;
        }

        public int Read(byte[] data, int timeout)
        {
            int i;
            for (i = 0; i < data.Length; i++)
            {
                if (!_senderToReceiverQueue.TryDequeue(out data[i])) break;
            }
            return i;
        }

        public int Write(byte[] buffer, int offset, int size, int timeout)
        {
            var count = 0;
            for (int i = offset; i < (size + offset); i++)
            {
                _receiverToSenderQueue.Enqueue(buffer[i]);
                count++;
            }
            return count;
        }

        public bool IsConnected { get; set; }
        public IPEndPoint LocalEndPoint { get; } = new IPEndPoint(1234, 1234);
        public IPEndPoint RemoteEndPoint { get; } = new IPEndPoint(4321, 4321);

        public event EventHandler DisconnectedEvent;

        public void SendDisconnctedEvent()
        {
            DisconnectedEvent?.Invoke(this, EventArgs.Empty);
        }
    }


    [TestClass]
    public class AsyncNetworkExchangeDriverTest
    {
        readonly ConcurrentQueue<byte> _senderToReceiverQueue = new ConcurrentQueue<byte>();
        readonly ConcurrentQueue<byte> _receiverToSenderQueue = new ConcurrentQueue<byte>();
        [TestMethod]
        public void Test()
        {
            var senderInterface = new SenderInterface(_senderToReceiverQueue, _receiverToSenderQueue)
            { IsConnected = false };
            var receiverInterface = new ReceiverInterface(_senderToReceiverQueue, _receiverToSenderQueue)
            { IsConnected = false };
            var sender = new Driver();
            var receiver = new Driver();

            sender.StartedEvent += Sender_StartedEvent;
            sender.StoppedEvent += Sender_StoppedEvent;

            Assert.IsFalse(sender.IsStarted);
            Assert.IsFalse(receiver.IsStarted);

            Assert.IsFalse(StoppedEventCalled);
            Assert.IsFalse(StartedEventCalled);

            sender.Start(senderInterface);
            Assert.IsFalse(StoppedEventCalled);
            Assert.IsTrue(StartedEventCalled);

            receiver.Start(receiverInterface);

            Assert.IsTrue(sender.IsStarted);
            Assert.IsTrue(receiver.IsStarted);

            StoppedEventCalled = false;
            StartedEventCalled = false;
            Assert.IsFalse(StoppedEventCalled);
            Assert.IsFalse(StartedEventCalled);

            sender.Stop();

            Assert.IsTrue(StoppedEventCalled);
            Assert.IsFalse(StartedEventCalled);


            Assert.IsFalse(sender.IsStarted);
            Assert.IsTrue(receiver.IsStarted);
            receiver.Stop();
            Assert.IsFalse(receiver.IsStarted);
            Assert.IsFalse(sender.IsStarted);

            sender.Start(senderInterface);
            receiver.Start(receiverInterface);

            Assert.IsTrue(sender.IsStarted);
            Assert.IsTrue(receiver.IsStarted);

            Assert.AreEqual(0, sender.ReceivedBytes.Count);
            Assert.AreEqual(0, receiver.ReceivedBytes.Count);

            var senderData = new byte[] { 0x03, 0x02, 0x01 };
            Assert.IsFalse(sender.WriteData(senderData));
            Thread.Sleep(10);
            Assert.AreEqual(0, receiver.ReceivedBytes.Count);

            var receiverData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            Assert.IsFalse(receiver.WriteData(receiverData));
            Thread.Sleep(10);
            Assert.AreEqual(0, sender.ReceivedBytes.Count);

            senderInterface.IsConnected = true;
            receiverInterface.IsConnected = true;

            Assert.IsTrue(sender.WriteData(senderData));
            Assert.IsTrue(receiver.WriteData(receiverData));

            Thread.Sleep(100);

            Assert.AreEqual(4, sender.ReceivedBytes.Count);
            for (int i = 0; i < receiverData.Length; i++)
            {
                Assert.AreEqual(receiverData[i], sender.ReceivedBytes[i]);
            }

            Assert.AreEqual(3, receiver.ReceivedBytes.Count);
            for (int i = 0; i < senderData.Length; i++)
            {
                Assert.AreEqual(senderData[i], receiver.ReceivedBytes[i]);
            }

            var perfomanceList = new List<int>();
            //Perfomance Test
            for (int j = 0; j < 10; j++)
            {
                var startTime = DateTime.Now;
                var now = DateTime.Now;
                int value = 1;
                sender.ReceivedBytes.Clear();
                receiver.ReceivedBytes.Clear();
                var index = 1;

                while ((now - startTime).TotalSeconds < 1)
                {
                    Assert.IsTrue(sender.WriteData(BitConverter.GetBytes(value)));
                    while (receiver.ReceivedBytes.Count < value * sizeof(int)) { }
                    value++;
                    now = DateTime.Now;
                }
                System.Diagnostics.Trace.WriteLine($"Packets per second {value}");
                for (int i = 0; i < value; i += sizeof(int))
                {
                    var val = BitConverter.GetBytes(index);
                    Assert.AreEqual(val[0], receiver.ReceivedBytes[i + 0]);
                    Assert.AreEqual(val[1], receiver.ReceivedBytes[i + 1]);
                    Assert.AreEqual(val[2], receiver.ReceivedBytes[i + 2]);
                    Assert.AreEqual(val[3], receiver.ReceivedBytes[i + 3]);
                    index++;
                }
                perfomanceList.Add(value);
            }

            var perfmance = perfomanceList[0];
            for (int i = 1; i < perfomanceList.Count; i++)
            {
                perfmance = (perfmance + perfomanceList[i]) / 2;
            }
            Assert.IsTrue(perfmance > 500);

            sender.ReceivedBytes.Clear();
            receiver.ReceivedBytes.Clear();

            Assert.AreEqual(0, sender.ReceivedBytes.Count);
            Assert.AreEqual(0, receiver.ReceivedBytes.Count);

            sender.Stop();
            Assert.IsFalse(sender.IsStarted);
            Assert.IsTrue(receiver.IsStarted);
            receiver.Stop();
            Assert.IsFalse(receiver.IsStarted);
            Assert.IsFalse(sender.IsStarted);

            Assert.IsFalse(sender.WriteData(senderData));
            Thread.Sleep(10);
            Assert.AreEqual(0, receiver.ReceivedBytes.Count);

            Assert.IsFalse(receiver.WriteData(receiverData));
            Thread.Sleep(10);
            Assert.AreEqual(0, sender.ReceivedBytes.Count);

            sender.Start(senderInterface);
            receiver.Start(receiverInterface);

            Assert.IsTrue(sender.IsStarted);
            Assert.IsTrue(receiver.IsStarted);

            //Test exceptions while chnage ReadBuffer
            receiver.ReadBufferSize = 256;
            Assert.AreEqual(256, receiver.ReadBufferSize);
            sender.ReadBufferSize = 256;
            Assert.AreEqual(256, sender.ReadBufferSize);

            //Test Stup while sending
            for (int i = 0; i < 1000; i += sizeof(int))
            {
                var val = BitConverter.GetBytes(i);
                Assert.IsTrue(sender.WriteData(val));

                if (i == 900) receiver.Stop();
            }

            receiver.Start(receiverInterface);
            //DataTrace test
            Assert.IsFalse(sender.DataTrace);
            sender.DataTrace = true;
            Assert.IsTrue(sender.DataTrace);

            Assert.IsNull(SenderDataEventArgs);
            Assert.IsNull(ReceiverDataEventArgs);

            Assert.IsTrue(sender.WriteData(senderData));

            Assert.IsNull(SenderDataEventArgs);
            Assert.IsNull(ReceiverDataEventArgs);

            sender.SendDataTraceEvent += Sender_SendDataTraceEvent;

            Assert.IsTrue(sender.WriteData(senderData));
            Assert.IsNotNull(SenderDataEventArgs);
            Assert.AreEqual(senderData[0], SenderDataEventArgs.Data[0]);
            Assert.AreEqual(senderData[1], SenderDataEventArgs.Data[1]);
            Assert.AreEqual(senderData[2], SenderDataEventArgs.Data[2]);
            Assert.IsNull(ReceiverDataEventArgs);

            Assert.IsFalse(receiver.DataTrace);
            receiver.DataTrace = true;
            Assert.IsTrue(receiver.DataTrace);

            SenderDataEventArgs = null;
            Assert.IsTrue(sender.WriteData(senderData));
            Assert.IsNotNull(SenderDataEventArgs);
            Assert.AreEqual(senderData[0], SenderDataEventArgs.Data[0]);
            Assert.AreEqual(senderData[1], SenderDataEventArgs.Data[1]);
            Assert.AreEqual(senderData[2], SenderDataEventArgs.Data[2]);
            Assert.IsNull(ReceiverDataEventArgs);

            receiver.ReceiveDataTraceEvent += Receiver_ReceiveDataTraceEvent;

            SenderDataEventArgs = null;
            Assert.IsTrue(sender.WriteData(senderData));
            Assert.IsNotNull(SenderDataEventArgs);
            Assert.AreEqual(senderData[0], SenderDataEventArgs.Data[0]);
            Assert.AreEqual(senderData[1], SenderDataEventArgs.Data[1]);
            Assert.AreEqual(senderData[2], SenderDataEventArgs.Data[2]);
            Thread.Sleep(100);
            Assert.IsNotNull(ReceiverDataEventArgs);
            Assert.AreEqual(senderData[0], ReceiverDataEventArgs.Data[0]);
            Assert.AreEqual(senderData[1], ReceiverDataEventArgs.Data[1]);
            Assert.AreEqual(senderData[2], ReceiverDataEventArgs.Data[2]);


            sender.Dispose();
            receiver.Dispose();

            Assert.IsTrue(senderInterface.IsDisposed);
            Assert.IsTrue(receiverInterface.IsDisposed);

        }

        [TestMethod]
        public void DataTraceTest()
        {
            Assert.Fail();
        }

        public NetworkDataEventArgs SenderDataEventArgs { get; set; } = null;
        public NetworkDataEventArgs ReceiverDataEventArgs { get; set; } = null;

        private void Receiver_ReceiveDataTraceEvent(object sender, NetworkDataEventArgs e)
        {
            ReceiverDataEventArgs = e;
        }

        private void Sender_SendDataTraceEvent(object sender, NetworkDataEventArgs e)
        {
            SenderDataEventArgs = e;
        }

        public bool StartedEventCalled { get; set; }
        public bool StoppedEventCalled { get; set; }

        private void Sender_StoppedEvent(object sender, EventArgs e)
        {
            StoppedEventCalled = true;
        }

        private void Sender_StartedEvent(object sender, EventArgs e)
        {
            StartedEventCalled = true;
        }
    }
}
