using System;
using Flekosoft.Common.Collection;

namespace Flekosoft.Common.Serialization
{
    public abstract class CollectionSerializer<T> : Serializer<T>
    {
        protected CollectionSerializer(T serialisableObject) : base(serialisableObject)
        {
            _collection = serialisableObject as CollectionBase;
            if (_collection == null) throw new ArgumentException("serialisableObject must be nested from CollectionBase");

            _collection.CollectionChanged += SerialisableObject_CollectionChanged;
            _collection.PropertyChanged += SerialisableObject_PropertyChanged;
        }

        private readonly CollectionBase _collection;

        protected abstract bool CheckPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e);

        private void SerialisableObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (CheckPropertyChanged(e)) Serialize();
        }

        private void SerialisableObject_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Serialize();
        }

        public override void Serialize()
        {
            InternalSerialize();
        }

        public override void Deserialize()
        {
            _collection.CollectionChanged -= SerialisableObject_CollectionChanged;
            _collection.PropertyChanged -= SerialisableObject_PropertyChanged;
            InternalDeserialize();
            _collection.CollectionChanged += SerialisableObject_CollectionChanged;
            _collection.PropertyChanged += SerialisableObject_PropertyChanged;
        }

        public abstract void InternalSerialize();

        public abstract void InternalDeserialize();
    }
}
