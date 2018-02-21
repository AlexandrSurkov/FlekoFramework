using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Flekosoft.Common;
using Flekosoft.Common.Collection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.Collection
{
    class DictionaryItemTestClass : PropertyChangedErrorNotifyDisposableBase
    {
        private int _prop;

        public int Prop
        {
            get => _prop;
            set
            {
                if (_prop != value)
                {
                    _prop = value;
                    OnPropertyChanged(nameof(Prop));
                }
            }
        }
    }

    [TestClass]
    public class DictionaryCollectionTests
    {
        private NotifyCollectionChangedEventArgs _notifyCollectionChangedEventArgs;
        private PropertyChangedEventArgs _propertyChangedEventArgs;

        [TestMethod]
        public void CollectionBaseTest()
        {
            var name = "TestCollection";
            DictionaryCollection<int, DictionaryItemTestClass> collection = new DictionaryCollection<int, DictionaryItemTestClass>(name, true);
            collection.CollectionChanged += Collection_CollectionChanged;
            collection.PropertyChanged += Collection_PropertyChanged;

            Assert.AreEqual(name, collection.InstanceName);
            Assert.AreEqual(name, collection.ToString());

            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;
            Assert.AreEqual(0, collection.Count);
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            Assert.IsNull(_propertyChangedEventArgs);
            var item = new DictionaryItemTestClass();
            collection.Add(0, item);
            Assert.IsNull(_propertyChangedEventArgs);
            Assert.IsNotNull(_notifyCollectionChangedEventArgs);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, _notifyCollectionChangedEventArgs.Action);
            Assert.AreEqual(item, _notifyCollectionChangedEventArgs.NewItems[0]);
            Assert.AreEqual(1, collection.Count);
            for (int i = 0; i < 10; i++)
            {
                _notifyCollectionChangedEventArgs = null;
                _propertyChangedEventArgs = null;
                item = new DictionaryItemTestClass() { Prop = i + 1 };
                Assert.IsNull(_notifyCollectionChangedEventArgs);
                collection.Add(i + 1, item);
                Assert.IsNull(_propertyChangedEventArgs);
                Assert.IsNotNull(_notifyCollectionChangedEventArgs);
                Assert.AreEqual(NotifyCollectionChangedAction.Add, _notifyCollectionChangedEventArgs.Action);
                Assert.AreEqual(item, _notifyCollectionChangedEventArgs.NewItems[0]);
                Assert.IsNull(_propertyChangedEventArgs);
            }
            Assert.AreEqual(11, collection.Count);



            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            Assert.IsNull(_propertyChangedEventArgs);
            collection.Remove(collection.KeyOf(item));
            Assert.IsNull(_propertyChangedEventArgs);
            Assert.IsNotNull(_notifyCollectionChangedEventArgs);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, _notifyCollectionChangedEventArgs.Action);
            Assert.AreEqual(item, _notifyCollectionChangedEventArgs.OldItems[0]);
            Assert.AreEqual(10, collection.Count);

            collection.Dispose();
            Assert.AreEqual(true, collection.IsDisposed);
            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void CollectionCollectionChangedEventTest()
        {
            var name = "TestCollection";
            DictionaryCollection<int, DictionaryItemTestClass> collection = new DictionaryCollection<int, DictionaryItemTestClass>(name, true);
            collection.CollectionChanged += Collection_CollectionChanged;
            collection.PropertyChanged += Collection_PropertyChanged;

            //Add item
            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            var item = new DictionaryItemTestClass();
            collection.Add(0, item);
            Assert.IsNotNull(_notifyCollectionChangedEventArgs);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, _notifyCollectionChangedEventArgs.Action);
            Assert.AreEqual(item, _notifyCollectionChangedEventArgs.NewItems[0]);
            Assert.AreEqual(1, collection.Count);
            Assert.IsNull(_propertyChangedEventArgs);


            _notifyCollectionChangedEventArgs = null;
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            item.Prop = item.Prop + 1;
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            Assert.IsNotNull(_propertyChangedEventArgs);
            Assert.AreEqual("Collection[0].Prop", _propertyChangedEventArgs.PropertyName);


            //Add one new item
            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            item = new DictionaryItemTestClass();
            collection.Add(1, item);
            Assert.IsNotNull(_notifyCollectionChangedEventArgs);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, _notifyCollectionChangedEventArgs.Action);
            Assert.AreEqual(item, _notifyCollectionChangedEventArgs.NewItems[0]);
            Assert.AreEqual(2, collection.Count);
            Assert.IsNull(_propertyChangedEventArgs);


            _notifyCollectionChangedEventArgs = null;
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            item.Prop = item.Prop + 1;
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            Assert.IsNotNull(_propertyChangedEventArgs);
            Assert.AreEqual("Collection[1].Prop", _propertyChangedEventArgs.PropertyName);

            //Add one new item
            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            item = new DictionaryItemTestClass();
            collection.Add(2, item);
            Assert.IsNotNull(_notifyCollectionChangedEventArgs);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, _notifyCollectionChangedEventArgs.Action);
            Assert.AreEqual(item, _notifyCollectionChangedEventArgs.NewItems[0]);
            Assert.AreEqual(3, collection.Count);
            Assert.IsNull(_propertyChangedEventArgs);


            _notifyCollectionChangedEventArgs = null;
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            item.Prop = item.Prop + 1;
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            Assert.IsNotNull(_propertyChangedEventArgs);
            Assert.AreEqual("Collection[2].Prop", _propertyChangedEventArgs.PropertyName);

            //RemoveItem
            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            Assert.IsNull(_propertyChangedEventArgs);
            collection.Remove(collection.KeyOf(item));
            Assert.IsNull(_propertyChangedEventArgs);
            Assert.IsNotNull(_notifyCollectionChangedEventArgs);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, _notifyCollectionChangedEventArgs.Action);
            Assert.AreEqual(item, _notifyCollectionChangedEventArgs.OldItems[0]);
            Assert.AreEqual(2, collection.Count);
            Assert.IsTrue(item.IsDisposed);

            _notifyCollectionChangedEventArgs = null;
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            item.Prop = item.Prop + 1;
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            Assert.IsNull(_propertyChangedEventArgs);

            //Clear
            _notifyCollectionChangedEventArgs = null;
            _propertyChangedEventArgs = null;
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            Assert.IsNull(_propertyChangedEventArgs);
            item = collection[0];
            collection.Clear();
            Assert.IsNull(_propertyChangedEventArgs);
            Assert.IsNotNull(_notifyCollectionChangedEventArgs);
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, _notifyCollectionChangedEventArgs.Action);
            Assert.IsNull(_notifyCollectionChangedEventArgs.NewItems);
            Assert.IsNull(_notifyCollectionChangedEventArgs.OldItems);
            Assert.AreEqual(0, collection.Count);
            Assert.IsTrue(item.IsDisposed);

            _notifyCollectionChangedEventArgs = null;
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            item.Prop = item.Prop + 1;
            Assert.IsNull(_notifyCollectionChangedEventArgs);
            Assert.IsNull(_propertyChangedEventArgs);

            Assert.IsNull(_notifyCollectionChangedEventArgs);
            Assert.IsNull(_propertyChangedEventArgs);
            collection.Dispose();
            Assert.IsNull(_propertyChangedEventArgs);
            Assert.IsNull(_notifyCollectionChangedEventArgs);

            Assert.AreEqual(true, collection.IsDisposed);
        }

        [TestMethod]
        public void EnumirationTest()
        {
            var name = "TestCollection";
            DictionaryCollection<int, DictionaryItemTestClass> collection = new DictionaryCollection<int, DictionaryItemTestClass>(name, true);
            collection.CollectionChanged += Collection_CollectionChanged;

            List<DictionaryItemTestClass> items = new List<DictionaryItemTestClass>();

            for (int i = 0; i < 10; i++)
            {
                var item = new DictionaryItemTestClass() { Prop = i + 1 };
                collection.Add(i, item);
                items.Add(item);
            }

            var index = 0;
            foreach (KeyValuePair<int, DictionaryItemTestClass> item in collection)
            {
                Assert.AreEqual(index, item.Key);
                Assert.AreEqual(items[index], item.Value);
                index++;
            }

            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(collection.ContainsKey(i));
                Assert.IsTrue(collection.ContainsValue(items[i]));
                Assert.AreEqual(items[i], collection[i]);
                Assert.AreEqual(i, collection.KeyOf(items[i]));
            }

            var ro = collection.AsReadOnly();

            index = 0;
            foreach (DictionaryItemTestClass item in ro)
            {
                Assert.AreEqual(items[index], item);
                index++;
            }

            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(items[i], ro[i]);
            }


            var testItem = new DictionaryItemTestClass() { Prop = 101 };
            Assert.IsFalse(collection.ContainsValue(testItem));

            collection.Dispose();


        }

        [TestMethod]
        public void DisposeTest()
        {
            var name = "TestCollection";
            DictionaryCollection<int, DictionaryItemTestClass> collection = new DictionaryCollection<int, DictionaryItemTestClass>(name, true);
            collection.CollectionChanged += Collection_CollectionChanged;

            List<DictionaryItemTestClass> items = new List<DictionaryItemTestClass>();

            for (int i = 0; i < 10; i++)
            {
                var item = new DictionaryItemTestClass() { Prop = i + 1 };
                collection.Add(i, item);
                items.Add(item);
            }

            collection.Dispose();

            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(items[i].IsDisposed);
            }

        }


        [TestMethod]
        public void ItemDisposeTest()
        {
            // Items are disposing in remove tests
            var name = "TestCollection";
            DictionaryCollection<int, DictionaryItemTestClass> collection = new DictionaryCollection<int, DictionaryItemTestClass>(name, true);
            collection.CollectionChanged += Collection_CollectionChanged;

            List<DictionaryItemTestClass> items = new List<DictionaryItemTestClass>();

            for (int i = 0; i < 10; i++)
            {
                var item = new DictionaryItemTestClass() { Prop = i + 1 };
                collection.Add(i, item);
                items.Add(item);
            }

            var di = collection[0];
            collection.Remove(0);
            Assert.IsTrue(di.IsDisposed);

            collection.Dispose();

            for (int i = 0; i < 9; i++)
            {
                Assert.IsTrue(items[i].IsDisposed);
            }

            // Item are NOT disposing in remove tests
            collection = new DictionaryCollection<int, DictionaryItemTestClass>(name, false);
            collection.CollectionChanged += Collection_CollectionChanged;

            items = new List<DictionaryItemTestClass>();

            for (int i = 0; i < 10; i++)
            {
                var item = new DictionaryItemTestClass() { Prop = i + 1 };
                collection.Add(i, item);
                items.Add(item);
            }

            di = collection[0];
            collection.Remove(0);
            Assert.IsFalse(di.IsDisposed);

            collection.Dispose();

            for (int i = 0; i < 9; i++)
            {
                Assert.IsFalse(items[i].IsDisposed);
            }

        }

        private void Collection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _propertyChangedEventArgs = e;
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _notifyCollectionChangedEventArgs = e;
        }
    }
}
