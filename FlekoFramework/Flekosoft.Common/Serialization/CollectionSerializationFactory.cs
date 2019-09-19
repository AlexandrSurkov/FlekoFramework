using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Flekosoft.Common.Collection;

namespace Flekosoft.Common.Serialization
{
    public abstract class CollectionSerializationFactory<TC, TV> : SerializationFactory<TV>
    { /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TC">type of collection</typeparam>
      /// <typeparam name="TV">type of collection element</typeparam>
      /// <param name="collection"></param>
        protected void AddCollection(CollectionBase collection)
        {
            if (IsDisposed) return;
            collection.Serializers.Add(GetCollectionSerializer(collection));
            collection.CollectionChanged += Collection_CollectionChanged<TV>;
        }

        private void Collection_CollectionChanged<T>(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDisposed) return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ISerializable item in e.NewItems)
                    {
                        var serializer = GetSerializer(item);
                        if (serializer != null) item.Serializers.Add(serializer);
                    }
                    break;
                default:
                    RemoveUnusedSerializers();
                    break;
            }
        }

        private void RemoveUnusedSerializers()
        {
            var removeList = new List<ISerializer>();
            foreach (ISerializer serializer in Serializers)
            {
                if (serializer is DisposableBase sd)
                {
                    if (sd.IsDisposed) removeList.Add(serializer);
                }
            }

            foreach (ISerializer serializer in removeList)
            {
                Serializers.Remove(serializer);
            }
        }

        private CollectionSerializer<TC> GetCollectionSerializer(ISerializable serializableObject)
        {
            try
            {
                var serializer = InternalGetCollectionSerializer(serializableObject);
                Serializers.Add(serializer);
                return serializer;
            }
            catch (Exception e)
            {
                OnErrorEvent(e);
                throw;
            }
        }

        protected abstract CollectionSerializer<TC> InternalGetCollectionSerializer(ISerializable serializableObject);
    }
}
