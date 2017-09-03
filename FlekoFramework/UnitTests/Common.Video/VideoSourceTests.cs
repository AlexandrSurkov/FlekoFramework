using System;
using System.Collections.Generic;
using Flekosoft.Common.Video;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Video
{
    class TestVideoSource : VideoSource
    {
        public TestVideoSource()
        {
            SupportedFps = new List<int>() { 1, 2, 3 }.AsReadOnly();
            SupportedResolution = new List<Resolution>() { Resolution.ResolutionCIF, Resolution.Resolution2CIF }.AsReadOnly();
            VideoResolution = SupportedResolution[0];
            FramesPerSecond = SupportedFps[0];
        }
        public bool IsStartInternalCalled { get; set; }
        public bool StartInternalReturnValue { get; set; } = true;
        protected override bool StartInternal()
        {
            IsStartInternalCalled = true;
            return StartInternalReturnValue;
        }

        public bool IsStopInternalCalled { get; set; }

        protected override void StopInternal()
        {
            IsStopInternalCalled = true;
        }

        public void SendNewFrame(VideoFrame frame)
        {
            OnNewFrame(frame);
        }
    }

    [TestClass]
    public class VideoSourceTests
    {
        [TestMethod]
        public void VideoSourceTest()
        {
            var vst = new TestVideoSource();

            Assert.IsFalse(vst.IsStarted);
            vst.StartInternalReturnValue = false;
            vst.IsStartInternalCalled = false;
            vst.Start();
            Assert.IsFalse(vst.IsStarted);
            Assert.IsTrue(vst.IsStartInternalCalled);

            vst.StartInternalReturnValue = true;
            vst.IsStartInternalCalled = false;
            vst.Start();
            Assert.IsTrue(vst.IsStarted);
            Assert.IsTrue(vst.IsStartInternalCalled);

            vst.IsStopInternalCalled = false;
            vst.Stop();
            Assert.IsFalse(vst.IsStarted);
            Assert.IsTrue(vst.IsStopInternalCalled);

            Assert.AreNotEqual(vst.SupportedResolution[1], vst.VideoResolution);
            vst.VideoResolution = vst.SupportedResolution[1];
            Assert.AreEqual(vst.SupportedResolution[1], vst.VideoResolution);

            Assert.AreNotEqual(vst.SupportedFps[1], vst.FramesPerSecond);
            vst.FramesPerSecond = vst.SupportedFps[1];
            Assert.AreEqual(vst.SupportedFps[1], vst.FramesPerSecond);

            vst.NewFrame += Vst_NewFrame;
            FrameEventArgs = null;
            Assert.IsNull(FrameEventArgs);
            var frame = new VideoFrame(DateTime.Now, new byte[] { 1, 2, 3 }, Resolution.Resolution1MP, FrameFormat.Argb32);
            vst.SendNewFrame(frame);
            Assert.AreEqual(frame, FrameEventArgs?.Frame);
            frame.Dispose();

            Assert.IsFalse(vst.IsDisposed);
            vst.Dispose();
            Assert.IsTrue(vst.IsDisposed);
        }

        public FrameEventArgs FrameEventArgs { get; set; }
        private void Vst_NewFrame(object sender, FrameEventArgs e)
        {
            FrameEventArgs = e;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ResolutionExceptionTest()
        {
            var vst = new TestVideoSource();
            vst.VideoResolution = Resolution.Resolution1080p;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FpsExceptionTest()
        {
            var vst = new TestVideoSource();
            vst.FramesPerSecond = 1000;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NewResolutionNullExceptionTest()
        {
            var vst = new TestVideoSource();
            vst.VideoResolution = null;
        }

    }
}
