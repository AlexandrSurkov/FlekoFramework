using System;

namespace Flekosoft.Common.Serialization
{
    public abstract class SerializationFactory<TV> : ErrorNotifyDisposableBase
    {

        protected Serializer<TV> GetSerializer(ISerializable serializableObject)
        {
            try
            {
                var serializer = GetInternalSerializer(serializableObject);
                Serializers.Add(serializer);
                return serializer;
            }
            catch (Exception e)
            {
                OnErrorEvent(e);
                throw;
            }
        }

        protected abstract Serializer<TV> GetInternalSerializer(ISerializable serializableObject);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (ISerializer serializer in Serializers)
                {
                    if (serializer is IDisposable sd)
                    {
                        sd.Dispose();
                    }
                }
            }
            base.Dispose(disposing);
        }
    }
}
