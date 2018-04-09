using System;
using System.IO;
using Flekosoft.Common.Serialization.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Serialization.Xml
{
    class TestXmlSerializer : SerializerXml<SerializerTestItem>
    {
        public TestXmlSerializer(SerializerTestItem serialisableObject, string rootName) : base(serialisableObject, rootName)
        {
        }

        public TestXmlSerializer(SerializerTestItem serialisableObject, string rootName, string storagePath, string fileName) : base(serialisableObject, rootName, storagePath, fileName)
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

        public bool CheckFile()
        {
            return CheckFileExist();
        }
    }

    [TestClass]
    public class SerializerXmlTests
    {
        [TestMethod]
        public void XmlTest()
        {
            var item = new SerializerTestItem();
            var text = "123sdf";
            var s = new TestXmlSerializer(item, text);

            Assert.IsFalse(s.StoreToFile);
            Assert.AreEqual(string.Empty, s.FileName);
            Assert.AreEqual(string.Empty, s.StoragePath);

            Assert.IsNotNull(s.XmlDocument);

            Assert.IsNotNull(s.XmlRoot);
            Assert.AreEqual(text, s.XmlRoot.Name);

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

            s.ClearSerializedData();
            Assert.AreEqual(0, s.XmlRoot.ChildNodes.Count);

            s.Dispose();

        }


        [TestMethod]
        public void FileTest()
        {
            var item = new SerializerTestItem();
            var text = "123sdf";
            var path = Path.GetTempPath();
            var filename = "filename";
            var s = new TestXmlSerializer(item, text, path, filename);

            Assert.AreEqual($"{filename}.xml", s.FileName);
            Assert.AreEqual(path, s.StoragePath);
            Assert.IsTrue(s.StoreToFile);

            var x = File.Exists(s.StoragePath + s.FileName);
            Assert.IsFalse(x);
            Assert.AreEqual(x, s.CheckFile());
            s.ClearSerializedData();

            var stream = File.Create(s.StoragePath + s.FileName);
            stream.Dispose();
            x = File.Exists(s.StoragePath + s.FileName);
            Assert.IsTrue(x);
            Assert.AreEqual(x, s.CheckFile());
            s.ClearSerializedData();
            x = File.Exists(s.StoragePath + s.FileName);
            Assert.IsFalse(x);

            s.Dispose();

            path = path.Remove(path.Length - 1, 1);
            s = new TestXmlSerializer(item, text, path, filename);
            Assert.AreEqual(path + Path.DirectorySeparatorChar, s.StoragePath);
            s.Dispose();

        }
    }
}
