using System.Globalization;
using System.IO;
using System.Xml;
using Flekosoft.Common.Serialization.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Serialization.Xml
{
    class CollectionTestXmlSerializer : CollectionSerializerXml<SerializerTestCollection>
    {
        public CollectionTestXmlSerializer(SerializerTestCollection serialisableObject, string rootName) : base(serialisableObject, rootName)
        {
        }
        public CollectionTestXmlSerializer(SerializerTestCollection serialisableObject, string rootName, string storagePath, string fileName) : base(serialisableObject, rootName, storagePath, fileName)
        {
        }

        public override void InternalSerialize()
        {
            foreach (SerializerTestItem item in SerialisableObject.AsReadOnly())
            {
                var node = XmlDocument.CreateElement(nameof(SerializerTestItem));
                node.InnerText = item.Prop.ToString(CultureInfo.InvariantCulture);
                XmlRoot.AppendChild(node);
            }
        }

        public override void InternalDeserialize()
        {
            foreach (XmlElement node in XmlRoot.ChildNodes)
            {
                if (node.Name == nameof(SerializerTestItem))
                {
                    if (int.TryParse(node.InnerText, out var value))
                    {
                        SerialisableObject.Add(new SerializerTestItem { Prop = value });
                    }
                }
            }
        }

        public bool CheckFile()
        {
            return CheckFileExist();
        }
    }

    [TestClass]
    public class CollectionSerializerXmlTests
    {
        [TestMethod]
        public void XmlTest()
        {
            var item = new SerializerTestCollection();
            var text = "123sdf";
            var s = new CollectionTestXmlSerializer(item, text);

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
            var item = new SerializerTestCollection();
            var text = "123sdf";
            var path = Path.GetTempPath();
            var filename = "filename";
            var s = new CollectionTestXmlSerializer(item, text, path, filename);

            Assert.AreEqual($"{filename}.xml", s.FileName);
            Assert.AreEqual(path, s.StoragePath);
            Assert.IsTrue(s.StoreToFile);

            var x = File.Exists(s.StoragePath + s.FileName);
            Assert.IsFalse(x);
            Assert.AreEqual(false, s.CheckFile());
            s.ClearSerializedData();

            var stream = File.Create(s.StoragePath + s.FileName);
            stream.Dispose();
            x = File.Exists(s.StoragePath + s.FileName);
            Assert.IsTrue(x);
            Assert.AreEqual(true, s.CheckFile());
            s.ClearSerializedData();
            x = File.Exists(s.StoragePath + s.FileName);
            Assert.IsFalse(x);

            s.Dispose();

            path = path.Remove(path.Length - 1, 1);
            s = new CollectionTestXmlSerializer(item, text, path, filename);
            Assert.AreEqual(path + Path.DirectorySeparatorChar, s.StoragePath);
            s.Dispose();
        }


        [TestMethod]
        public void FileSerialize_DeserializeTest()
        {
            var collection = new SerializerTestCollection();
            var text = "asd";
            var path = Path.GetTempPath();
            var filename = "filename";

            var s = new CollectionTestXmlSerializer(collection, text, path, filename);

            int val = 123;
            Assert.AreEqual(0, collection.Count);
            collection.Add(new SerializerTestItem() {Prop = val});
            s.Dispose();

            collection.Clear();

            s = new CollectionTestXmlSerializer(collection, text, path, filename);
            s.Deserialize();

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(val,collection[0].Prop);
            s.ClearSerializedData();

            s.Dispose();
        }
    }
}
