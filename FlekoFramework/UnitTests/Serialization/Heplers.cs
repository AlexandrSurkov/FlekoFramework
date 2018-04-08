using System.Collections.Generic;
using System.ComponentModel;
using Flekosoft.Common;
using Flekosoft.Common.Collection;
using Flekosoft.Common.Serialization;

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

    class TestCollectionSerializer : CollectionSerializer<SerializerTestCollection>
    {
        public bool SerializerCalled { get; set; }
        public bool DeserializerCalled { get; set; }

        public bool DefaultCheckPropertyChanged { get; set; }

        readonly List<int> _list = new List<int>();
        public TestCollectionSerializer(SerializerTestCollection serialisableObject) : base(serialisableObject)
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

    class TestItemSerializer : Serializer<SerializerTestItem>
    {
        public bool SerializerCalled { get; set; }
        public bool DeserializerCalled { get; set; }
        public bool ClearCalled { get; set; }

        public bool DefaultCheckPropertyChanged { get; set; }

        public TestItemSerializer(SerializerTestItem item) : base(item)
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
            ClearCalled = true;
        }

        public override void InternalSerialize()
        {
            SerializerCalled = true;
        }

        public override void InternalDeserialize()
        {
            DeserializerCalled = true;
        }
    }

}
