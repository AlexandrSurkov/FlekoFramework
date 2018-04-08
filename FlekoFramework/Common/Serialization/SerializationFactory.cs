using System;

namespace Flekosoft.Common.Serialization
{
    public abstract class SerializationFactory<TV> : ErrorNotifyDisposableBase
    {

        protected Serializer<TV> GetSerializer(ISerializabe serialisableObject)
        {
            try
            {
                var serializer = GetInternalSerializer(serialisableObject);
                Serializers.Add(serializer);
                return serializer;
            }
            catch (Exception e)
            {
                OnErrorEvent(e);
                throw;
            }
        }

        protected abstract Serializer<TV> GetInternalSerializer(ISerializabe serialisableObject);

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
