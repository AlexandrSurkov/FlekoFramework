using Flekosoft.Common.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Serialization
{
    class TestSerializationFactory : CollectionSerializationFactory<SerializerTestCollection, SerializerTestItem>
    {
        public TestSerializationFactory(SerializerTestCollection c)
        {
            AddCollection(c);
        }

        protected override Serializer<SerializerTestItem> GetInternalSerializer(ISerializabe serialisableObject)
        {
            if (serialisableObject is SerializerTestItem) return new TestItemSerializer((SerializerTestItem)serialisableObject);
            return null;
        }

        protected override CollectionSerializer<SerializerTestCollection> InternalGetCollectionSerializer(ISerializabe serialisableObject)
        {
            if (serialisableObject is SerializerTestCollection) return new TestCollectionSerializer((SerializerTestCollection)serialisableObject);
            return null;
        }
    }
    [TestClass]
    public class SerializationFactoryTests
    {
        [TestMethod]
        public void Test()
        {

            var c = new SerializerTestCollection();
            Assert.AreEqual(0, c.Serializers.Count);
            var f = new TestSerializationFactory(c);
            Assert.AreEqual(1, c.Serializers.Count);
            Assert.IsInstanceOfType(c.Serializers[0], typeof(TestCollectionSerializer));

            var i = new SerializerTestItem();
            Assert.AreEqual(0, i.Serializers.Count);
            c.Add(i);
            Assert.AreEqual(1, i.Serializers.Count);
            Assert.IsInstanceOfType(i.Serializers[0], typeof(TestItemSerializer));
            TestItemSerializer s = (TestItemSerializer) i.Serializers[0];
            c.Remove(i);
            Assert.IsTrue(s.IsDisposed);
            Assert.AreEqual(0, i.Serializers.Count);

            i = new SerializerTestItem();
            c.Add(i);
            Assert.AreEqual(1, i.Serializers.Count);
            Assert.IsInstanceOfType(i.Serializers[0], typeof(TestItemSerializer));
            s = (TestItemSerializer)i.Serializers[0];

            f.Dispose();
            Assert.IsTrue(s.IsDisposed);


        }
    }
}
