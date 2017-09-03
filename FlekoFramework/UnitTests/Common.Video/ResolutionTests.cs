using Flekosoft.Common.Video;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Video
{
    [TestClass]
    public class ResolutionTests
    {
        [TestMethod]
        public void ResolutionTest()
        {
            var res = new Resolution(100, 200);
            Assert.AreEqual(100, res.Width);
            Assert.AreEqual(200, res.Height);
            var res1 = new Resolution(res);
            Assert.AreEqual(100, res1.Width);
            Assert.AreEqual(200, res1.Height);
        }

        [TestMethod]
        public void ResolutionPresetsTest()
        {

            var res = Resolution.ResolutionQCIF; Assert.AreEqual(176, res.Width); Assert.AreEqual(120, res.Height);
            res = Resolution.ResolutionCIF; Assert.AreEqual(352, res.Width); Assert.AreEqual(240, res.Height);
            res = Resolution.Resolution2CIF; Assert.AreEqual(704, res.Width); Assert.AreEqual(240, res.Height);
            res = Resolution.Resolution4CIF; Assert.AreEqual(704, res.Width); Assert.AreEqual(480, res.Height);
            res = Resolution.ResolutionD1; Assert.AreEqual(720, res.Width); Assert.AreEqual(480, res.Height);
            res = Resolution.Resolution720p; Assert.AreEqual(1280, res.Width); Assert.AreEqual(720, res.Height);
            res = Resolution.Resolution960p; Assert.AreEqual(1280, res.Width); Assert.AreEqual(960, res.Height);
            res = Resolution.Resolution1MP; Assert.AreEqual(1280, res.Width); Assert.AreEqual(1024, res.Height);
            res = Resolution.Resolution2MP; Assert.AreEqual(1600, res.Width); Assert.AreEqual(1200, res.Height);
            res = Resolution.Resolution1080p; Assert.AreEqual(1920, res.Width); Assert.AreEqual(1080, res.Height);
            res = Resolution.Resolution3MP; Assert.AreEqual(2048, res.Width); Assert.AreEqual(1536, res.Height);
            res = Resolution.Resolution5MP; Assert.AreEqual(2592, res.Width); Assert.AreEqual(1944, res.Height);
        }
    }
}
