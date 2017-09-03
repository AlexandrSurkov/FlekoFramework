using System;
using System.Collections.Generic;
using System.Threading;
using Flekosoft.Common.Video;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Video
{
    class TestFrameProcessor : FrameProcessor
    {
        public List<VideoFrame> ProcessFrameCalled { get;}  = new List<VideoFrame>();
        public bool Return { get; set; } = true;
        protected override void ProcessFrameInternal(VideoFrame frame)
        {
            while (!Return) { }
            ProcessFrameCalled.Add(frame);
        }
    }

    [TestClass]
    public class FrameProcessorTests
    {
        [TestMethod]
        public void FrameProcessorTest()
        {
            var fp = new TestFrameProcessor();

            for (byte i = 0; i < 10; i++)
            {
                var frame = new VideoFrame(DateTime.Now, new[] { i }, Resolution.Resolution1MP, FrameFormat.Argb32);
                fp.ProcessFrameCalled.Clear();
                fp.ProcessFrame(frame);
                Thread.Sleep(10);
                Assert.AreEqual(frame, fp.ProcessFrameCalled[0]);
                frame.Dispose();
            }
            Assert.IsFalse(fp.IsDisposed);
            fp.Dispose();
            Assert.IsTrue(fp.IsDisposed);
        }

        [TestMethod]
        public void FrameProcessorQueueTest()
        {
            var fp = new TestFrameProcessor();

            var framesList = new List<VideoFrame>();
            fp.Return = false;

            for (byte i = 0; i < 10; i++)
            {
                var frame = new VideoFrame(DateTime.Now, new byte[] { i }, Resolution.Resolution1MP, FrameFormat.Argb32);
                framesList.Add(frame);
                fp.ProcessFrame(frame);
            }
            Assert.AreEqual(0, fp.ProcessFrameCalled.Count);
            fp.Return = true;
            Thread.Sleep(10);
            Assert.AreEqual(10, fp.ProcessFrameCalled.Count);

            for (byte i = 0; i < 10; i++)
            {
                Assert.AreEqual(framesList[i], fp.ProcessFrameCalled[i]);
                framesList[i].Dispose();
            }
            
            Assert.IsFalse(fp.IsDisposed);
            fp.Dispose();
            Assert.IsTrue(fp.IsDisposed);

        }


    }
}
