using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Flekosoft.Common;
using Flekosoft.Common.Collection;
using Flekosoft.Common.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Serialization
{
    class SerializerTestItem : PropertyChangedErrorNotifyDisposableBase
    {
        private int _prop;

        public int Prop
        {
            get => _prop;
            set
            {
                if (Prop != value)
                {
                    _prop = value;
                    OnPropertyChanged(nameof(Prop));
                }
            }
        }
    }

    class SerializerTestCollection : ListCollection<SerializerTestItem>
    {
        public SerializerTestCollection() : base("SerializerTestCollection", true)
        {
        }

        private int _prop;

        public int Prop
        {
            get => _prop;
            set
            {
                if (Prop != value)
                {
                    _prop = value;
                    OnPropertyChanged(nameof(Prop));
                }
            }
        }
    }

    class TestSerializer : CollectionSerializer<SerializerTestCollection>
    {
        public bool SerializerCalled { get; set; }
        public bool DeserializerCalled { get; set; }

        public bool DefaultCheckPropertyChanged { get; set; }

        readonly List<int> _list = new List<int>();
        public TestSerializer(SerializerTestCollection serialisableObject) : base(serialisableObject)
        {
        }

        protected override bool CheckPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (DefaultCheckPropertyChanged)
                return base.CheckPropertyChanged(sender, e);
            return true;
        }

        public override void ClearSerializedData()
        {
            _list.Clear();
        }

        public override void InternalSerialize()
        {
            SerializerCalled = true;
            foreach (SerializerTestItem item in SerialisableObject)
            {
                _list.Add(item.Prop);
            }
        }

        public override void InternalDeserialize()
        {
            DeserializerCalled = true;
            foreach (int i in _list)
            {
                SerialisableObject.Add(new SerializerTestItem() { Prop = i });
            }
        }
    }

    [TestClass]
    public class CollectionSerializerTests
    {
        private NotifyCollectionChangedEventArgs _notifyCollectionChangedEventArgs;
        private PropertyChangedEventArgs _propertyChangedEventArgs;

        [TestMethod]
        public void SerializeDeserializeTest()
        {
            var collection = new SerializerTestCollection();
            collection.PropertyChanged += Collection_PropertyChanged;
            collection.CollectionChanged += Collection_CollectionChanged;

            for (int i = 0; i < 10; i++)
            {
                collection.Add(new SerializerTestItem { Prop = i });
            }

            var serializer = new TestSerializer(collection);

            var collectionLen = collection.Count;

            serializer.SerializerCalled = false;
            serializer.DeserializerCalled = false;
            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;

            collection.Serializers.Add(serializer);

            //Test Empty deserialize

            Assert.IsFalse(serializer.SerializerCalled);
            Assert.IsFalse(serializer.DeserializerCalled);
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            Assert.IsNull(_propertyChangedEventArgs);

            collection.Serializers[0].Deserialize();

            Assert.IsFalse(serializer.SerializerCalled);
            Assert.IsTrue(serializer.DeserializerCalled);
            Assert.AreEqual(collectionLen, collection.Count);
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            Assert.IsNull(_propertyChangedEventArgs);


            //TestSerialize
            serializer.SerializerCalled = false;
            serializer.DeserializerCalled = false;
            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;

            collection.Serializers[0].Serialize();

            Assert.IsTrue(serializer.SerializerCalled);
            Assert.IsFalse(serializer.DeserializerCalled);
            Assert.AreEqual(collectionLen, collection.Count);
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            Assert.IsNull(_propertyChangedEventArgs);

            collection.Clear();
            Assert.AreEqual(0, collection.Count);

            //TestDeserialize
            serializer.SerializerCalled = false;
            serializer.DeserializerCalled = false;
            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;

            collection.Serializers[0].Deserialize();

            Assert.IsFalse(serializer.SerializerCalled);
            Assert.IsTrue(serializer.DeserializerCalled);
            Assert.AreEqual(collectionLen, collection.Count);
            Assert.IsNotNull(_notifyCollectionChangedEventArgs);
            Assert.IsNull(_propertyChangedEventArgs);

            Assert.AreEqual(10, collection.Count);

            for (int i = 0; i < 10; i++)
            {
                collection[i].Prop = i;
            }

            serializer.Dispose();

        }

        [TestMethod]
        public void SerializeOnCollectionChangedTest()
        {
            var collection = new SerializerTestCollection();
            collection.PropertyChanged += Collection_PropertyChanged;
            collection.CollectionChanged += Collection_CollectionChanged;

            var serializer = new TestSerializer(collection);
            collection.Serializers.Add(serializer);

            //Test on Add
            serializer.SerializerCalled = false;
            serializer.DeserializerCalled = false;
            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;

            for (int i = 0; i < 10; i++)
            {
                serializer.SerializerCalled = false;
                serializer.DeserializerCalled = false;
                _notifyCollectionChangedEventArgs = null;
                _propertyChangedEventArgs = null;

                collection.Add(new SerializerTestItem { Prop = i });

                Assert.IsTrue(serializer.SerializerCalled);
                Assert.IsFalse(serializer.DeserializerCalled);
                Assert.IsNotNull(_notifyCollectionChangedEventArgs);
                Assert.IsNull(_propertyChangedEventArgs);
            }

            //Test on RemoveAt
            for (int i = 0; i < 10; i++)
            {
                serializer.SerializerCalled = false;
                serializer.DeserializerCalled = false;
                _notifyCollectionChangedEventArgs = null;
                _propertyChangedEventArgs = null;

                collection.RemoveAt(0);

                Assert.IsTrue(serializer.SerializerCalled);
                Assert.IsFalse(serializer.DeserializerCalled);
                Assert.IsNotNull(_notifyCollectionChangedEventArgs);
                Assert.IsNull(_propertyChangedEventArgs);
            }

            //Test on Remove
            for (int i = 0; i < 10; i++)
            {
                serializer.SerializerCalled = false;
                serializer.DeserializerCalled = false;
                _notifyCollectionChangedEventArgs = null;
                _propertyChangedEventArgs = null;

                collection.Add(new SerializerTestItem { Prop = i });

                Assert.IsTrue(serializer.SerializerCalled);
                Assert.IsFalse(serializer.DeserializerCalled);
                Assert.IsNotNull(_notifyCollectionChangedEventArgs);
                Assert.IsNull(_propertyChangedEventArgs);
            }

            for (int i = 0; i < 10; i++)
            {
                serializer.SerializerCalled = false;
                serializer.DeserializerCalled = false;
                _notifyCollectionChangedEventArgs = null;
                _propertyChangedEventArgs = null;

                collection.Remove(collection[0]);

                Assert.IsTrue(serializer.SerializerCalled);
                Assert.IsFalse(serializer.DeserializerCalled);
                Assert.IsNotNull(_notifyCollectionChangedEventArgs);
                Assert.IsNull(_propertyChangedEventArgs);
            }

            //Test on Clear
            for (int i = 0; i < 10; i++)
            {
                serializer.SerializerCalled = false;
                serializer.DeserializerCalled = false;
                _notifyCollectionChangedEventArgs = null;
                _propertyChangedEventArgs = null;

                collection.Add(new SerializerTestItem { Prop = i });

                Assert.IsTrue(serializer.SerializerCalled);
                Assert.IsFalse(serializer.DeserializerCalled);
                Assert.IsNotNull(_notifyCollectionChangedEventArgs);
                Assert.IsNull(_propertyChangedEventArgs);
            }

            serializer.SerializerCalled = false;
            serializer.DeserializerCalled = false;
            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;

            collection.Clear();

            Assert.IsTrue(serializer.SerializerCalled);
            Assert.IsFalse(serializer.DeserializerCalled);
            Assert.IsNotNull(_notifyCollectionChangedEventArgs);
            Assert.IsNull(_propertyChangedEventArgs);

            //Test on Dispose
            for (int i = 0; i < 10; i++)
            {
                serializer.SerializerCalled = false;
                serializer.DeserializerCalled = false;
                _notifyCollectionChangedEventArgs = null;
                _propertyChangedEventArgs = null;

                collection.Add(new SerializerTestItem { Prop = i });

                Assert.IsTrue(serializer.SerializerCalled);
                Assert.IsFalse(serializer.DeserializerCalled);
                Assert.IsNotNull(_notifyCollectionChangedEventArgs);
                Assert.IsNull(_propertyChangedEventArgs);
            }

            serializer.SerializerCalled = false;
            serializer.DeserializerCalled = false;
            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;

            collection.Dispose();

            Assert.IsFalse(serializer.SerializerCalled);
            Assert.IsFalse(serializer.DeserializerCalled);
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            Assert.IsNull(_propertyChangedEventArgs);
        }

        [TestMethod]
        public void SerializeOnPropertyChangedTest()
        {
            var collection = new SerializerTestCollection();
            collection.PropertyChanged += Collection_PropertyChanged;
            collection.CollectionChanged += Collection_CollectionChanged;

            var serializer = new TestSerializer(collection);
            collection.Serializers.Add(serializer);

            //Test on Add
            serializer.SerializerCalled = false;
            serializer.DeserializerCalled = false;
            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;

            serializer.SerializerCalled = false;
            serializer.DeserializerCalled = false;
            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;

            var item = new SerializerTestItem { Prop = 1 };
            collection.Add(item);

            Assert.IsTrue(serializer.SerializerCalled);
            Assert.IsFalse(serializer.DeserializerCalled);
            Assert.IsNotNull(_notifyCollectionChangedEventArgs);
            Assert.IsNull(_propertyChangedEventArgs);


            //Collection Property change test
            serializer.SerializerCalled = false;
            serializer.DeserializerCalled = false;
            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;

            collection.Prop++;

            Assert.IsTrue(serializer.SerializerCalled);
            Assert.IsFalse(serializer.DeserializerCalled);
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            Assert.IsNotNull(_propertyChangedEventArgs);


            //Item Property change test - must not be serialized
            serializer.DefaultCheckPropertyChanged = true;
            serializer.SerializerCalled = false;
            serializer.DeserializerCalled = false;
            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;

            item.Prop++;

            Assert.IsFalse(serializer.SerializerCalled);
            Assert.IsFalse(serializer.DeserializerCalled);
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            Assert.IsNotNull(_propertyChangedEventArgs);

            serializer.DefaultCheckPropertyChanged = false;
            serializer.SerializerCalled = false;
            serializer.DeserializerCalled = false;
            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;

            item.Prop++;

            Assert.IsTrue(serializer.SerializerCalled);
            Assert.IsFalse(serializer.DeserializerCalled);
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            Assert.IsNotNull(_propertyChangedEventArgs);
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _notifyCollectionChangedEventArgs = e;
        }

        private void Collection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _propertyChangedEventArgs = e;
        }
    }
}
