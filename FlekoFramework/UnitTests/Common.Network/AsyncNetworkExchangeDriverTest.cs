using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Flekosoft.Common.Network.Tcp.Internals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Network
{
    class Driver : AsyncNetworkExchangeDriver
    {
        public List<byte> ReceivedBytes { get; } = new List<byte>();

        protected override void ProcessByteInternal(byte dataByte)
        {
            ReceivedBytes.Add(dataByte);
        }

        public bool WriteData(byte[] data)
        {
            return Write(data);
        }
    }

    class SenderInterface : INetworkExchangeInterface
    {
        private readonly ConcurrentQueue<byte> _senderToReceiverQueue;
        private readonly ConcurrentQueue<byte> _receiverToSenderQueue;

        public SenderInterface(ConcurrentQueue<byte> senderToReceiverQueue, ConcurrentQueue<byte> receiverToSenderQueue)
        {
            _senderToReceiverQueue = senderToReceiverQueue;
            _receiverToSenderQueue = receiverToSenderQueue;
        }

        public int Read(byte[] data)
        {
            int i;
            for (i = 0; i < data.Length; i++)
            {
                if (!_receiverToSenderQueue.TryDequeue(out data[i])) break;
            }
            return i;
        }

        public int Write(byte[] buffer, int offset, int size)
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
        public IPEndPoint LocalEndpoint { get; } = new IPEndPoint(1234, 1234);
        public IPEndPoint RemoteEndpoint { get; } = new IPEndPoint(4321, 4321);
    }

    class ReceiverInterface : INetworkExchangeInterface
    {
        private readonly ConcurrentQueue<byte> _senderToReceiverQueue;
        private readonly ConcurrentQueue<byte> _receiverToSenderQueue;

        public ReceiverInterface(ConcurrentQueue<byte> senderToReceiverQueue, ConcurrentQueue<byte> receiverToSenderQueue)
        {
            _senderToReceiverQueue = senderToReceiverQueue;
            _receiverToSenderQueue = receiverToSenderQueue;
        }

        public int Read(byte[] data)
        {
            int i;
            for (i = 0; i < data.Length; i++)
            {
                if (!_senderToReceiverQueue.TryDequeue(out data[i])) break;
            }
            return i;
        }

        public int Write(byte[] buffer, int offset, int size)
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
        public IPEndPoint LocalEndpoint { get; } = new IPEndPoint(1234, 1234);
        public IPEndPoint RemoteEndpoint { get; } = new IPEndPoint(4321, 4321);
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

            Assert.IsFalse(sender.IsStarted);
            Assert.IsFalse(receiver.IsStarted);

            sender.Start(senderInterface);
            receiver.Start(receiverInterface);

            Assert.IsTrue(sender.IsStarted);
            Assert.IsTrue(receiver.IsStarted);

            sender.Stop();
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

            for (int j = 0; j < 20; j++)
            {
                var startTime = DateTime.Now;
                var now = DateTime.Now;
                int value = 1;
                sender.ReceivedBytes.Clear();
                receiver.ReceivedBytes.Clear();

                while ((now - startTime).TotalSeconds < 1)
                {
                    Assert.IsTrue(sender.WriteData(BitConverter.GetBytes(value)));
                    while (receiver.ReceivedBytes.Count < value * sizeof(int)) { }
                    value++;
                    now = DateTime.Now;
                }
                System.Diagnostics.Trace.WriteLine($"Packets per second {value}");
                //var index = 1;
                //for (int i = 0; i < value; i += sizeof(long))
                //{
                //    Assert.AreEqual(index, BitConverter.ToInt32(receiver.ReceivedBytes.ToArray(), i));
                //    index++;
                //}
            }




            sender.Dispose();
            receiver.Dispose();
        }
    }
}
