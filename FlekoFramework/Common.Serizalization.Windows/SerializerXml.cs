using Flekosoft.Common.Serialization;
using System.Xml;

namespace Common.Serizalization.Windows
{
    public abstract class SerializerXml<T> : Serializer<T>
    {
        protected SerializerXml(T serialisableObject, XmlElement root) : base(serialisableObject)
        {
            Root = root;
        }
        protected XmlElement Root { get; }
    }
}
