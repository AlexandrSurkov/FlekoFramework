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
        }

        private readonly CollectionBase _collection;

        protected override bool CheckPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Contains("Collection[")) return true;
            return false;
        }

        private void SerialisableObject_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (IsDisposed) return;
            Serialize();
        }

        public override void Deserialize()
        {
            if (IsDisposed) return;
            _collection.CollectionChanged -= SerialisableObject_CollectionChanged;
            InternalDeserialize();
            _collection.CollectionChanged += SerialisableObject_CollectionChanged;
        }
    }
}
