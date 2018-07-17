using System.Collections.Generic;
using Flekosoft.Common.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Math
{
    [TestClass]
    public class Crc8Tests
    {
        [TestMethod]
        public void Crc8Test()
        {
            List<byte> chain = new List<byte>() { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 };

            var result = Crc8.Calculate(Crc8.Crc8Type.Crc8, chain.ToArray(), chain.Count, 0);
            Assert.AreEqual(0xBC, result);

            result = Crc8.Calculate(Crc8.Crc8Type.Crc8Ccitt, chain.ToArray(), chain.Count, 0);
            Assert.AreEqual(0xF4, result);

            result = Crc8.Calculate(Crc8.Crc8Type.Crc8DallasMaxim, chain.ToArray(), chain.Count, 0);
            Assert.AreEqual(0xA2, result);

            result = Crc8.Calculate(Crc8.Crc8Type.Crc8SaeJ1850, chain.ToArray(), chain.Count, 0);
            Assert.AreEqual(0x37, result);

            result = Crc8.Calculate(Crc8.Crc8Type.Crc8Wcdma, chain.ToArray(), chain.Count, 0);
            Assert.AreEqual(0xEA, result);



            chain = new List<byte>() { 0x01, 0x02, 0x03, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 };

            var offset = 3;

            result = Crc8.Calculate(Crc8.Crc8Type.Crc8, chain.ToArray(), chain.Count - offset, offset);
            Assert.AreEqual(0xBC, result);

            result = Crc8.Calculate(Crc8.Crc8Type.Crc8Ccitt, chain.ToArray(), chain.Count - offset, offset);
            Assert.AreEqual(0xF4, result);

            result = Crc8.Calculate(Crc8.Crc8Type.Crc8DallasMaxim, chain.ToArray(), chain.Count - offset, offset);
            Assert.AreEqual(0xA2, result);

            result = Crc8.Calculate(Crc8.Crc8Type.Crc8SaeJ1850, chain.ToArray(), chain.Count - offset, offset);
            Assert.AreEqual(0x37, result);

            result = Crc8.Calculate(Crc8.Crc8Type.Crc8Wcdma, chain.ToArray(), chain.Count - offset, offset);
            Assert.AreEqual(0xEA, result);

        }
    }
}
