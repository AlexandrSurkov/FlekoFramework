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

        protected virtual bool CheckPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Contains("Collection[")) return true;
            return false;
        }

        private void SerialisableObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (CheckPropertyChanged(sender, e)) Serialize();
        }

        private void SerialisableObject_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Serialize();
        }

        public override void Serialize()
        {
            InternalSerialize();
            AppendDebugMessage($"{_collection.CollectionName}: Serialized");
        }

        public override void Deserialize()
        {
            _collection.CollectionChanged -= SerialisableObject_CollectionChanged;
            _collection.PropertyChanged -= SerialisableObject_PropertyChanged;
            InternalDeserialize();
            _collection.CollectionChanged += SerialisableObject_CollectionChanged;
            _collection.PropertyChanged += SerialisableObject_PropertyChanged;
            AppendDebugMessage($"{_collection.CollectionName}: Deserialized");
        }

        public abstract void InternalSerialize();

        public abstract void InternalDeserialize();
    }
}
