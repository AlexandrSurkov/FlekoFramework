using System;
using Flekosoft.Common.Serialization.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Serialization.Xml
{
    class TestXmlSerializer : SerializerXml<SerializerTestItem>
    {
        public TestXmlSerializer(SerializerTestItem serialisableObject, string rootName) : base(serialisableObject, rootName)
        {
        }

        public override void InternalSerialize()
        {
            throw new NotImplementedException();
        }

        public override void InternalDeserialize()
        {
            throw new NotImplementedException();
        }

        public override void ClearSerializedData()
        {
            throw new NotImplementedException();
        }
    }

    [TestClass]
    public class SerializerXmlTests
    {
        [TestMethod]
        public void Test()
        {
            var item = new SerializerTestItem();
            var text = "123sdf";
            var s = new TestXmlSerializer(item,text);

            Assert.IsNotNull(s.XmlDocument);

            Assert.IsNotNull(s.XmlRoot);
            Assert.AreEqual(text,s.XmlRoot.Name);

            var element = s.XmlDocument.CreateElement("element1");
            s.XmlRoot.AppendChild(element);
            Assert.AreEqual(1, s.XmlRoot.ChildNodes.Count);

            element = s.XmlDocument.CreateElement("element2");
            s.XmlRoot.AppendChild(element);
            Assert.AreEqual(2, s.XmlRoot.ChildNodes.Count);

            s.XmlRoot.RemoveChild(element);
            Assert.AreEqual(1, s.XmlRoot.ChildNodes.Count);

            element = s.XmlDocument.CreateElement("element3");
            s.XmlRoot.AppendChild(element);
            Assert.AreEqual(2, s.XmlRoot.ChildNodes.Count);

            s.XmlRoot.RemoveAll();
            Assert.AreEqual(0, s.XmlRoot.ChildNodes.Count);

            s.Dispose();

        }
    }
}
