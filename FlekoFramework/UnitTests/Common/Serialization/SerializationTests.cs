using System;
using Flekosoft.UnitTests.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Serialization
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void SerializationTest()
        {
            var i = new SerializerTestItem();
            var s = new TestItemSerializer(i);

            Assert.IsFalse(s.SerializerCalled);
            s.Serialize();
            Assert.IsTrue(s.SerializerCalled);

            Assert.IsFalse(s.DeserializerCalled);
            s.Deserialize();
            Assert.IsTrue(s.DeserializerCalled);

            s.SerializerCalled = false;
            s.DeserializerCalled = false;


            s.IsEnabled = false;
            Assert.IsFalse(s.SerializerCalled);
            s.Serialize();
            Assert.IsFalse(s.SerializerCalled);

            Assert.IsFalse(s.DeserializerCalled);
            s.Deserialize();
            Assert.IsFalse(s.DeserializerCalled);

            s.SerializerCalled = false;
            s.DeserializerCalled = false;


            s.IsEnabled = true;
            Assert.IsFalse(s.SerializerCalled);
            s.Serialize();
            Assert.IsTrue(s.SerializerCalled);

            Assert.IsFalse(s.DeserializerCalled);
            s.Deserialize();
            Assert.IsTrue(s.DeserializerCalled);



            s.Dispose();
            i.Dispose();
        }
    }
}
