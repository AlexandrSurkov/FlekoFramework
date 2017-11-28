using Flekosoft.Common.Collection;

namespace Flekosoft.Common.Serialization
{
    public abstract class CollectionSerializer
    {
        protected CollectionBase Collection { get; }

        protected CollectionSerializer(CollectionBase collection)
        {
            Collection = collection;
            Collection.CollectionChanged += Collection_CollectionChanged;
        }

        private void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Serialize();
        }

        public void Serialize()
        {
            InternalSerialize();
        }

        public void Deserialize()
        {
            Collection.CollectionChanged -= Collection_CollectionChanged;
            InternalDeserialize();
            Collection.CollectionChanged += Collection_CollectionChanged;
        }

        public abstract void InternalSerialize();

        public abstract void InternalDeserialize();
    }
}
