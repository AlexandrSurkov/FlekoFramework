using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flekosoft.Common.Serialization
{
    public abstract class Serializer<T>:ISerializer
    {
        protected Serializer(T serialisableObject)
        {
            if(!(serialisableObject is ISerializabe)) throw new ArgumentException("serialisableObject must implement ISerializabe");
            SerialisableObject = serialisableObject;
        }

        protected T SerialisableObject { get;}
        public abstract void Serialize();
        public abstract void Deserialize();
        public abstract void ClearSerializedData();
    }
}
